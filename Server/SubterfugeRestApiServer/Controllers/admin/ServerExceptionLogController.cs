using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;

namespace SubterfugeRestApiServer.Controllers.admin;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin/exceptions")]
public class ServerExceptionLogController : ControllerBase
{
    private IDatabaseCollection<DbServerException> _dbExceptionLog;

    public ServerExceptionLogController(IDatabaseCollectionProvider mongo)
    {
        this._dbExceptionLog = mongo.GetCollection<DbServerException>();
    }
    
    [HttpGet]
    public async Task<ActionResult<ServerExceptionLogResponse>> GetServerExceptions(
        int pagination = 1,
        string? username = null,
        string? userId = null,
        string? httpMethod = null,
        string? requestUrl = null,
        string? exceptionSource = null,
        string? remoteIpAddress = null
    )
    {
        var query = _dbExceptionLog.Query();
    
        if (username != null)
        {
            query = query.Where(it => it.Username.Contains(username));
        }

        if (userId != null)
        {
            query = query.Where(it => it.UserId == userId);
        }
    
        if (httpMethod != null)
        {
            query = query.Where(it => it.HttpMethod == httpMethod);
        }
    
        if (requestUrl != null)
        {
            query = query.Where(it => it.RequestUri == requestUrl);
        }

        if (exceptionSource != null)
        {
            query = query.Where(it => it.ExceptionSource == exceptionSource);
        }

        if (remoteIpAddress != null)
        {
            query = query.Where(it => it.RemoteIpAddress == remoteIpAddress);
        }

        var matchingServerActions = (await query
            .OrderByDescending(it => it.UnixTimeProcessed)
            .Skip(50 * (pagination - 1))
            .Take(50)
            .ToListAsync())
            .Select(it => it.ToServerException())
            .ToList();

        return Ok(new ServerExceptionLogResponse()
        {
            Exceptions = matchingServerActions
        });
    }
}