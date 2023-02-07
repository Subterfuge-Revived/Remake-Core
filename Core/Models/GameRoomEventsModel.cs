using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public class GameRoomEvent
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public DateTime TimeIssued { get; set; }
        public User IssuedBy { get; set; }
        
        public GameEventData GameEventData { get; set; }
    }

    public class GameEventData
    {
        public int OccursAtTick { get; set; }
        public EventDataType EventDataType { get; set; }
        public String SerializedEventData { get; set; }
    }

    public class GetGameRoomEventsResponse : NetworkResponse
    {
        public List<GameRoomEvent> GameEvents { get; set; }
    }

    public class SubmitGameEventRequest
    {
        public GameEventData GameEventData { get; set; }
    }
    
    public class SubmitGameEventResponse : NetworkResponse
    {
        public string EventId { get; set; }
        public GameRoomEvent GameRoomEvent { get; set; }
    }

    public class UpdateGameEventRequest
    {
        public GameEventData GameEventData { get; set; }
    }

    public class DeleteGameEventResponse : NetworkResponse { }

}