using Newtonsoft.Json;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeDatabaseProvider.Models;

public class DbGameEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime TimeIssued { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.MaxValue;
    public string RoomId { get; set; }
    public User IssuedBy { get; set; }
    public int OccursAtTick { get; set; }
    public String GameEventType { get; set; }
    
    public String SerializedEventData { get; set; }


    public GameRoomEvent ToGameEventData()
    {
        // Determine if the user is trying to submit an admin-only game event:
        EventDataType eventType = EventDataType.Unknown;
        EventDataType.TryParse(GameEventType, true, out eventType);

        GameEventData data = new GameEventData()
        {
            OccursAtTick = OccursAtTick,
            EventData = null,
        };

        switch (eventType)
        {
            case EventDataType.LaunchEventData:
                data.EventData = JsonConvert.DeserializeObject<LaunchEventData>(SerializedEventData);
                break;
            case EventDataType.DrillMineEventData:
                data.EventData = JsonConvert.DeserializeObject<DrillMineEventData>(SerializedEventData);
                break;
            case EventDataType.GameEndEventData:
                data.EventData = JsonConvert.DeserializeObject<GameEndEventData>(SerializedEventData);
                break;
            case EventDataType.PauseGameEventData:
                data.EventData = JsonConvert.DeserializeObject<PauseGameEventData>(SerializedEventData);
                break;
            case EventDataType.ToggleShieldEventData:
                data.EventData = JsonConvert.DeserializeObject<ToggleShieldEventData>(SerializedEventData);
                break;
            case EventDataType.UnpauseGameEventData:
                data.EventData = JsonConvert.DeserializeObject<UnpauseGameEventData>(SerializedEventData);
                break;
            case EventDataType.PlayerLeaveGameEventData:
                data.EventData = JsonConvert.DeserializeObject<PlayerLeaveGameEventData>(SerializedEventData);
                break;
            case EventDataType.Unknown:
            default:
                break;
                
        }
        
        return new GameRoomEvent()
        {
            Id = Id,
            RoomId = RoomId,
            TimeIssued = TimeIssued,
            IssuedBy = IssuedBy,
            GameEventData = data,
        };
    }

    public static DbGameEvent FromGameEventRequest(SubmitGameEventRequest request, User issuer, string roomId)
    {
        return new DbGameEvent()
        {
            RoomId = roomId,
            IssuedBy = issuer,
            OccursAtTick = request.GameEventData.OccursAtTick,
            GameEventType = request.GameEventData.EventData.EventDataType,
            SerializedEventData = JsonConvert.SerializeObject(request.GameEventData.EventData),
        };
    }
}