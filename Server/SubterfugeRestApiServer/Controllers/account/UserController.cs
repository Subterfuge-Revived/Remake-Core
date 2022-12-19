using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController : ControllerBase
{
    public UserController(IConfiguration configuration)
    {
        _config = configuration;
    }

    private readonly IConfiguration _config;
    
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<AuthorizationResponse>> Login(AuthorizationRequest request)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel != null)
        {
            // TODO: This indicates that a user has two accounts!
            // They logged in to another account while holding a token for a different account.
            // Flag this in the database
            var t = "";
        }

        var user = await AuthenticateUserByPassword(request);
        if (user == null)
            return Unauthorized();

        
        var tokenString = GenerateJsonWebToken(user);
        return Ok(
            new AuthorizationResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Token = tokenString,
                User = user.UserModel.ToUser(),
            }
        );
    }

    private string GenerateJsonWebToken(DbUserModel dbUserModel)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim> {
            new Claim("name", dbUserModel.UserModel.Username),
            new Claim("uuid", dbUserModel.UserModel.Id),
            new Claim("email", dbUserModel.UserModel.Email),
            new Claim("DateOfJoin", ((DateTimeOffset)dbUserModel.UserModel.DateCreated).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in dbUserModel.UserModel.Claims)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Issuer"],
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private async Task<DbUserModel?> AuthenticateUserByPassword(AuthorizationRequest request)
    {
        // Try to get a user
        DbUserModel? dbUserModel = await DbUserModel.GetUserFromUsername(request.Username);

        if (dbUserModel == null || !JwtManager.VerifyPasswordHash(request.Password, dbUserModel.UserModel.PasswordHash))
            return null;

        return dbUserModel;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<AccountRegistrationResponse>> RegisterAccount(AccountRegistrationRequest registrationRequeset)
    {
        DbUserModel? dbUserModel = await DbUserModel.GetUserFromUsername(registrationRequeset.Username);
        if(dbUserModel != null)
            return Conflict(ResponseFactory.createResponse(ResponseType.DUPLICATE, "A very popular choice! Unfortunately, that username is taken..."));
            
        // Create a new user model
        DbUserModel model = new DbUserModel(registrationRequeset);
        await model.SaveToDatabase();
        string token = GenerateJsonWebToken(model);
        return Ok(new AccountRegistrationResponse
        {
            Token = token,
            User = model.AsUser(),
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet]
    public async Task<ActionResult<GetUserResponse>> GetUsers(
        int pagination = 1,
        string? username = null,
        string? email = null,
        string? deviceIdentifier = null,
        string? userId = null,
        UserClaim? claim = null,
        string? phone = null,
        Boolean isBanned = false
    ) {
        var filterBuilder = Builders<UserModel>.Filter;
        var filter = filterBuilder.Empty;
        
        if (!email.IsNullOrEmpty())
        {
            filter &= filterBuilder.Regex(model => model.Email, $".*{email}.*");
        }

        if (!username.IsNullOrEmpty())
        {
            filter &= filterBuilder.Regex(model => model.Username, $".*{username}.*");
        }
        
        if (!deviceIdentifier.IsNullOrEmpty())
        {
            filter &= filterBuilder.Eq(model => model.DeviceIdentifier, deviceIdentifier);
        }
        
        if (!userId.IsNullOrEmpty())
        {
            filter &= filterBuilder.Eq(model => model.Id, userId);
        }
        
        if (!phone.IsNullOrEmpty())
        {
            filter &= filterBuilder.Eq(model => model.PhoneNumber, phone);
        }

        if (claim != null)
        {
            // Create a filter that requires all `claims` values to exist in the model's claim list.
            filter &= filterBuilder.All(model => model.Claims, new UserClaim[]{ claim.GetValueOrDefault() });
        }

        if (isBanned)
        {
            // Create a filter that only gets models with a 'BannedUntil' date after today.
            filter &= filterBuilder.Gt(model => model.BannedUntil, DateTime.Now);
        }
        else
        {
            // Create a filter that only gets models with a 'BannedUntil' date before today.
            filter &= filterBuilder.Lte(model => model.BannedUntil, DateTime.Now);
        }

        var matchingUsers = (await MongoConnector.GetUserCollection().FindAsync(
                filter,
                new FindOptions<UserModel>() 
                {
                    Sort = Builders<UserModel>.Sort.Descending(it => it.DateCreated),
                    Limit = 50,
                    Skip = 50 * (pagination - 1),
                }
            ))
            .ToList()
            .Select(it => it.Obfuscate())
            .ToArray();

        return Ok(new GetUserResponse()
        {
            Users = matchingUsers
        });
    }
}