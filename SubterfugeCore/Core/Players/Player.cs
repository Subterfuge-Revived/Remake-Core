using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Network;

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
        private string playerName { get;  }
        
        /// <summary>
        /// The player's id
        /// </summary>
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
        /// Checks if the player's queen is alive at the current game tick.
        /// </summary>
        /// <returns>If the player's queen is alive</returns>
        public bool isAlive()
        {
            List<Specialist> playerSpecs = Game.timeMachine.getState().getPlayerSpecialists(this);
            
            // Find the player's queen.
            foreach (Specialist spec in playerSpecs)
            {
                Queen playerQueen = spec as Queen;
                if (playerQueen != null)
                {
                    return playerQueen.isCaptured;
                }
            }

            // Player doesn't have a queen. Odd but possible if stolen.
            return false;
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
