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

    public async override Task OnExceptionAsync(ExceptionContext context)
    {
        await HandleException(context);
        await base.OnExceptionAsync(context);
    }

    public async override void OnException(ExceptionContext context)
    {
        await HandleException(context);
        base.OnException(context);
    }

    private async Task HandleException(ExceptionContext context)
    {
        // If the result was a specific exception, we can instead cast it to the expected result.
        if (context.Exception is ActionResultException actionResultException)
        {
            var handledResponse = actionResultException.ToActionResult();
            
            var username = (context.HttpContext.Items["User"] as DbUserModel)?.Username;
            var userId = (context.HttpContext.Items["User"] as DbUserModel)?.Id;
            var remoteIpAddress = context.HttpContext.Connection.RemoteIpAddress;
            var httpMethod = context.HttpContext.Request?.Method;
            var requestUrl = context.HttpContext.Request?.Path.Value;
            var queryString = context.HttpContext.Request?.QueryString;
            var statusCode = 500;
            
            _logger.LogInformation(
                "{user}(uuid={userId}, ip={ip}) {method} {url}{queryString} => {statusCode} : {actionResultException}",
                username,
                userId,
                remoteIpAddress,
                httpMethod,
                requestUrl,
                queryString,
                statusCode,
                actionResultException.GetType().ToString()
            );

            var serverAction = new DbServerAction()
            {
                Username = username,
                UserId = userId,
                RemoteIpAddress = remoteIpAddress?.ToString(),
                HttpMethod = httpMethod,
                RequestUrl = requestUrl,
                StatusCode = 500,
                UserAgent = context.HttpContext.Request?.Headers["User-Agent"]
            };
            
            await _db.GetCollection<DbServerAction>().Upsert(serverAction);
            
            context.Result = handledResponse;
            context.ExceptionHandled = true;
            
            return;
        }

        var response = new ObjectResult(
            new NetworkResponse()
            {
                Status = new ResponseStatus()
                {
                    IsSuccess = false,
                    ResponseType = ResponseType.INTERNAL_SERVER_ERROR,
                    Detail = context.Exception?.Message + " " + context.Exception?.StackTrace,
                },
            })
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };
        
        context.Result = response;
        context.ExceptionHandled = true;

        await TryLogDatabase(context);
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