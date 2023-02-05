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
    public async Task<PingResponse> Ping()
    {
        return await Task.FromResult(new PingResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
    
    [HttpGet]
    [Authorize]
    public async Task<AuthorizedPingResponse> AuthorizedPing()
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();
        
        return await Task.FromResult(new AuthorizedPingResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            LoggedInUser = dbUserModel.ToUser()
        });
    }
}