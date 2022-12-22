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

namespace SubterfugeRestApiServer;

[ApiController]
[Route("api/user/")]
public class UserController : ControllerBase, ISubterfugeAccountApi
{
    
    private IDatabaseCollection<DbUserModel> _dbUsers;

    public UserController(IConfiguration configuration, IDatabaseCollectionProvider mongo)
    {
        _config = configuration;
        _dbUsers = mongo.GetCollection<DbUserModel>();
    }

    private readonly IConfiguration _config;

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
            // This indicates that a user has two accounts!
            // They logged in to another account while holding a token for a different account on the same device/API.
            dbUserModel.MultiboxAccounts.Add(new MultiboxAccount()
            {
                MultiboxReason = MultiBoxReason.LOGIN_WITH_CREDENTIALS_FOR_ANOTHER_ACCOUNT,
                TimeOccured = DateTime.UtcNow,
                User = user.ToUser()
            });
            await _dbUsers.Upsert(dbUserModel);
            
            user.MultiboxAccounts.Add(new MultiboxAccount()
            {
                MultiboxReason = MultiBoxReason.LOGIN_WITH_CREDENTIALS_FOR_ANOTHER_ACCOUNT,
                TimeOccured = DateTime.UtcNow,
                User = dbUserModel.ToUser()
            });
            await _dbUsers.Upsert(user);
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
        List<DbUserModel> usersMatchingUsernameOrPhone = await _dbUsers.Query()
            .Where(it => it.Username == registrationRequeset.Username)
            .Where(it => it.PhoneNumber == registrationRequeset.PhoneNumber)
            .ToListAsync();

        DbUserModel? matchingUsername = usersMatchingUsernameOrPhone.FirstOrDefault(it => it.Username == registrationRequeset.Username);
        if (matchingUsername != null)
            throw new ConflictException("A very popular choice! Unfortunately, that username is taken...");

        // Create the new user account
        DbUserModel model = DbUserModel.FromRegistrationRequest(registrationRequeset);
        
        var matchingPhone = usersMatchingUsernameOrPhone.Where(it => it.PhoneNumber == registrationRequeset.PhoneNumber).ToList();
        if (!matchingPhone.IsNullOrEmpty())
        {
            // Flag all accounts matching the phone number as a possible multibox.
            foreach(DbUserModel user in matchingPhone)
            {
                model.MultiboxAccounts.Add(new MultiboxAccount()
                {
                    MultiboxReason = MultiBoxReason.DUPLICATE_PHONE_NUMBER,
                    TimeOccured = DateTime.UtcNow,
                    User = user.ToUser()
                });
                await _dbUsers.Upsert(model);
            
                user.MultiboxAccounts.Add(new MultiboxAccount()
                {
                    MultiboxReason = MultiBoxReason.DUPLICATE_PHONE_NUMBER,
                    TimeOccured = DateTime.UtcNow,
                    User = model.ToUser()
                });
                await _dbUsers.Upsert(user);
            }
        }
            
        // Create a new user model
        await _dbUsers.Upsert(model);
        
        string token = GenerateJsonWebToken(model);
        return new AccountRegistrationResponse
        {
            Token = token,
            User = model.ToUser(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        };
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    [Route("query")]
    public async Task<GetUserResponse> GetUsers([FromQuery] GetUserRequest request)
    {
        IMongoQueryable<DbUserModel> query = _dbUsers.Query();
        
        if (!request.EmailSearch.IsNullOrEmpty())
            query = query.Where(it => it.Email.Contains(request.EmailSearch));

        if (!request.UsernameSearch.IsNullOrEmpty())
            query = query.Where(it => it.Username.Contains(request.UsernameSearch));
        
        if (!request.DeviceIdentifierSearch.IsNullOrEmpty())
            query = query.Where(it => it.DeviceIdentifier == request.DeviceIdentifierSearch);
        
        if (!request.UserIdSearch.IsNullOrEmpty())
            query = query.Where(it => it.Id == request.UserIdSearch);

        if (request.RequireUserClaim != null)
            // Create a filter that require the claim to exist in the user claims
            query = query.Where(it => it.Claims.Any(it => it == request.RequireUserClaim));

        if (request.isBanned)
        {
            // Create a filter that only gets models with a 'BannedUntil' date after today.
            query = query.Where(it => it.BannedUntil > DateTime.UtcNow);
        }
        else
        {
            // Create a filter that only gets models with a 'BannedUntil' date before today.
            query = query.Where(it => it.BannedUntil <= DateTime.UtcNow);
        }

        var matchingUsers = (await query
                .OrderByDescending(it => it.DateCreated)
                .Skip(50 * (request.pagination - 1))
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToUser())
            .ToList();

        return new GetUserResponse()
        {
            Users = matchingUsers
        };
    }

    private string GenerateJsonWebToken(DbUserModel user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim> {
            new Claim("name", user.Username),
            new Claim("uuid", user.Id),
            new Claim("email", user.Email),
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