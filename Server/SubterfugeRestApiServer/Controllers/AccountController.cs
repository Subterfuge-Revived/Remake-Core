using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public class AccountController : ControllerBase, INetworkAccountManager
{
    [HttpPost(Name = "Login")]
    public AuthorizationResponse Login(AuthorizationRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost(Name = "LoginWithToken")]
    public AuthorizationResponse LoginWithToken(AuthorizedTokenRequest tokenRequest)
    {
        throw new NotImplementedException();
    }

    [HttpPost(Name = "RegisterAccount")]
    public AuthorizationResponse RegisterAccount(AccountRegistrationRequest registrationRequeset)
    {
        throw new NotImplementedException();
    }

    [HttpPost(Name = "GetRoles")]
    public GetRolesResponse GetRoles(GetRolesRequest roleRequest)
    {
        DbUserModel dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if(dbUserModel == null)
            return new GetRolesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Claims = new List<UserClaim>(){},
            };
            
        var response = new GetRolesResponse() {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
        };
        response.Claims.AddRange(dbUserModel.UserModel.Claims);
        return response;
    }
}