using MongoDB.Driver.Linq;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;

namespace SubterfugeRestApiServer.Middleware;

// Middleware to set a DbUserModel if the JWT token is set.
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private IDatabaseCollectionProvider _db;

    public JwtMiddleware(RequestDelegate next, IDatabaseCollectionProvider mongo)
    {
        _next = next;
        _db = mongo;
    }

    public async Task Invoke(HttpContext context)
    {
        string? userId = null;
        try
        {
            userId = context.User?.Claims?.First(it => it.Type == "uuid")?.Value;
            if (userId != null)
            {
                DbUserModel? user = await _db.GetCollection<DbUserModel>().Query().FirstOrDefaultAsync(it => it.Id == userId);
                if (user != null)
                {
                    context.Items["User"] = user;
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