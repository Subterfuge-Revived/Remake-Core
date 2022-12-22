using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
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
    public async Task<PingResponse> AuthorizedPing()
    {
        return await Task.FromResult(new PingResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
}