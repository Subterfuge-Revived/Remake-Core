using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
        var user = await AuthenticateUserByPassword(request);
        if (user == null)
            return Unauthorized();

        
        var tokenString = GenerateJsonWebToken(user);
        return Ok(
            new AuthorizationResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Token = tokenString
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
            return Conflict("Username already exists");
            
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
    [HttpPost]
    public async Task<ActionResult<GetUserResponse>> GetUsers(GetUserRequest getUserRequest)
    {
        var filterBuilder = Builders<UserModel>.Filter;
        var filter = filterBuilder.Empty;
        
        if (!getUserRequest.EmailSearch.IsNullOrEmpty())
        {
            filter &= filterBuilder.Regex(model => model.Email, $".*{getUserRequest.EmailSearch}.*");
        }

        if (!getUserRequest.UsernameSearch.IsNullOrEmpty())
        {
            filter &= filterBuilder.Regex(model => model.Username, $".*{getUserRequest.UsernameSearch}.*");
        }
        
        if (!getUserRequest.DeviceIdentifierSearch.IsNullOrEmpty())
        {
            filter &= filterBuilder.Regex(model => model.DeviceIdentifier, $".*{getUserRequest.DeviceIdentifierSearch}.*");
        }

        if (!getUserRequest.RequireUserClaims.IsNullOrEmpty())
        {
            filter &= filterBuilder.All(model => model.Claims, getUserRequest.RequireUserClaims);
        }

        if (getUserRequest.isBanned)
        {
            filter &= filterBuilder.Gt(model => model.BannedUntil, DateTime.Now);
        }
        else
        {
            filter &= filterBuilder.Lte(model => model.BannedUntil, DateTime.Now);
        }

        var matchingUsers = (await MongoConnector.GetUserCollection().FindAsync(filter))
            .ToList()
            .Select(it => it.Obfuscate())
            .ToArray();

        return Ok(new GetUserResponse()
        {
            Users = matchingUsers
        });
    }
}