using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SubterfugeRestApiServer.Authentication;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole;

public class BasicAuthenticationOptions : AuthenticationSchemeOptions
{
    public BasicAuthenticationOptions()
    {
    }
}

public class JwtAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
{
    public JwtAuthenticationHandler(
        IOptionsMonitor<BasicAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // The endpoint requires authorization before performing.
        // Get JWT header
        string authorizationHeader = Request.Headers["Subterfuge-Auth-Token"];
        if (authorizationHeader != null)
        {
            if (JwtManager.ValidateToken(authorizationHeader, out var uuid))
            {
                // Validate user exists.
                DbUserModel? dbUserModel = await DbUserModel.GetUserFromGuid(uuid);
                if (dbUserModel != null)
                {
                    // create a ClaimsPrincipal from your header
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, dbUserModel.UserModel.Id),
                    };
                    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
                    var ticket = new AuthenticationTicket(claimsPrincipal,
                        new AuthenticationProperties { IsPersistent = false },
                        Scheme.Name
                    );

                    Context.Items["User"] = dbUserModel;
                    
                    return AuthenticateResult.Success(ticket);
                }
            }
        }
        return AuthenticateResult.Fail("Unauthorized");
    }
}