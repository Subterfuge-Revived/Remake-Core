using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MongoDB.Driver;
using SubterfugeCore.Models;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeRestApiServer.Middleware;

public class ExceptionResponseMiddleware : ExceptionFilterAttribute
{

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
        
        var response = new ObjectResult(
            new ResponseStatus()
            {
                IsSuccess = false,
                ResponseType = ResponseType.INTERNAL_SERVER_ERROR,
                Detail = context.Exception?.Message + " " + context.Exception?.StackTrace,
                Uri = context.HttpContext.Request.Path
            }) {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
        
        context.Result = response;
        context.ExceptionHandled = true;
        
        StreamReader reader = new StreamReader(context.HttpContext.Request.Body);
        var RequestBody = reader.ReadToEnd();

        var user = context.HttpContext.Items["User"] as DbUserModel;
        var Username = user?.UserModel?.Username;
        var UserId = user?.UserModel?.Id;

        var serverException = new ServerExceptionLog()
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
        };
        
        await MongoConnector.GetCollection<ServerExceptionLog>().ReplaceOneAsync(
            it => it.Id == serverException.Id,
            serverException,
            new UpdateOptions { IsUpsert = true }
        );
    }
}