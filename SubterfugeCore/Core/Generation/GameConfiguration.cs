namespace SubterfugeCore.Core.Generation
{
    public class GameConfiguration
    {
        public int numPlayers { get; set; }
        public int outpostsPerPlayer { get; set; }
        public int seed { get; set; }
        public int minimumOutpostDistance { get; set; }
        public int maxiumumOutpostDistance { get; set; }
        public int dormantsPerPlayer { get; set; }
    }
}