using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeRestApiServer.Middleware;

public class ExceptionResponseMiddleware : ExceptionFilterAttribute
{

    public override Task OnExceptionAsync(ExceptionContext context)
    {
        HandleException(context);
        return base.OnExceptionAsync(context);
    }

    public override void OnException(ExceptionContext context)
    {
        HandleException(context);
        base.OnException(context);
    }

    private void HandleException(ExceptionContext context)
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

        
        // TODO: Add this to a custom database table to specifically track server exceptions so that we can identify problem areas and what might be causing them.
        // Log the userId, request URI, stack trace, etc. This should let us reproduce issues and track down bugs :)
        var Uri = context.HttpContext.Request.Path;
        var RequestMethod = context.HttpContext.Request.Method;
        
        StreamReader reader = new StreamReader(context.HttpContext.Request.Body);
        var RequestBody = reader.ReadToEnd();

        var Username = (context.HttpContext.Items["User"] as DbUserModel)?.UserModel?.Username;
        var UserId = (context.HttpContext.Items["User"] as DbUserModel)?.UserModel?.Id;
        var UserClaims = (context.HttpContext.Items["User"] as DbUserModel)?.UserModel?.Claims;
    }
}