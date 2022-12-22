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
[Route("api/admin/serverLog")]
public class ServerActionLogController : ControllerBase
{
    
    private IDatabaseCollection<DbServerAction> _dbServerLog;

    public ServerActionLogController(IDatabaseCollectionProvider mongo)
    {
        this._dbServerLog = mongo.GetCollection<DbServerAction>();
    }
    
    [HttpGet]
    public async Task<ActionResult<ServerActionLogResponse>> GetActionLog(
        int pagination = 1,
        string? username = null,
        string? userId = null,
        string? httpMethod = null,
        string? requestUrl = null
    ) {
        var query = _dbServerLog.Query();
        
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
            query = query.Where(it => it.RequestUrl == requestUrl);
        }

        var matchingServerActions = (await query
            .OrderByDescending(it => it.TimesAccessed)
            .Skip(50 * (pagination - 1))
            .Take(50)
            .ToListAsync())
            .Select(it => it.ToServerAction())
            .ToList();

        return Ok(new ServerActionLogResponse()
        {
            Actions = matchingServerActions,
        });
    }
}