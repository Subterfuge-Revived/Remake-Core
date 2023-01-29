using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SubterfugeCore.Models.GameEvents
{
    [JsonConverter(typeof(JsonSubtypes), "EventDataType")]
    public class NetworkGameEventData
    {
        // This MUST be a string type, otherwise it does not work with JsonConverter to properly parse the data
        public virtual string EventDataType { get; }
    }

    public enum EventDataType
    {
        Unknown,
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
        public override string EventDataType { get; } = GameEvents.EventDataType.LaunchEventData.ToString();
        public string SourceId { get; set; }
        public string DestinationId { get; set; }
        public int DrillerCount { get; set; }
        public List<string> SpecialistIds { get; set; } = new List<string>();
    }

    public class ToggleShieldEventData : NetworkGameEventData
    {
        public override string EventDataType { get; } = GameEvents.EventDataType.ToggleShieldEventData.ToString();
        public string SourceId { get; set; }
    }
    
    public class DrillMineEventData : NetworkGameEventData
    {
        public override string EventDataType { get; } = GameEvents.EventDataType.DrillMineEventData.ToString();
        public String SourceId { get; set; }
    }

    public class PlayerLeaveGameEventData : NetworkGameEventData
    {
        public override string EventDataType { get; } = GameEvents.EventDataType.PlayerLeaveGameEventData.ToString();
        public SimpleUser Player { get; set; }
    }

    // Empty event. Will prevent the game from moving/ticking forwards until a following "Unpause game event data" is present in the 
    public class PauseGameEventData : NetworkGameEventData
    {
        public override string EventDataType { get; } = GameEvents.EventDataType.PauseGameEventData.ToString();
        public DateTime TimePaused { get; set; } = DateTime.Now;
    }

    public class UnpauseGameEventData : NetworkGameEventData
    {
        public override string EventDataType { get; } = GameEvents.EventDataType.UnpauseGameEventData.ToString();
        public DateTime TimeUnpaused { get; set; } = DateTime.Now;
    }

    public class GameEndEventData : NetworkGameEventData
    {
        public override string EventDataType { get; } = GameEvents.EventDataType.GameEndEventData.ToString();
        public DateTime GameEndAt { get; set; } = DateTime.Now;
        public bool ClosedByAdmin { get; set; } = false; 
        public SimpleUser? WinningPlayer { get; set; }
    }
}