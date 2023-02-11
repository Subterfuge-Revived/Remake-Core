using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{
    // IMPORTANT! If new event types are added, ensure that DbGameEvent implements a serializer for the event data when it reads stores to the DB
    public enum EventDataType
    {
        Unknown = 0,
        LaunchEventData = 1,
        ToggleShieldEventData = 2,
        DrillMineEventData = 3,
        PlayerLeaveGameEventData = 4,
        
        // Admin or server-only events.
        // Validate that a player does not submit these.
        PauseGameEventData = 5,
        UnpauseGameEventData = 6,
        GameEndEventData = 7,
    }

    public class LaunchEventData
    {
        public string SourceId { get; set; }
        public string DestinationId { get; set; }
        public int DrillerCount { get; set; }
        public List<string> SpecialistIds { get; set; } = new List<string>();
    }

    public class ToggleShieldEventData
    {
        public string SourceId { get; set; }
    }
    
    public class DrillMineEventData
    {
        public String SourceId { get; set; }
    }

    public class PlayerLeaveGameEventData
    {
        public SimpleUser Player { get; set; }
    }

    // Empty event. Will prevent the game from moving/ticking forwards until a following "Unpause game event data" is present in the 
    public class PauseGameEventData
    {
        public DateTime TimePaused { get; set; } = DateTime.Now;
    }

    public class UnpauseGameEventData
    {
        public DateTime TimeUnpaused { get; set; } = DateTime.Now;
    }

    public class GameEndEventData
    {
        public DateTime GameEndAt { get; set; } = DateTime.Now;
        public bool ClosedByAdmin { get; set; } = false; 
        public SimpleUser? WinningPlayer { get; set; }
    }
}