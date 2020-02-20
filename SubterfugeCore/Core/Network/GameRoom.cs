using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    public class GameRoom
    {
        public int creator_id { get; set; }
        public bool rated { get; set; }
        public int player_count { get; set; }
        public int min_rating { get; set; }
        public string description { get; set; }
        public int goal { get; set; }
        public bool anonimity { get; set; }
        public int map { get; set; }
        public int seed { get; set; }
        public List<NetworkUser> players { get; set; }
    }
}