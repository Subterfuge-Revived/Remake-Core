using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class NetworkGameEvent
    {
        public int event_id { get; set; }
        public int time_issued { get; set; }
        public int occurs_at { get; set; }
        public int player_id { get; set; }
        public string event_msg { get; set; }
    }
}