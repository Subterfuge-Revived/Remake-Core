using System.Collections.Generic;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Generation
{
    public class GameConfiguration
    {
        public GameConfiguration(List<Player> players)
        {
            // Requires a list of players so that map generation can get the appropriate map & link ownership when generated.
            this.players = players;
        }
        public List<Player> players { get; }
        public int outpostsPerPlayer { get; set; }
        public int seed { get; set; }
        public int minimumOutpostDistance { get; set; }
        public int maxiumumOutpostDistance { get; set; }
        public int dormantsPerPlayer { get; set; }
    }
}