using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeRestApiServer.Middleware;

// Middleware to set a DbUserModel if the JWT token is set.
public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        string? userId = null;
        try
        {
            userId = context.User?.Claims?.First(it => it.Type == "uuid")?.Value;
            if (userId != null)
            {
                DbUserModel? dbUserModel = await DbUserModel.GetUserFromGuid(userId);
                if (dbUserModel != null)
                {
                    context.Items["User"] = dbUserModel;
                }
            }
        }
        catch (Exception e)
        {
            // Noop.
            // Just attach the DbUserModel if it can be found from the user claim. If not, do nothing.
        }

        await _next(context);
    }
}