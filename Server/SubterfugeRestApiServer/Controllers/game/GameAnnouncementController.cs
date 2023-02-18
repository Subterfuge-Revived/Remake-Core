using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;
using Subterfuge.Remake.Server.Database;
using Subterfuge.Remake.Server.Database.Models;

namespace Subterfuge.Remake.Server.Controllers.game;

[ApiController]
[Authorize]
[Route("api/announcements/")]
public class GameAnnouncementController : ControllerBase, ISubterfugeAnnouncementApi
{

    private IDatabaseCollection<DbGameAnnouncement> _dbGameAnnouncements;

    public GameAnnouncementController(IDatabaseCollectionProvider mongo)
    {
        this._dbGameAnnouncements = mongo.GetCollection<DbGameAnnouncement>();
    }

    [HttpPost]
    [Route("create")]
    [Authorize(Roles = "Administrator")]
    public async Task<SubterfugeResponse<CreateAnnouncementResponse>> CreateAnnouncement(CreateAnnouncementRequest announcementRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<CreateAnnouncementResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<CreateAnnouncementResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Must be an administrator to do this.");

        var model = DbGameAnnouncement.fromGameAnnouncementRequest(announcementRequest, dbUserModel.ToSimpleUser());
        await _dbGameAnnouncements.Upsert(model);

        return SubterfugeResponse<CreateAnnouncementResponse>.OfSuccess(new CreateAnnouncementResponse()
        {
            AnnouncementId = model.Id,
        });
    }
    
    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<SubterfugeResponse<CreateAnnouncementResponse>> UpdateAnnouncement(string id, CreateAnnouncementRequest announcementRequest)
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<CreateAnnouncementResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
            return SubterfugeResponse<CreateAnnouncementResponse>.OfFailure(ResponseType.PERMISSION_DENIED, "Must be an administrator to do this.");

        DbGameAnnouncement announcement = await _dbGameAnnouncements.Query()
            .Where(announcement => announcement.Id == id)
            .FirstAsync();

        if (announcement == null)
            return SubterfugeResponse<CreateAnnouncementResponse>.OfFailure(ResponseType.NOT_FOUND, "Must be an administrator to do this.");

        announcement.Message = announcementRequest.Message;
        announcement.Title = announcementRequest.Title;
        announcement.ExpiresAt = announcementRequest.ExpiresAt;
        announcement.StartsAt = announcementRequest.StartsAt;
        announcement.BroadcastTo = announcementRequest.BroadcastTo;
        
        await _dbGameAnnouncements.Upsert(announcement);

        return SubterfugeResponse<CreateAnnouncementResponse>.OfSuccess(new CreateAnnouncementResponse()
        {
            AnnouncementId = announcement.Id,
        });
    }

    [HttpGet]
    public async Task<SubterfugeResponse<GetAnnouncementsResponse>> GetAnnouncements()
    {
        DbUserModel? dbUserModel = HttpContext.Items["User"] as DbUserModel;
        if (dbUserModel == null)
            return SubterfugeResponse<GetAnnouncementsResponse>.OfFailure(ResponseType.UNAUTHORIZED, "Not logged in");

        var query = _dbGameAnnouncements.Query();

        if (!dbUserModel.HasClaim(UserClaim.Administrator))
        {
            var now = DateTime.UtcNow;
            // If not an admin, only show active announcements for the logged in user.
            query = query.Where(it => it.StartsAt < now)
                .Where(it => it.ExpiresAt > now)
                .Where(it => it.BroadcastTo.Contains("global") || it.BroadcastTo.Contains(dbUserModel.Id));
        }

        var results = (await query.ToListAsync())
            .Select(it => it.toGameAnnouncement())
            .ToList();

        return SubterfugeResponse<GetAnnouncementsResponse>.OfSuccess(new GetAnnouncementsResponse()
        {
            Announcements = results
        });
    }
}