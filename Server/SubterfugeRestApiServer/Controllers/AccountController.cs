using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Route("api/[controller]/[action]")]
public class AccountController : ControllerBase
{
    public AccountController(IConfiguration configuration)
    {
        _config = configuration;
    }

    private readonly IConfiguration _config;
    
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
        
        var claims = new[] {
            new Claim("name", dbUserModel.UserModel.Username),
            new Claim("uuid", dbUserModel.UserModel.Id),
            new Claim("email", dbUserModel.UserModel.Email),
            new Claim("DateOfJoin", ((DateTimeOffset)dbUserModel.UserModel.DateCreated).ToUnixTimeSeconds().ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

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

        if (dbUserModel == null || !VerifyPasswordHash(request.Password, dbUserModel.UserModel.PasswordHash))
            return null;

        return dbUserModel;
    }
    
    public static String HashPassword(string password)
    {
        // Create salt
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            
        // Create the Rfc2898DeriveBytes and get the hash value: 
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
        byte[] hash = pbkdf2.GetBytes(20);
            
        // Combine salt and password bytes
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
            
        // Return value
        return Convert.ToBase64String(hashBytes);
    }

    public static Boolean VerifyPasswordHash(string password, string hashedPassword)
    {
        /* Fetch the stored value */
        string savedPasswordHash = hashedPassword;
            
        /* Extract the bytes */
        byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            
        /* Get the salt */
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);
            
        /* Compute the hash on the entered password the user entered using the extracted salt */
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
        byte[] hash = pbkdf2.GetBytes(20);
            
        /* Compare the results */
        for (int i=0; i < 20; i++)
            if (hashBytes[i + 16] != hash[i])
                return false;
        return true;
    }

    [AllowAnonymous]
    [HttpPost(Name = "RegisterAccount")]
    public async Task<ActionResult<AccountRegistrationResponse>> RegisterAccount(AccountRegistrationRequest registrationRequeset)
    {
        System.Diagnostics.Debug.WriteLine("Attempting to register a user!");
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
        var userId = HttpContext.User.Claims.First(it => it.Type == "uuid").Value;
        DbUserModel dbUserModel = await DbUserModel.GetUserFromGuid(userId);
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