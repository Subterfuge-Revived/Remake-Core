using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.Generation
{
    /// <summary>
    /// The GameConfiguration class allows configuring the game before it begins. This allows setting up map generation
    /// parameters, the number of players, and more.
    /// </summary>
    public class GameConfiguration
    {
        
        /// <summary>
        /// A list of players in the game 
        /// </summary>
        public readonly List<Player> Players;

        /// <summary>
        /// A list of players in the game 
        /// </summary>
        public readonly DateTime StartTime;

        /// <summary>
        /// Map generation configuration
        /// </summary>
        public readonly MapConfiguration MapConfiguration;
        
        /// <summary>
        /// The constructor for the GameConfiguration object requires a list of players to be passed in.
        /// This is because you can never configure a game with zero players.
        /// </summary>
        /// <param name="players">A list of the players in the game</param>
        public GameConfiguration(List<Player> players, DateTime startTime, MapConfiguration configuration)
        {
            // Requires a list of players so that map generation can get the appropriate map & link ownership when generated.
            this.Players = players;
            this.StartTime = startTime;
            this.MapConfiguration = configuration;
        }
    }
}