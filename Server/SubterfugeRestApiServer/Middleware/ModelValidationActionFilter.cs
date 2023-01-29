using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeRestApiServer.Middleware;

public class ModelValidationActionFilter
{
    public static Func<ActionContext, IActionResult> InvalidModelStateResponseFactory()
    {
        return context =>
        {
            var errors = context.ModelState.Values.SelectMany(m => m.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new ResponseStatus()
            {
                Detail = "{ \"errors\": " + JsonSerializer.Serialize(errors) + " }",
                IsSuccess = false,
                ResponseType = ResponseType.VALIDATION_ERROR
            };
            
            return new BadRequestObjectResult(response);
        };
    }
}