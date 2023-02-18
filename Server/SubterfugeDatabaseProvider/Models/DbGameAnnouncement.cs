using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Server.Database.Models;

public class DbGameAnnouncement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Message { get; set; }
    public SimpleUser PostedBy { get; set; }
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(21);
    public string BroadcastTo { get; set; } = "global";

    public static DbGameAnnouncement fromGameAnnouncementRequest(CreateAnnouncementRequest request, SimpleUser PostedBy)
    {
        return new DbGameAnnouncement()
        {
            Title = request.Title,
            Message = request.Message,
            PostedBy = PostedBy,
            BroadcastTo = request.BroadcastTo,
            StartsAt = request.StartsAt,
            ExpiresAt = request.ExpiresAt
        };
    }

    public GameAnnouncement toGameAnnouncement()
    {
        return new GameAnnouncement()
        {
            Id = Id,
            ExpiresAt = ExpiresAt,
            Message = Message,
            Title = Title,
            PostedBy = PostedBy,
            StartsAt = StartsAt,
        };
    }
}