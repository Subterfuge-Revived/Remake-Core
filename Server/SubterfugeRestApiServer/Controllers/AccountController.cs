using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController : ControllerBase
{
    public AccountController(IConfiguration configuration, ILogger<AccountController> logger)
    {
        _config = configuration;
        _logger = logger;
    }

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    
    [AllowAnonymous]
    [HttpPost(Name = "Login")]
    public async Task<ActionResult<AuthorizationResponse>> Login(AuthorizationRequest request)
    {
        var user = await AuthenticateUserByPassword(request);

        if (user != null)
        {
            var tokenString = GenerateJsonWebToken(user);
            return Ok(
                new AuthorizationResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                    Token = tokenString
                }
            );
        }

        return Unauthorized(new AuthorizationResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED),
        });
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
        DbUserModel dbUserModel = await DbUserModel.GetUserFromUsername(request.Username);

        if (dbUserModel == null || !JwtManager.VerifyPasswordHash(request.Password, dbUserModel.UserModel.PasswordHash))
            return null;

        return dbUserModel;
    }

    [AllowAnonymous]
    [HttpPost(Name = "RegisterAccount")]
    public async Task<ActionResult<AccountRegistrationResponse>> RegisterAccount(AccountRegistrationRequest registrationRequeset)
    {
        DbUserModel dbUserModel = await DbUserModel.GetUserFromUsername(registrationRequeset.Username);
        if(dbUserModel != null)
            return Conflict(new AccountRegistrationResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
            });
            
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

    [Authorize]
    [HttpPost(Name = "GetRoles")]
    public async Task<ActionResult<GetRolesResponse>> GetRoles(GetRolesRequest roleRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return Unauthorized(new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
            });
            
        var response = new GetRolesResponse() {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            Claims = dbUserModel.UserModel.Claims
        };
        return Ok(response);
    }
}