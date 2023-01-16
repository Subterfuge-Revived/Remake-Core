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
    }
    
    public class LaunchEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.LaunchEventData;
        public String SourceId { get; set; }
        public String DestinationId { get; set; }
        public int DrillerCount { get; set; }
        public List<String> SpecialistIds { get; set; } = new List<string>();
    }

    public class ToggleShieldEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.ToggleShieldEventData;
        public String SourceId { get; set; }
    }
    
    public class DrillMineEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.DrillMineEventData;
        public String SourceId { get; set; }
    }

    public class PlayerLeaveGameEventData : NetworkGameEventData
    {
        public override EventDataType EventDataType { get; } = EventDataType.PlayerLeaveGameEventData;
        public User Player { get; set; }
    }
}