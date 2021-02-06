using System.Collections.Generic;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Config;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Players
{
    /// <summary>
    /// An instance of a player
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The name or alias of the player
        /// </summary>
        private string PlayerName { get;  }

        /// <summary>
        /// The player's id
        /// </summary>
        private string PlayerId { get; }

        /// <summary>
        /// Constructor to create an instance of a player based off of their player Id
        /// </summary>
        /// <param name="playerId">The player's ID in the database</param>
        public Player(string playerId)
        {
            this.PlayerId = playerId;
        }

        /// <summary>
        /// Constructor to create an instance of a player based off of their player Id and name
        /// </summary>
        /// <param name="playerId">The player's ID in the database</param>
        /// <param name="name">The player's name</param>
        public Player(string playerId, string name)
        {
            this.PlayerId = playerId;
            this.PlayerName = name;
        }

        /// <summary>
        /// Creates a player from a protobuf player.
        /// </summary>
        /// <param name="user">protobuf player</param>
        public Player(User user)
        {
            this.PlayerId = user.Id;
            this.PlayerName = user.Username;
        }

        /// <summary>
        /// Gets the player's id
        /// </summary>
        /// <returns>The player's database ID</returns>
        public string GetId()
        {
            return this.PlayerId;
        }

        /// <summary>
        /// Get the player's username
        /// </summary>
        /// <returns>The player's username</returns>
        public string GetPlayerName()
        {
            return this.PlayerName;
        }
    }
}
