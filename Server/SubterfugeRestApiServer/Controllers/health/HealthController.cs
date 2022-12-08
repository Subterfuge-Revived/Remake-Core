using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.health;

[ApiController]
[Route("api/[controller]/[action]")]
public class HealthController : ControllerBase
{
    public HealthController(
        IConfiguration configuration,
        ILogger<AccountController> logger
    )
    {
        _config = configuration;
        _logger = logger;
    }

    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    
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