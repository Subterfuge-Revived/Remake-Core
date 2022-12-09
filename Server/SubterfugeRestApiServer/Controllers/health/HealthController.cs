using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.health;

[ApiController]
[Route("api/[controller]/[action]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<PingResponse> Ping()
    {
        return Ok(new PingResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
    
    [HttpGet]
    [Authorize]
    public ActionResult<PingResponse> AuthorizedPing()
    {
        return Ok(new PingResponse()
        {
            Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
        });
    }
}