using SubterfugeCore.Core.Network;

namespace SubterfugeCore.Core.Players
{
    /// <summary>
    /// An instance of a player
    /// </summary>
    public class Player
    {
        private string playerName { get;  }
        private int playerId { get; }

        /// <summary>
        /// Constructor to create an instance of a player based off of their player Id
        /// </summary>
        /// <param name="playerId">The player's ID in the database</param>
        public Player(int playerId)
        {
            this.playerId = playerId;
        }
        
        /// <summary>
        /// Constructor to create an instance of a player based off of their player Id and name
        /// </summary>
        /// <param name="playerId">The player's ID in the database</param>
        /// <param name="name">The player's name</param>
        public Player(int playerId, string name)
        {
            this.playerId = playerId;
            this.playerName = name;
        }

        public Player(NetworkUser networkUser)
        {
            this.playerId = networkUser.id;
            this.playerName = networkUser.name;
        }

        /// <summary>
        /// Gets the player's id
        /// </summary>
        /// <returns>The player's database ID</returns>
        public int getId()
        {
            return this.playerId;
        }

        /// <summary>
        /// Get the player's username
        /// </summary>
        /// <returns>The player's username</returns>
        public string getPlayerName()
        {
            return this.playerName;
        }
    }
}
