using System.Net;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServer.Middleware;

public class BanMiddleware
{
    private readonly RequestDelegate _next;
    private IDatabaseCollection<DbIpBan> _dbIpBans;
    private ILogger _logger;

    public BanMiddleware(
        RequestDelegate next,
        IDatabaseCollectionProvider mongo,
        ILogger<BanMiddleware> logger
    ) {
        _next = next;
        _dbIpBans = mongo.GetCollection<DbIpBan>();
        _logger = logger;
    }
    
    public async Task Invoke(HttpContext context)
    {
        var remoteIp = context.Connection.RemoteIpAddress;

        var isIpBanned = (await _dbIpBans.Query()
            .Where(it => it.BannedUntil > DateTime.UtcNow)
            .ToListAsync())
        .Where(it => new Regex(it.IpAddressOrRegex).IsMatch(remoteIp.ToString()))
        .ToList();

        if (isIpBanned.Count > 0)
        {
            PreventBannedAccess(context);
            return;
        }
        
        
        var user = context.Items["User"] as DbUserModel;
        if (user != null)
        {
            if (user.BannedUntil > DateTime.UtcNow)
            {
                PreventBannedAccess(context);
                return;
            }
        }
        
        await _next.Invoke(context);
    }

    private void PreventBannedAccess(HttpContext context)
    {
        
        var username = (context.Items["User"] as DbUserModel)?.Username;
        var userId = (context.Items["User"] as DbUserModel)?.Id;
        var remoteIpAddress = context.Connection.RemoteIpAddress;
        var httpMethod = context.Request?.Method;
        var requestUrl = context.Request?.Path.Value;
        var queryString = context.Request?.QueryString;
        var statusCode = context.Response?.StatusCode;
            
        _logger.LogInformation(
            "BANNED ATTEMPT {user}(uuid={userId}, ip={ip}) {method} {url}{queryString} => {statusCode}",
            username,
            userId,
            remoteIpAddress,
            httpMethod,
            requestUrl,
            queryString,
            "403"
        );
        
        var response = new ResponseStatus()
        {
            IsSuccess = false,
            ResponseType = ResponseType.BANNED,
            Detail = "You are banned.",
        };
            
        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Response.WriteAsync(JsonConvert.SerializeObject(response));
    }
}