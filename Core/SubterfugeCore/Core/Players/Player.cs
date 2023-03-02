using System;
using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.Players.Currency;

namespace Subterfuge.Remake.Core.Players
{
    /// <summary>
    /// An instance of a player
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The name or alias of the player
        /// </summary>
        public SimpleUser PlayerInstance { get; }

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

        public SpecialistPool SpecialistPool;

        /// <summary>
        /// Creates a player from a protobuf player.
        /// </summary>
        /// <param name="user">protobuf player</param>
        public Player(User user)
        {
            this.PlayerInstance = user.ToSimpleUser();
            this._numMinesBuilt = 0;
            this._neptunium = 0;
            this._isEliminated = false;
            this.CurrencyManager = new CurrencyManager();
        }

        public Player(SimpleUser playerInstance)
        {
            this.PlayerInstance = playerInstance;
            this._numMinesBuilt = 0;
            this._neptunium = 0;
            this._isEliminated = false;
            this.CurrencyManager = new CurrencyManager();
        }
        
        public Player(
            SimpleUser playerInstance,
            List<SpecialistTypeId> specialistPool
        ) {
            this.PlayerInstance = playerInstance;
            this._numMinesBuilt = 0;
            this._neptunium = 0;
            this._isEliminated = false;
            this.CurrencyManager = new CurrencyManager();
            this.SpecialistPool = new SpecialistPool(this, Game.SeededRandom, specialistPool);
        }

        /// <summary>
        /// Gets the player's id
        /// </summary>
        /// <returns>The player's database ID</returns>
        public string GetId()
        {
            return this.PlayerInstance.Id;
        }

        /// <summary>
        /// Get the player's username
        /// </summary>
        /// <returns>The player's username</returns>
        public string GetPlayerName()
        {
            return this.PlayerInstance.Username;
        }

        /// <summary>
        /// Increases or decreases the amount of neptunium the player has. Pass a negative value for neptunium to decease.
        /// </summary>
        /// <param name="neptunium">The value to change _neptunium by</param>
        public int AlterNeptunium(int neptunium)
        {
            this._neptunium += neptunium;
            return neptunium;
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

        public override bool Equals(object? obj)
        {
            Player? other = obj as Player;
            if (other == null)
                return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return PlayerInstance.GetHashCode();
        }

        private bool Equals(Player other)
        {
            return other.PlayerInstance.Id == this.PlayerInstance.Id;
        }
    }
}
