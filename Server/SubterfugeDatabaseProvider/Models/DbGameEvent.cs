using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbGameEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimeIssued { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string RoomId { get; set; }
    public int OccursAtTick { get; set; }
    public User IssuedBy { get; set; }
    public NetworkGameEventData EventData { get; set; }

    public GameEventData ToGameEventData()
    {
        return new GameEventData()
        {
            Id = Id,
            RoomId = RoomId,
            TimeIssued = TimeIssued,
            OccursAtTick = OccursAtTick,
            IssuedBy = IssuedBy,
            EventData = EventData
        };
    }

    public static DbGameEvent FromGameEventRequest(SubmitGameEventRequest request, User issuer, string roomId)
    {
        return new DbGameEvent()
        {
            RoomId = roomId,
            OccursAtTick = request.GameEventRequest.OccursAtTick,
            IssuedBy = issuer,
            EventData = request.GameEventRequest.EventData
        };
    }
}