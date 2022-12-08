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
            _logger.LogInformation(
                "{user}(uuid={userId}, ip={ip}) {method} {url} => {statusCode}",
                (context.Items["User"] as DbUserModel)?.UserModel?.Username,
                (context.Items["User"] as DbUserModel)?.UserModel?.Id,
                context.Connection.RemoteIpAddress,
                context.Request?.Method,
                context.Request?.Path.Value,
                context.Response?.StatusCode);
        }
    }
}