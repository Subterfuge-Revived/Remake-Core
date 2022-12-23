using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeServerConsole.Responses;

public abstract class ActionResultException : Exception
{
    public readonly NetworkResponse response = new NetworkResponse();

    public abstract ActionResult ToActionResult();
}

public class NotFoundException : ActionResultException
{
    public NotFoundException(string details)
    {
        response.Status = ResponseFactory.createResponse(ResponseType.NOT_FOUND, details);
    }
    public override ActionResult ToActionResult()
    {
        return new NotFoundObjectResult(response);
    }
}

public class ConflictException : ActionResultException
{
    public ConflictException(string details)
    {
        response.Status = ResponseFactory.createResponse(ResponseType.DUPLICATE, details);
    }
    
    public override ActionResult ToActionResult()
    {
        return new ConflictObjectResult(response);
    }
}

public class BadRequestException : ActionResultException
{
    public BadRequestException(string details)
    {
        response.Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST, details);
    }
    public override ActionResult ToActionResult()
    {
        return new BadRequestObjectResult(response);
    }
}

public class UnauthorizedException : ActionResultException
{
    public UnauthorizedException()
    {
        response.Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED, "Unauthorized");
    }
    public override ActionResult ToActionResult()
    {
        return new UnauthorizedObjectResult(response);
    }
}

public class ForbidException : ActionResultException
{
    public ForbidException()
    {
        response.Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST, "You don't have the correct permissions to do that.");
    }
    public override ActionResult ToActionResult()
    {
        return new ObjectResult(response) { StatusCode = (int)HttpStatusCode.Forbidden };
    }
} 