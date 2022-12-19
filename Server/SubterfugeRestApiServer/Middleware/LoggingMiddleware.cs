using MongoDB.Driver;
using SubterfugeCore.Models;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeRestApiServer.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    
    public LoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<LoggingMiddleware>();
    }
    
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        finally
        {
            var username = (context.Items["User"] as DbUserModel)?.UserModel?.Username;
            var userId = (context.Items["User"] as DbUserModel)?.UserModel?.Id;
            var remoteIpAddress = context.Connection.RemoteIpAddress;
            var httpMethod = context.Request?.Method;
            var requestUrl = context.Request?.Path.Value;
            var queryString = context.Request?.QueryString;
            var statusCode = context.Response?.StatusCode;
            
            _logger.LogInformation(
                "{user}(uuid={userId}, ip={ip}) {method} {url}{queryString} => {statusCode}",
                username,
                userId,
                remoteIpAddress,
                httpMethod,
                requestUrl,
                queryString,
                statusCode
            );

            var serverAction = new ServerActionLog()
            {
                Username = username,
                UserId = userId,
                RemoteIpAddress = remoteIpAddress.ToString(),
                HttpMethod = httpMethod,
                RequestUrl = requestUrl,
                StatusCode = statusCode,
            };
            
            await MongoConnector.GetCollection<ServerActionLog>().ReplaceOneAsync(
                it => it.Id == serverAction.Id,
                serverAction,
                new UpdateOptions { IsUpsert = true }
            );
        }
    }
}