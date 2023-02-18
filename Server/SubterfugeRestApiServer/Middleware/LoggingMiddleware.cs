using Subterfuge.Remake.Server.Database;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private IDatabaseCollectionProvider _db;
    
    public LoggingMiddleware(
        RequestDelegate next,
        ILoggerFactory loggerFactory,
        IDatabaseCollectionProvider mongo
    )
    {
        _db = mongo;
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
            var username = (context.Items["User"] as DbUserModel)?.Username;
            var userId = (context.Items["User"] as DbUserModel)?.Id;
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

            var serverAction = new DbServerAction()
            {
                Username = username,
                UserId = userId,
                RemoteIpAddress = remoteIpAddress?.ToString(),
                HttpMethod = httpMethod,
                RequestUrl = requestUrl,
                StatusCode = statusCode,
                UserAgent = context.Request?.Headers["User-Agent"]
            };
            
            await _db.GetCollection<DbServerAction>().Upsert(serverAction);
        }
    }
}