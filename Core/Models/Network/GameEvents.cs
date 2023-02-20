using System;
using System.Collections.Generic;

namespace Subterfuge.Remake.Api.Network
{
    // IMPORTANT! If new event types are added, ensure that SubterfugeGameEventController implements a deserializer for the event data to verify it
    // Also ensure that GameEventFactory has a deserializer to convert the event into a game event.
    public enum EventDataType
    {
        Unknown = 0,
        LaunchEventData = 1,
        ToggleShieldEventData = 2,
        DrillMineEventData = 3,
        PlayerLeaveGameEventData = 4,
        PauseGameEventData = 5, // Admin Only
        UnpauseGameEventData = 6, // Admin Only
        GameEndEventData = 7, // Admin Only
        HireSpecialistEventData = 8,
        PromoteSpecialistEventData = 9,
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

    public class HireSpecialistEventData
    {
        public string HireLocation { get; set; }
        public string SpecialistIdHired { get; set; }
    }

    public class PromoteSpecialistEventData
    {
        public string PromotionLocation { get; set; }
        public string SpecialistIdPromoted { get; set; }
    }
}