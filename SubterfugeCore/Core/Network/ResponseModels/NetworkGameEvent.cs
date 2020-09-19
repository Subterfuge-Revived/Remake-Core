using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class NetworkGameEvent
    {
        public int EventId { get; set; }
        public int TimeIssued { get; set; }
        public int OccursAt { get; set; }
        public int PlayerId { get; set; }
        public string EventMsg { get; set; }
    }
}