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

    // Make all of these classes have a common base
    public class EventData { }

    public class LaunchEventData : EventData
    {
        public string SourceId { get; set; }
        public string DestinationId { get; set; }
        public int DrillerCount { get; set; }
        public List<string> SpecialistIds { get; set; } = new List<string>();
    }

    public class ToggleShieldEventData : EventData
    {
        public string SourceId { get; set; }
    }
    
    public class DrillMineEventData : EventData
    {
        public String SourceId { get; set; }
    }

    public class PlayerLeaveGameEventData : EventData
    {
        public SimpleUser Player { get; set; }
    }

    // Empty event. Will prevent the game from moving/ticking forwards until a following "Unpause game event data" is present in the 
    public class PauseGameEventData : EventData
    {
        public DateTime TimePaused { get; set; } = DateTime.Now;
    }

    public class UnpauseGameEventData : EventData
    {
        public DateTime TimeUnpaused { get; set; } = DateTime.Now;
    }

    public class GameEndEventData : EventData
    {
        public DateTime GameEndAt { get; set; } = DateTime.Now;
        public bool ClosedByAdmin { get; set; } = false; 
        public SimpleUser? WinningPlayer { get; set; }
    }

    public class HireSpecialistEventData : EventData
    {
        public string HireLocation { get; set; }
        public SpecialistIds SpecialistIdHired { get; set; }
    }

    public class PromoteSpecialistEventData : EventData
    {
        public string PromotionLocation { get; set; }
        public string SpecialistIdPromotedFrom { get; set; }
        public string SpecialistIdPromotedTo { get; set; }
    }
}