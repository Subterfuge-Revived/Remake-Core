using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbGameEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimeIssued { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
    public string RoomId { get; set; }
    public User IssuedBy { get; set; }
    public GameEventData EventData { get; set; }

    public GameRoomEvent ToGameEventData()
    {
        return new GameRoomEvent()
        {
            Id = Id,
            RoomId = RoomId,
            TimeIssued = TimeIssued,
            IssuedBy = IssuedBy,
            GameEventData = EventData,
        };
    }

    public static DbGameEvent FromGameEventRequest(SubmitGameEventRequest request, User issuer, string roomId)
    {
        return new DbGameEvent()
        {
            RoomId = roomId,
            IssuedBy = issuer,
            EventData = request.GameEventData
        };
    }
}