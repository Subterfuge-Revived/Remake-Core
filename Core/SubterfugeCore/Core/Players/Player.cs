using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Players.Currency;
using SubterfugeCore.Models.GameEvents;

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
        /// The number of mines the player has drilled.
        /// </summary>
        private int _numMinesBuilt;

        /// <summary>
        /// The player's total amount of neptunium mined
        /// </summary>
        private int _neptunium;

        /// <summary>
        /// The player's status in game; false if player is alive (i.e. controls their queen)
        /// </summary>
        private bool _isEliminated;

        /// <summary>
        /// The player's currency tracker
        /// </summary>
        public CurrencyManager CurrencyManager;

        /// <summary>
        /// Constructor to create an instance of a player based off of their player Id
        /// </summary>
        /// <param name="playerId">The player's ID in the database</param>
        public Player(string playerId)
        {
            this.PlayerId = playerId;
            this.PlayerName = playerId;
            this._numMinesBuilt = 0;
            this._neptunium = 0;
            this._isEliminated = false;
            this.CurrencyManager = new CurrencyManager();
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
            this._numMinesBuilt = 0;
            this._neptunium = 0;
            this._isEliminated = false;
            this.CurrencyManager = new CurrencyManager();
        }

        /// <summary>
        /// Creates a player from a protobuf player.
        /// </summary>
        /// <param name="user">protobuf player</param>
        public Player(User user)
        {
            this.PlayerId = user.Id;
            this.PlayerName = user.Username;
            this._numMinesBuilt = 0;
            this._neptunium = 0;
            this._isEliminated = false;
            this.CurrencyManager = new CurrencyManager();
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

        /// <summary>
        /// Increases or decreases the amount of neptunium the player has. Pass a negative value for neptunium to decease.
        /// </summary>
        /// <param name="neptunium">The value to change _neptunium by</param>
        public void AlterNeptunium(int neptunium)
        {
            this._neptunium += neptunium;
        }

        public int GetNeptunium()
        {
            return this._neptunium;
        }

        /// <summary>
        /// Increases or decreases the amount of mines the player has constructed. Pass a negative value to decrease the number of mines built.
        /// </summary>
        /// <param name="numMines">The value to change _numMinesBuilt by</param>
        public void AlterMinesDrilled(int numMines)
        {
            _numMinesBuilt += numMines;
        }

        public int GetMinesDrilled()
        {
            return _numMinesBuilt;
        }

        /// <summary>
        /// Determines the amount of drillers required to drill a new mine based on the amount of mines the player has already drilled. 
        /// </summary>
        /// <returns></returns>
        public int GetRequiredDrillersToMine()
        {
            if (GetMinesDrilled() < Constants.MiningCostInitial.Length)
            {
                return Constants.MiningCostInitial[GetMinesDrilled()];
            }
            else
            {
                return Constants.MiningCostInitial[Constants.MiningCostInitial.Length - 1] + (GetMinesDrilled() - Constants.MiningCostInitial.Length + 1) * Constants.MiningCostIncrease;
            }
        }

        public void SetEliminated(bool isEliminated)
        {
            this._isEliminated = isEliminated;
        }

        public bool IsEliminated()
        {
            return this._isEliminated;
        }

        public User ToUser()
        {
            return new User()
            {
                Id = PlayerId,
                Username = PlayerName,
            };
        }

        public override bool Equals(object obj)
        {
            var player = obj as Player;
            if (player is null)
            {
                return false;
            }

            return this == player;
        }

        public override int GetHashCode()
        {
            return (PlayerId != null ? PlayerId.GetHashCode() : 0);
        }

        public static bool operator ==(Player p1, Player p2)
        {
            if (p1 is null && p2 is null)
            {
                return true;
            }
            if (p1 is null || p2 is null)
            {
                return false;
            }
            return (p1.PlayerId == p2.PlayerId);
        }
        
        public static bool operator !=(Player p1, Player p2)
        {
            if (p1 is null && p2 is null)
            {
                return false;
            }
            if (p1 is null || p2 is null)
            {
                return true;
            }
            return (p1.PlayerId != p2.PlayerId);
        }
    }
}
