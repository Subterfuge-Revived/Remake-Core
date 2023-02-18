using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Server.Database.Models;

public class DbGameEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimeIssued { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
    public string RoomId { get; set; }
    public User IssuedBy { get; set; }
    public int OccursAtTick { get; set; }
    public EventDataType GameEventType { get; set; }
    
    public String SerializedEventData { get; set; }


    public GameRoomEvent ToGameEventData()
    {
        return new GameRoomEvent()
        {
            Id = Id,
            RoomId = RoomId,
            TimeIssued = TimeIssued,
            IssuedBy = IssuedBy,
            GameEventData = new GameEventData()
            {
                EventDataType = GameEventType,
                OccursAtTick = OccursAtTick,
                SerializedEventData = SerializedEventData
            },
        };
    }

    public static DbGameEvent FromGameEventRequest(SubmitGameEventRequest request, User issuer, string roomId)
    {
        return new DbGameEvent()
        {
            RoomId = roomId,
            IssuedBy = issuer,
            OccursAtTick = request.GameEventData.OccursAtTick,
            GameEventType = request.GameEventData.EventDataType,
            SerializedEventData = request.GameEventData.SerializedEventData,
        };
    }
}