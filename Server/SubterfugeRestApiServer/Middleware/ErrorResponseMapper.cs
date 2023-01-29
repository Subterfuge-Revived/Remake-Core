using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SubterfugeRestApiServer.Middleware;

public class ErrorResponseMapper
{
    
    public static Func<ActionContext, IActionResult> ValidationErrorFactory()
    {
        return context =>
        {
            var errors = context.ModelState.Values.SelectMany(m => m.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new NetworkResponse()
            {
                Status = new ResponseStatus()
                {
                    Detail = "{ \"errors\": " + JsonSerializer.Serialize(errors) + " }",
                    IsSuccess = false,
                    ResponseType = ResponseType.VALIDATION_ERROR
                }
            };
            

            return new BadRequestObjectResult(response);
        };
    }
}