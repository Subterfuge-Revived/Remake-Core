using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.Controllers.admin;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin/")]
public class SubterfugeActionLogController : ControllerBase, ISubterfugeAdminApi
{
    
    private IDatabaseCollection<DbServerAction> _dbServerLog;
    private IDatabaseCollection<DbServerException> _dbExceptionLog;

    public SubterfugeActionLogController(IDatabaseCollectionProvider mongo)
    {
        this._dbServerLog = mongo.GetCollection<DbServerAction>();
        this._dbExceptionLog = mongo.GetCollection<DbServerException>();
    }

    [HttpGet]
    [Route("serverLog")]
    public async Task<ServerActionLogResponse> GetActionLog([FromQuery] ServerActionLogRequeset request)
    {
        var query = _dbServerLog.Query();
        
        if (request.Username != null)
        {
            query = query.Where(it => it.Username.Contains(request.Username));
        }

        if (request.UserId != null)
        {
            query = query.Where(it => it.UserId == request.UserId);
        }
        
        if (request.HttpMethod != null)
        {
            query = query.Where(it => it.HttpMethod == request.HttpMethod);
        }
        
        if (request.RequestUrl != null)
        {
            query = query.Where(it => it.RequestUrl == request.RequestUrl);
        }

        var matchingServerActions = (await query
                .OrderByDescending(it => it.TimesAccessed)
                .Skip(50 * (request.Pagination - 1))
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToServerAction())
            .ToList();

        return new ServerActionLogResponse()
        {
            Actions = matchingServerActions,
        };
    }

    [HttpGet]
    [Route("exceptions")]
    public async Task<ServerExceptionLogResponse> GetServerExceptions([FromQuery] ServerExceptionLogRequest request)
    {
        var query = _dbExceptionLog.Query();
    
        if (request.Username != null)
        {
            query = query.Where(it => it.Username.Contains(request.Username));
        }

        if (request.UserId != null)
        {
            query = query.Where(it => it.UserId == request.UserId);
        }
    
        if (request.HttpMethod != null)
        {
            query = query.Where(it => it.HttpMethod == request.HttpMethod);
        }
    
        if (request.RequestUrl != null)
        {
            query = query.Where(it => it.RequestUri == request.RequestUrl);
        }

        if (request.ExceptionSource != null)
        {
            query = query.Where(it => it.ExceptionSource == request.ExceptionSource);
        }

        if (request.RemoteIpAddress != null)
        {
            query = query.Where(it => it.RemoteIpAddress == request.RemoteIpAddress);
        }

        var matchingServerActions = (await query
                .OrderByDescending(it => it.UnixTimeProcessed)
                .Skip(50 * (request.Pagination - 1))
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToServerException())
            .ToList();

        return new ServerExceptionLogResponse()
        {
            Exceptions = matchingServerActions
        };
    }
}