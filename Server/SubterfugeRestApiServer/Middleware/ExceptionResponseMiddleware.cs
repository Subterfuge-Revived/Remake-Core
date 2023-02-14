using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.Middleware;

public class ExceptionResponseMiddleware : ExceptionFilterAttribute
{
    private IDatabaseCollectionProvider _db;
    private ILogger _logger;

    public ExceptionResponseMiddleware(IDatabaseCollectionProvider mongo, ILogger<ExceptionResponseMiddleware> logger)
    {
        this._db = mongo;
        _logger = logger;
    }

    public override async Task OnExceptionAsync(ExceptionContext context)
    {
        await HandleException(context);
        await base.OnExceptionAsync(context);
    }

    public override async void OnException(ExceptionContext context)
    {
        await HandleException(context);
        base.OnException(context);
    }

    private async Task HandleException(ExceptionContext context)
    {
        // Format the response object:
        var response = new ObjectResult(
            SubterfugeResponse<string>.OfFailure(
                ResponseType.INTERNAL_SERVER_ERROR,
                context.Exception?.Message + " " + context.Exception?.StackTrace
            )
        )
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        
        context.Result = response;
        context.ExceptionHandled = true;
        
        LogException(context);
        await TryLogDatabase(context);
    }

    private void LogException(ExceptionContext context)
    {
        var username = (context.HttpContext.Items["User"] as DbUserModel)?.Username;
        var userId = (context.HttpContext.Items["User"] as DbUserModel)?.Id;
        var remoteIpAddress = context.HttpContext.Connection.RemoteIpAddress;
        var httpMethod = context.HttpContext.Request?.Method;
        var requestUrl = context.HttpContext.Request?.Path.Value;
        var queryString = context.HttpContext.Request?.QueryString;
        var statusCode = 500;
        
        _logger.LogError(
            "{user}(uuid={userId}, ip={ip}) {method} {url}{queryString} => {statusCode} \n {exception} \n {stackTrace}",
            username,
            userId,
            remoteIpAddress,
            httpMethod,
            requestUrl,
            queryString,
            statusCode,
            context.Exception.Message,
            context.Exception.StackTrace
        );
    }
    
    private async Task TryLogDatabase(ExceptionContext context)
    {
        StreamReader reader = new StreamReader(context.HttpContext.Request.Body);
        var RequestBody = reader.ReadToEnd();

        var user = context.HttpContext.Items["User"] as DbUserModel;
        var Username = user?.Username;
        var UserId = user?.Id;

        var serverException = new DbServerException()
        {
            Username = Username,
            UserId = UserId,
            RemoteIpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
            HttpMethod = context.HttpContext.Request.Method,
            RequestUri = context.HttpContext.Request.Path,
            RequestBody = RequestBody,
            QueryString = context.HttpContext.Request.QueryString.ToString(),
            ExceptionMessage = context.Exception?.Message,
            ExceptionSource = context.Exception?.Source,
            StackTrace = context.Exception?.StackTrace,
            UserAgent = context.HttpContext.Request.Headers["User-Agent"]
        };

        await _db.GetCollection<DbServerException>().Upsert(serverException);
    }
}