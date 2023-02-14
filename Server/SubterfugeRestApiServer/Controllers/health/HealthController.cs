using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.health;

[ApiController]
[Route("api/[controller]/[action]")]
public class HealthController : ControllerBase, ISubterfugeHealthApi
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<SubterfugeResponse<PingResponse>> Ping()
    {
        return SubterfugeResponse<PingResponse>.OfSuccess(new PingResponse());
    }
    
    [HttpGet]
    [Authorize]
    public async Task<SubterfugeResponse<AuthorizedPingResponse>> AuthorizedPing()
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<AuthorizedPingResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in.");
        
        return SubterfugeResponse<AuthorizedPingResponse>.OfSuccess(new AuthorizedPingResponse()
        {
            LoggedInUser = dbUserModel.ToUser()
        });
    }
}