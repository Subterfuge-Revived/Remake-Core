using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SubterfugeCore.Models;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeRestApiServer.Controllers.admin;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin/serverLog")]
public class ServerActionLogController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ServerActionLogResponse>> GetActionLog(
        int pagination = 1,
        string? username = null,
        string? userId = null,
        string? httpMethod = null,
        string? requestUrl = null
    ) {
        var filterBuilder = Builders<ServerActionLog>.Filter;
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
            filter &= filterBuilder.Eq(model => model.RequestUrl, requestUrl);
        }

        var matchingServerActions = (await MongoConnector.GetCollection<ServerActionLog>().FindAsync(
                filter,
                new FindOptions<ServerActionLog>()
                {
                    Sort = Builders<ServerActionLog>.Sort.Descending(it => it.UnixTimeProcessed),
                    Limit = 50,
                    Skip = 50 * (pagination - 1),
                }
            ))
            .ToList();

        return Ok(new ServerActionLogResponse()
        {
            Actions = matchingServerActions,
        });
    }
}