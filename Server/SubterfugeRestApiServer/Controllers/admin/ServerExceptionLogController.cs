using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SubterfugeCore.Models;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeRestApiServer.Controllers.admin;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin/exceptions")]
public class ServerExceptionLogController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ServerExceptionLogResponse>> GetServerExceptions(
        int pagination = 1,
        string? username = null,
        string? userId = null,
        string? httpMethod = null,
        string? requestUrl = null,
        string? exceptionSource = null,
        string? remoteIpAddress = null
    ) {
        var filterBuilder = Builders<ServerExceptionLog>.Filter;
        var filter = filterBuilder.Empty;
    
        if (username != null)
        {
            filter &= filterBuilder.Regex(model => model.Username, $".*{username}.*");
        }

        if (userId != null)
        {
            filter &= filterBuilder.Eq(model => model.UserId, userId);
        }
    
        if (httpMethod != null)
        {
            filter &= filterBuilder.Eq(model => model.HttpMethod, httpMethod);
        }
    
        if (requestUrl != null)
        {
            filter &= filterBuilder.Eq(model => model.RequestUri, requestUrl);
        }

        if (exceptionSource != null)
        {
            filter &= filterBuilder.Eq(model => model.ExceptionSource, exceptionSource);
        }

        if (remoteIpAddress != null)
        {
            filter &= filterBuilder.Eq(model => model.RemoteIpAddress, remoteIpAddress);
        }

        var matchingServerActions = (await MongoConnector.GetServerExceptionLog().FindAsync(
                filter,
                new FindOptions<ServerExceptionLog>()
                {
                    Sort = Builders<ServerExceptionLog>.Sort.Descending(it => it.UnixTimeProcessed),
                    Limit = 50,
                    Skip = 50 * (pagination - 1),
                }
            ))
            .ToList();

        return Ok(new ServerExceptionLogResponse()
        {
            Exceptions = matchingServerActions
        });
    }
}