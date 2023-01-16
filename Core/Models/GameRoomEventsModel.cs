using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    public class GameEventData
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public DateTime TimeIssued { get; set; }
        public int OccursAtTick { get; set; }
        public User IssuedBy { get; set; }
        public NetworkGameEventData EventData { get; set; }
    }

    public class GameEventRequest
    {
        public int OccursAtTick { get; set; }
        public NetworkGameEventData EventData { get; set; }
    }

    public class GetGameRoomEventsResponse : NetworkResponse
    {
        public List<GameEventData> GameEvents { get; set; }
    }

    public class SubmitGameEventRequest
    {
        public GameEventRequest GameEventRequest { get; set; }
    }
    
    public class SubmitGameEventResponse : NetworkResponse
    {
        public string EventId { get; set; }
        public GameEventData GameEventData { get; set; }
    }

    public class UpdateGameEventRequest
    {
        public GameEventRequest GameEventRequest { get; set; }
    }

    public class DeleteGameEventResponse : NetworkResponse { }

}