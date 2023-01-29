using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeDatabaseProvider.Models;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Collections;
using SubterfugeServerConsole.Responses;

namespace SubterfugeRestApiServer.Controllers.admin;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/admin/")]
public class SubterfugeAdminController : ControllerBase, ISubterfugeAdminApi
{
    
    private IDatabaseCollection<DbServerAction> _dbServerLog;
    private IDatabaseCollection<DbServerException> _dbExceptionLog;
    private IDatabaseCollection<DbUserModel> _dbUsers;
    private IDatabaseCollection<DbIpBan> _dbIpBans;

    public SubterfugeAdminController(IDatabaseCollectionProvider mongo)
    {
        this._dbServerLog = mongo.GetCollection<DbServerAction>();
        this._dbExceptionLog = mongo.GetCollection<DbServerException>();
        this._dbUsers = mongo.GetCollection<DbUserModel>();
        this._dbIpBans = mongo.GetCollection<DbIpBan>();
    }

    [HttpGet]
    [Route("serverLog")]
    public async Task<ServerActionLogResponse> GetActionLog([FromQuery] ServerActionLogRequeset request)
    {
        var query = _dbServerLog.Query();
        
        if (request.Username != null)
        {
            query = query.Where(it => it.Username.Contains(request.Username));
        }

        if (request.UserId != null)
        {
            query = query.Where(it => it.UserId == request.UserId);
        }
        
        if (request.HttpMethod != null)
        {
            query = query.Where(it => it.HttpMethod == request.HttpMethod);
        }
        
        if (request.RequestUrl != null)
        {
            query = query.Where(it => it.RequestUrl == request.RequestUrl);
        }

        var matchingServerActions = (await query
                .OrderByDescending(it => it.TimesAccessed)
                .Skip(50 * (request.Pagination - 1))
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToServerAction())
            .ToList();

        return new ServerActionLogResponse()
        {
            Actions = matchingServerActions,
        };
    }

    [HttpGet]
    [Route("exceptions")]
    public async Task<ServerExceptionLogResponse> GetServerExceptions([FromQuery] ServerExceptionLogRequest request)
    {
        var query = _dbExceptionLog.Query();
    
        if (request.Username != null)
        {
            query = query.Where(it => it.Username.Contains(request.Username));
        }

        if (request.UserId != null)
        {
            query = query.Where(it => it.UserId == request.UserId);
        }
    
        if (request.HttpMethod != null)
        {
            query = query.Where(it => it.HttpMethod == request.HttpMethod);
        }
    
        if (request.RequestUrl != null)
        {
            query = query.Where(it => it.RequestUri == request.RequestUrl);
        }

        if (request.ExceptionSource != null)
        {
            query = query.Where(it => it.ExceptionSource == request.ExceptionSource);
        }

        if (request.RemoteIpAddress != null)
        {
            query = query.Where(it => it.RemoteIpAddress == request.RemoteIpAddress);
        }

        var matchingServerActions = (await query
                .OrderByDescending(it => it.UnixTimeProcessed)
                .Skip(50 * (request.Pagination - 1))
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToServerException())
            .ToList();

        return new ServerExceptionLogResponse()
        {
            Exceptions = matchingServerActions
        };
    }

    [HttpPost]
    [Route("banPlayer")]
    public async Task<NetworkResponse> BanPlayer(string userId, DateTime until, string reason, string adminNotes)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            throw new ForbidException();
        
        DbUserModel? targetUser = await _dbUsers.Query().FirstOrDefaultAsync(it => it.Id == userId);
        if (targetUser == null)
            throw new NotFoundException("User not found.");

        targetUser.BanHistory.Add(new AccountBan()
        {
            AdministratorNotes = adminNotes,
            DateExpires = until,
            Reason = reason
        });

        if (targetUser.BannedUntil <= until)
            targetUser.BannedUntil = until;
        
        await _dbUsers.Upsert(targetUser);
        
        return new NetworkResponse()
        {
            Status = new ResponseStatus()
            {
                Detail = $"Successfully banned {userId} until {until}",
                IsSuccess = true,
                ResponseType = ResponseType.SUCCESS
            }
        };
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [Route("banIp")]
    public async Task<NetworkResponse> BanIp(string directIpOrRegex, DateTime until, string adminNotes)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            throw new ForbidException();

        var newBan = new DbIpBan()
        {
            BannedUntil = until,
            IpAddressOrRegex = directIpOrRegex,
            AdminNotes = adminNotes,
        };
        
        await _dbIpBans.Upsert(newBan);

        return new NetworkResponse()
        {
            Status = new ResponseStatus()
            {
                Detail = $"Successfully banned {directIpOrRegex} until {until}",
                IsSuccess = true,
                ResponseType = ResponseType.SUCCESS
            }
        };
    }

    [HttpGet]
    [Route("ipBans")]
    public async Task<GetIpBansResponse> GetIpBans(int pagination = 1)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            throw new ForbidException();

        var ipBans = (await _dbIpBans.Query()
            .OrderByDescending(it => it.DateApplied)
            .Skip((pagination - 1) * 50)
            .Take(50)
            .ToListAsync())
            .Select(it => it.ToIpBan())
            .ToList();

        return new GetIpBansResponse()
        {
            BannedIps = ipBans
        };
    }

    [HttpGet]
    [Route("playerBans")]
    public async Task<GetBannedPlayerResponse> GetBannedPlayers(int pagination = 1)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            throw new UnauthorizedException();

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            throw new ForbidException();

        var bannedPlayers = (await _dbUsers.Query()
                .OrderByDescending(it => it.BannedUntil)
                .Where(it => it.BannedUntil > DateTime.UtcNow)
                .Skip((pagination - 1) * 50)
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToDetailedUser())
            .ToList();

        return new GetBannedPlayerResponse()
        {
            BannedUsers = bannedPlayers
        };
    }

    [HttpPost]
    [Route("echo")]
    public async Task<Echo> EchoRequest(Echo request)
    {
        return request;
    }
}