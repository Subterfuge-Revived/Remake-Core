using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Generation
{
    /// <summary>
    /// The GameConfiguration class allows configuring the game before it begins. This allows setting up map generation
    /// parameters, the number of players, and more.
    /// </summary>
    public class GameConfiguration
    {
        /// <summary>
        /// The constructor for the GameConfiguration object requires a list of players to be passed in.
        /// This is because you can never configure a game with zero players.
        /// </summary>
        /// <param name="players">A list of the players in the game</param>
        public GameConfiguration(List<Player> players)
        {
            // Requires a list of players so that map generation can get the appropriate map & link ownership when generated.
            this.Players = players;
        }

        /// <summary>
        /// A list of players in the game 
        /// </summary>
        public List<Player> Players { get; }

        /// <summary>
        /// The number of outposts that each player should be assigned when the map is generated
        /// </summary>
        public int OutpostsPerPlayer { get; set; } = 5;

        /// <summary>
        /// The seed to be used for the random number generator when creating the map.
        /// </summary>
        public int Seed { get; set; } = 0;
        
        /// <summary>
        /// The closest possible distance that two outposts can be together before trying to re-evaluate a new position.
        /// </summary>
        public int MinimumOutpostDistance { get; set; }
        
        /// <summary>
        /// The farthest away two outposts can be. Outposts are generated around a central outpost so this is
        /// essentially the radius of the player's territory. Ex. setting this to 100 means that the total size
        /// of a player's territory is up to 200x200
        /// </summary>
        public int MaxiumumOutpostDistance { get; set; }

        /// <summary>
        /// The number of dormant outposts to generate within each player's outposts. These are additional un-owned
        /// outposts that are generated within the player's territory. These outposts are still generated using the
        /// generation parameters above so the total number of outposts in a player's territory is equal to
        /// dormantsPerPlayer + outpostsPerPlayer. The dormants are assigned to the farthest outposts from the center
        /// of the player's territory.
        /// </summary>
        public int DormantsPerPlayer { get; set; } = 1;
    }
}