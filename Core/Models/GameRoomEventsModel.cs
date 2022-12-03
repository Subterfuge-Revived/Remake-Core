using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{

    public interface INetworkGameEventController
    {
        GetGameRoomEventsResponse GetGameRoomEvents(GetGameRoomEventsRequest gameRoomEventsRequest);
        SubmitGameEventResponse SubmitGameEvent(SubmitGameEventRequest submitGameEventRequest);
        SubmitGameEventResponse UpdateGameEvent(UpdateGameEventRequest updateGameEventRequest);
        DeleteGameEventResponse DeleteGameEvent(DeleteGameEventRequest deleteGameEventRequest);
    }

    public class GameEventData
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public long UnixTimeIssued { get; set; }
        public int OccursAtTick { get; set; }
        public User IssuedBy { get; set; }
        public NetworkGameEventData EventData { get; set; }
    }

    public class GameEventRequest
    {
        public int OccursAtTick { get; set; }
        public NetworkGameEventData EventData { get; set; }
    }
    
    public class GetGameRoomEventsRequest
    {
        public string RoomId { get; set; }
        public long AfterTick { get; set; }
    }

    public class GetGameRoomEventsResponse : NetworkResponse
    {
        public List<GameEventData> GameEvents { get; set; }
    }

    public class SubmitGameEventRequest
    {
        public string RoomId { get; set; }
        public GameEventRequest GameEventRequest { get; set; }
    }
    
    public class SubmitGameEventResponse : NetworkResponse
    {
        public string EventId { get; set; }
        public GameEventData GameEventData { get; set; }
    }

    public class UpdateGameEventRequest
    {
        public string RoomId { get; set; }
        public string EventId { get; set; }
        public GameEventRequest GameEventRequest { get; set; }
    }

    public class DeleteGameEventRequest
    {
        public string RoomId { get; set; }
        public string EventId { get; set; }
    }

    public class DeleteGameEventResponse : NetworkResponse { }

}