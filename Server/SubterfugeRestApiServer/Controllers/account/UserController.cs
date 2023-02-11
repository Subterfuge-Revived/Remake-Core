using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace SubterfugeRestApiServer;

[ApiController]
[Route("api/user/")]
public class UserController : ControllerBase, ISubterfugeAccountApi
{
    
    private readonly IDatabaseCollection<DbUserModel> _dbUsers;
    private readonly IDatabaseCollection<DbChatMessage> _dbChatMessages;
    private readonly IConfiguration _config;
    public readonly TwilioConfig _twilioConfig;

    public UserController(IConfiguration configuration, IDatabaseCollectionProvider mongo)
    {
        _config = configuration;
        _dbUsers = mongo.GetCollection<DbUserModel>();
        _dbChatMessages = mongo.GetCollection<DbChatMessage>();

        _twilioConfig = new TwilioConfig(_config.GetSection("PhoneVerification"));
        
        // Check to send the user an SMS
        if (_twilioConfig.enabled)
        {
            TwilioClient.Init(_twilioConfig.twilioAccountSid, _twilioConfig.twilioAuthToken);
        }
    }


    [AllowAnonymous]
    [HttpPost]
    [Route("login")]
    public async Task<AuthorizationResponse> Login(AuthorizationRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        
        var user = await AuthenticateUserByPassword(request);
        if (user == null)
            throw new UnauthorizedException();
        
        if (dbUserModel != null && dbUserModel.Username != request.Username)
        {
            await FlagAccountDuplicate(user, dbUserModel, MultiBoxReason.LoginWithCredentialsForAnotherAccount);
        }

        
        var tokenString = GenerateJsonWebToken(user);
        return new AuthorizationResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Token = tokenString,
            User = user.ToUser(),
        };
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public async Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest registrationRequeset)
    {
        List<DbUserModel> usersMatchingUsernamePhoneOrDeviceId = await _dbUsers.Query()
            .Where(it => it.Username == registrationRequeset.Username || it.PhoneNumber == registrationRequeset.PhoneNumber || it.DeviceIdentifier == registrationRequeset.DeviceIdentifier)
            .ToListAsync();

        DbUserModel? matchingUsername = usersMatchingUsernamePhoneOrDeviceId.FirstOrDefault(it => it.Username == registrationRequeset.Username);
        if (matchingUsername != null)
            throw new ConflictException("There is already an account registered on this device.");

        // Create the new user account
        DbUserModel model = DbUserModel.FromRegistrationRequest(registrationRequeset);
        
        var matchingPhone = usersMatchingUsernamePhoneOrDeviceId.Where(it => it.PhoneNumber == registrationRequeset.PhoneNumber).ToList();
        // Flag all accounts matching the phone number as a possible multibox.
        foreach(DbUserModel user in matchingPhone)
        {
            await FlagAccountDuplicate(user, model, MultiBoxReason.DuplicatePhoneNumber);
        }
        
        var matchingDeviceId = usersMatchingUsernamePhoneOrDeviceId.Where(it => it.DeviceIdentifier == registrationRequeset.DeviceIdentifier).ToList();
        // Flag all accounts matching the device Id number as a possible multibox.
        foreach(DbUserModel user in matchingDeviceId)
        {
            await FlagAccountDuplicate(user, model, MultiBoxReason.DuplicateDeviceId);
        }
        
        // Check to send the user an SMS
        if (_twilioConfig.enabled)
        {
            var verification = VerificationResource.Create(
                to: registrationRequeset.PhoneNumber,
                channel: "sms",
                pathServiceSid: _twilioConfig.twilioVerificationServiceSid
            );
        }
        else
        {
            // If phone validation is disabled, mark their account as being verified
            var claimCopy = model.Claims.ToList();
            claimCopy.Add(UserClaim.PhoneVerified);
            model.Claims = claimCopy.ToArray();
        }
            
        // Create a new user model
        await _dbUsers.Upsert(model);
        
        string token = GenerateJsonWebToken(model);
        return new AccountRegistrationResponse
        {
            Token = token,
            User = model.ToUser(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            RequirePhoneValidation = _twilioConfig.enabled
        };
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("verifyPhone")]
    public async Task<AccountVadliationResponse> VerifyPhone(AccountValidationRequest validationRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        if (dbUserModel.PhoneNumber != validationRequest.PhoneNumber)
            throw new UnauthorizedException();
        
        // Validate through twilio
        var verificationCheck = VerificationCheckResource.Create(
            to: validationRequest.PhoneNumber,
            code: validationRequest.VerificationCode,
            pathServiceSid: _twilioConfig.twilioVerificationServiceSid
        );

        if (verificationCheck.Status == "approved")
        {
            // Update the account model to be verified:
            var claimCopy = dbUserModel.Claims.ToList();
            claimCopy.Add(UserClaim.PhoneVerified);
            dbUserModel.Claims = claimCopy.ToArray();

            await _dbUsers.Upsert(dbUserModel);

            return new AccountVadliationResponse()
            {
                wasValidationSuccessful = true,
            };
        }
        
        return new AccountVadliationResponse()
        {
            wasValidationSuccessful = false,
        };
    }

    private async Task FlagAccountDuplicate(DbUserModel accountOne, DbUserModel accountTwo, MultiBoxReason reason)
    {
        // Don't flag admin accounts.
        if (accountOne.HasClaim(UserClaim.Administrator) || accountTwo.HasClaim(UserClaim.Administrator))
            return;
        
        accountOne.MultiboxAccounts.Add(new MultiboxAccount()
        {
            MultiboxReason = reason,
            TimeOccured = DateTime.UtcNow,
            User = accountTwo.ToSimpleUser()
        });
        await _dbUsers.Upsert(accountOne);
            
        accountTwo.MultiboxAccounts.Add(new MultiboxAccount()
        {
            MultiboxReason = reason,
            TimeOccured = DateTime.UtcNow,
            User = accountOne.ToSimpleUser()
        });
        await _dbUsers.Upsert(accountTwo);
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    [Route("query")]
    public async Task<GetDetailedUsersResponse> GetUsers([FromQuery] GetUserRequest request)
    {
        IMongoQueryable<DbUserModel> query = _dbUsers.Query();

        if (!request.UsernameSearch.IsNullOrEmpty())
            query = query.Where(it => it.Username.Contains(request.UsernameSearch));
        
        if (!request.DeviceIdentifierSearch.IsNullOrEmpty())
            query = query.Where(it => it.DeviceIdentifier == request.DeviceIdentifierSearch);
        
        if (!request.UserIdSearch.IsNullOrEmpty())
            query = query.Where(it => it.Id == request.UserIdSearch);

        if (request.RequireUserClaim != null)
            // Create a filter that require the claim to exist in the user claims
            query = query.Where(it => it.Claims.Any(it => it == request.RequireUserClaim));

        if (request.isBanned != null)
        {
            if ((bool)request.isBanned)
            {
                // Create a filter that only gets models with a 'BannedUntil' date after today.
                query = query.Where(it => it.BannedUntil > DateTime.UtcNow);
            }
            else
            {
                // Create a filter that only gets models with a 'BannedUntil' date before today.
                query = query.Where(it => it.BannedUntil <= DateTime.UtcNow);
            }
        }

        var matchingUsers = (await query
                .OrderByDescending(it => it.DateCreated)
                .Skip(50 * (request.pagination - 1))
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToDetailedUser())
            .ToList();

        return new GetDetailedUsersResponse()
        {
            users = matchingUsers
        };
    }
    
    [HttpGet]
    public async Task<GetUserResponse> GetUser(string userId)
    {
        IMongoQueryable<DbUserModel> query = _dbUsers.Query();
        query = query.Where(it => it.Id == userId);

        var matchingUsers = (await query
            .OrderByDescending(it => it.DateCreated)
            .FirstOrDefaultAsync());

        if (matchingUsers == null)
            throw new NotFoundException("The requested player was not found");

        return new GetUserResponse()
        {
            User = matchingUsers.ToUser()
        };
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    [Route("messages")]
    public async Task<GetPlayerChatMessagesResponse> GetPlayerChatMessages(string playerId, int pagination = 1)
    {

        var user = await _dbUsers.Query()
            .Where(it => it.Id == playerId)
            .FirstOrDefaultAsync();

        if (user == null)
            throw new NotFoundException("Player not found");

        var chatMessages = await _dbChatMessages.Query()
            .Where(it => it.SentBy.Id == playerId)
            .OrderByDescending(it => it.SentAt)
            .Skip((pagination - 1) * 50)
            .Take(50)
            .ToListAsync();

        return new GetPlayerChatMessagesResponse()
        {
            Messages = chatMessages.Select(it => it.ToChatMessage()).ToList()
        };
    }

    private string GenerateJsonWebToken(DbUserModel user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim> {
            new Claim("name", user.Username),
            new Claim("uuid", user.Id),
            new Claim("DateOfJoin", ((DateTimeOffset)user.DateCreated).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in user.Claims)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Issuer"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private async Task<DbUserModel?> AuthenticateUserByPassword(AuthorizationRequest request)
    {
        // Try to get a user
        DbUserModel? dbUserModel = await _dbUsers.Query().FirstOrDefaultAsync(it => it.Username == request.Username);

        if (dbUserModel == null || !JwtManager.VerifyHashedStringMatches(request.Password, dbUserModel.PasswordHash))
            return null;

        return dbUserModel;
    }
}