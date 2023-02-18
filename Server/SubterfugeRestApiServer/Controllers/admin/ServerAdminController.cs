using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;
using Subterfuge.Remake.Server.Database;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Controllers.admin;

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
    public async Task<SubterfugeResponse<ServerActionLogResponse>> GetActionLog([FromQuery] ServerActionLogRequeset request)
    {
        var query = _dbServerLog.Query();
        
        if (request.Username != null)
        {
            query = query.Where(it => (it.Username ?? string.Empty).ToLower().Contains(request.Username.ToLower()));
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

        return SubterfugeResponse<ServerActionLogResponse>.OfSuccess(new ServerActionLogResponse()
        {
            Actions = matchingServerActions,
        });
    }

    [HttpGet]
    [Route("exceptions")]
    public async Task<SubterfugeResponse<ServerExceptionLogResponse>> GetServerExceptions([FromQuery] ServerExceptionLogRequest request)
    {
        var query = _dbExceptionLog.Query();
    
        if (request.Username != null)
        {
            query = query.Where(it => (it.Username ?? string.Empty).ToLower().Contains(request.Username.ToLower()));
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

        return SubterfugeResponse<ServerExceptionLogResponse>.OfSuccess(new ServerExceptionLogResponse()
        {
            Exceptions = matchingServerActions
        });
    }

    [HttpPost]
    [Route("banPlayer")]
    public async Task<SubterfugeResponse<BanPlayerResponse>> BanPlayer([FromBody] BanPlayerRequest banPlayerRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<BanPlayerResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<BanPlayerResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Must be an administrator to do this");
        
        DbUserModel? targetUser = await _dbUsers.Query().FirstOrDefaultAsync(it => it.Id == banPlayerRequest.UserId);
        if (targetUser == null)
            return SubterfugeResponse<BanPlayerResponse>.OfFailure(ResponseType.NOT_FOUND, "Player to ban not found");

        targetUser.BanHistory.Add(new AccountBan()
        {
            AdministratorNotes = banPlayerRequest.AdminNotes,
            DateExpires = banPlayerRequest.Until,
            Reason = banPlayerRequest.Reason
        });

        if (targetUser.BannedUntil <= banPlayerRequest.Until)
            targetUser.BannedUntil = banPlayerRequest.Until;
        
        await _dbUsers.Upsert(targetUser);
        
        return SubterfugeResponse<BanPlayerResponse>.OfSuccess(new BanPlayerResponse()
        {
            wasSuccess = true,
        });
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [Route("banIp")]
    public async Task<SubterfugeResponse<BanIpResponse>> BanIp([FromBody] BanIpRequest banIpRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<BanIpResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<BanIpResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Must be an Administrator.");

        var newBan = new DbIpBan()
        {
            Reason = banIpRequest.Reason,
            BannedUntil = banIpRequest.Until,
            IpAddressOrRegex = banIpRequest.DirectIpOrRegex,
            AdminNotes = banIpRequest.AdminNotes,
        };
        
        await _dbIpBans.Upsert(newBan);

        return SubterfugeResponse<BanIpResponse>.OfSuccess(new BanIpResponse()
        {
            wasSuccess = true,
        });
    }

    [HttpGet]
    [Route("ipBans")]
    public async Task<SubterfugeResponse<GetIpBansResponse>> GetIpBans(int pagination = 1)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<GetIpBansResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<GetIpBansResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Must be an Administrator.");

        var ipBans = (await _dbIpBans.Query()
            .OrderByDescending(it => it.DateApplied)
            .Skip((pagination - 1) * 50)
            .Take(50)
            .ToListAsync())
            .Select(it => it.ToIpBan())
            .ToList();

        return SubterfugeResponse<GetIpBansResponse>.OfSuccess(new GetIpBansResponse()
        {
            BannedIps = ipBans
        });
    }

    [HttpGet]
    [Route("playerBans")]
    public async Task<SubterfugeResponse<GetBannedPlayerResponse>> GetBannedPlayers(int pagination = 1)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<GetBannedPlayerResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<GetBannedPlayerResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Must be an Administrator.");

        var bannedPlayers = (await _dbUsers.Query()
                .OrderByDescending(it => it.BannedUntil)
                .Where(it => it.BannedUntil > DateTime.UtcNow)
                .Skip((pagination - 1) * 50)
                .Take(50)
                .ToListAsync())
            .Select(it => it.ToDetailedUser())
            .ToList();

        return SubterfugeResponse<GetBannedPlayerResponse>.OfSuccess(new GetBannedPlayerResponse()
        {
            BannedUsers = bannedPlayers
        });
    }

    [HttpPost]
    [Route("echo")]
    public async Task<SubterfugeResponse<Echo>> EchoRequest(Echo request)
    {
        return SubterfugeResponse<Echo>.OfSuccess(request);
    }
}