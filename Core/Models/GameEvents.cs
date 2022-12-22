using System;
using System.Collections.Generic;

namespace SubterfugeCore.Models.GameEvents
{

    public abstract class NetworkGameEventData {}
    
    public class LaunchEventData : NetworkGameEventData 
    {
        public String SourceId { get; set; }
        public String DestinationId { get; set; }
        public int DrillerCount { get; set; }
        public List<String> SpecialistIds { get; set; }
    }

    public class ToggleShieldEventData : NetworkGameEventData
    {
        public String SourceId { get; set; }
    }
    
    public class DrillMineEventData : NetworkGameEventData
    {
        public String SourceId { get; set; }
    }

    public class PlayerLeaveGameEventData : NetworkGameEventData
    {
        public User player { get; set; }
    }
}