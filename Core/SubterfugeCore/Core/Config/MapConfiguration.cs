using System.Collections.Generic;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Config
{
    public class MapConfiguration
    {
        /// <summary>
        /// The number of outposts that each player should be assigned when the map is generated
        /// </summary>
        public int OutpostsPerPlayer { get; set; } = 3;

        /// <summary>
        /// The seed to be used for the random number generator when creating the map.
        /// </summary>
        public int Seed { get; set; } = 0;

        //Maybe make these distances dependent on sonar range?

        /// <summary>
        /// The closest possible distance that two outposts can be together before trying to re-evaluate a new position.
        /// </summary>
        public int MinimumOutpostDistance { get; set; } = 30;

        /// <summary>
        /// The farthest away two outposts can be. Outposts are generated around a central outpost so this is
        /// essentially the radius of the player's territory. Ex. setting this to 100 means that the total size
        /// of a player's territory is up to 200x200
        /// </summary>
        public int MaxiumumOutpostDistance { get; set; } = 180;

        /// <summary>
        /// The number of dormant outposts to generate within each player's outposts. These are additional un-owned
        /// outposts that are generated within the player's territory. These outposts are still generated using the
        /// generation parameters above so the total number of outposts in a player's territory is equal to
        /// dormantsPerPlayer + outpostsPerPlayer. The dormants are assigned to the farthest outposts from the center
        /// of the player's territory.
        /// </summary>
        public int DormantsPerPlayer { get; set; } = 6;
        
        /// <summary>
        /// The number of dormant outposts to generate within each player's outposts. These are additional un-owned
        /// outposts that are generated within the player's territory. These outposts are still generated using the
        /// generation parameters above so the total number of outposts in a player's territory is equal to
        /// dormantsPerPlayer + outpostsPerPlayer. The dormants are assigned to the farthest outposts from the center
        /// of the player's territory.
        /// </summary>
        public List<Player> players { get; set; }
        
        public MapConfiguration(List<Player> players)
        {
            this.players = players;
        }
    }
}