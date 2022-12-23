using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
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
        if (context.Exception is ActionResultException exception)
        {
            context.Result = exception.ToActionResult();
            context.ExceptionHandled = true;
            return;
        }
        
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            _logger.LogError(context.Exception.Message);
            _logger.LogError(context.Exception.StackTrace);
        }
        
        var response = new ObjectResult(
            new ResponseStatus()
            {
                IsSuccess = false,
                ResponseType = ResponseType.INTERNAL_SERVER_ERROR,
                Detail = context.Exception?.Message + " " + context.Exception?.StackTrace,
            }) {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
        
        context.Result = response;
        context.ExceptionHandled = true;
        
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