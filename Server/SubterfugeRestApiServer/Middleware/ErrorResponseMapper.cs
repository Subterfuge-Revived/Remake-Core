using Microsoft.AspNetCore.Mvc;
using Subterfuge.Remake.Api.Network;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Subterfuge.Remake.Server.Middleware;

public class ErrorResponseMapper
{
    
    public static Func<ActionContext, IActionResult> ValidationErrorFactory()
    {
        return context =>
        {
            var errors = context.ModelState.Values.SelectMany(m => m.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = SubterfugeResponse<string>.OfFailure(ResponseType.VALIDATION_ERROR, "{ \"errors\": " + JsonSerializer.Serialize(errors) + " }");
            return new BadRequestObjectResult(response);
        };
    }
}