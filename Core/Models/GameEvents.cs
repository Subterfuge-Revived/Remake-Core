using System;
using System.Collections.Generic;
using JsonSubTypes;
using Newtonsoft.Json;

namespace SubterfugeCore.Models.GameEvents
{
    [JsonConverter(typeof(JsonSubtypes), "EventDataType")]
    public class NetworkGameEventData
    {
        // Used to determine which event to serialize / deserialize as a REST request will not know the class type
        public virtual EventDataType EventDataType { get; }
    }

    public enum EventDataType
    {
        LaunchEventData,
        ToggleShieldEventData,
        DrillMineEventData,
        PlayerLeaveGameEventData,
        
        // Admin or server-only events.
        // Validate that a player does not submit these.
        PauseGameEventData,
        UnpauseGameEventData,
        GameEndEventData,
    }

    public class LaunchEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.LaunchEventData;
        public string SourceId { get; set; }
        public string DestinationId { get; set; }
        public int DrillerCount { get; set; }
        public List<string> SpecialistIds { get; set; } = new List<string>();
    }

    public class ToggleShieldEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.ToggleShieldEventData;
        public string SourceId { get; set; }
    }
    
    public class DrillMineEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.DrillMineEventData;
        public String SourceId { get; set; }
    }

    public class PlayerLeaveGameEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.PlayerLeaveGameEventData;
        public SimpleUser Player { get; set; }
    }

    // Empty event. Will prevent the game from moving/ticking forwards until a following "Unpause game event data" is present in the 
    public class PauseGameEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.PauseGameEventData;
        public DateTime TimePaused { get; set; } = DateTime.Now;
    }

    public class UnpauseGameEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.UnpauseGameEventData;
        public DateTime TimeUnpaused { get; set; } = DateTime.Now;
    }

    public class GameEndEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.GameEndEventData;
        public DateTime GameEndAt { get; set; } = DateTime.Now;
        public bool ClosedByAdmin { get; set; } = false; 
        public SimpleUser? WinningPlayer { get; set; }
    }
}