using System;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class DrillerCarrier : EntityComponent
    {
        private Player _owner;
        private int _drillers;
        private bool _isCaptured;
        private bool _isDestroyed;

        /// <summary>
        /// Creates a new driller carrier
        /// </summary>
        /// <param name="drillerCount">The number of drillers in the carrier</param>
        /// <param name="owner">The owner of the carrier</param>
        /// <param name="parent"></param>
        public DrillerCarrier(IEntity parent, int drillerCount, Player owner) : base(parent)
        {
            this._drillers = drillerCount;
            this._owner = owner;
        }

        /// <summary>
        /// Gets the number of drillers on the carrier
        /// </summary>
        /// <returns></returns>
        public int GetDrillerCount()
        {
            return _drillers;
        }

        /// <summary>
        /// Sets the number of drillers on the carrier
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set</param>
        public void SetDrillerCount(int drillerCount)
        {
            this._drillers = drillerCount;
        }

        public int AlterDrillers(int delta)
        {
            var initialAmount = _drillers;
            _drillers = Math.Max(0, _drillers + delta);
            return initialAmount - _drillers;
        }

        /// <summary>
        /// Check if the carrier contains the specified number of drillers
        /// </summary>
        /// <param name="drillers">The number of drillers to check for</param>
        /// <returns>If the carrier contains the number of drillers</returns>
        public bool HasDrillers(int drillers)
        {
            return this._drillers >= drillers;
        }

        /// <summary>
        /// Sets the driller carrier to have a new owner.
        /// </summary>
        /// <param name="newOwner">The new owner of the carrier</param>
        /// <param name="newDrillerCount">The new driller count of the carrier</param>
        public void SetNewOwner(Player newOwner, int newDrillerCount)
        {
            _drillers = newDrillerCount;
            _owner = newOwner;
            _isCaptured = false;
        }

        public Player? GetOwner()
        {
            return this._owner;
        }

        public void SetOwner(Player player)
        {
            this._owner = player;
        }

        /// <summary>
        /// Checks if the carrier is dead
        /// This should only be true if the carrier is not existing in the game.
        /// </summary>
        /// <returns>If the carrier is alive</returns>
        public bool IsCaptured()
        {
            return _isCaptured;
        }

        public void SetCaptured(bool isCaptured)
        {
            this._isCaptured = isCaptured;
        }

        public bool IsDestroyed()
        {
            return _isDestroyed;
        }

        /// <summary>
        /// Destroys the outpost.
        /// </summary>
        /// <returns>The number of drillers lost.</returns>
        public int Destroy()
        {
            _isDestroyed = true;
            var drillersDestroyed = _drillers;
            _drillers = 0;
            return drillersDestroyed;
        }

        public void UndoDestroy(int drillersToRestore)
        {
            _isDestroyed = false;
            _drillers = drillersToRestore;
        }
    }
}