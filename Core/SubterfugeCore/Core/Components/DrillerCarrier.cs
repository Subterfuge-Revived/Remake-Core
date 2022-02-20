using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Components
{
    public class DrillerCarrier : EntityComponent
    {
        private Player owner;
        private int drillers = 0;
        private bool isCaptured = false;
        private bool isDestroyed = false;

        /// <summary>
        /// Creates a new driller carrier
        /// </summary>
        /// <param name="drillerCount">The number of drillers in the carrier</param>
        /// <param name="owner">The owner of the carrier</param>
        /// <param name="parent"></param>
        public DrillerCarrier(IEntity parent, int drillerCount, Player owner) : base(parent)
        {
            this.drillers = drillerCount;
            this.owner = owner;
        }

        /// <summary>
        /// Gets the number of drillers on the carrier
        /// </summary>
        /// <returns></returns>
        public int GetDrillerCount()
        {
            return drillers;
        }

        /// <summary>
        /// Sets the number of drillers on the carrier
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set</param>
        public void SetDrillerCount(int drillerCount)
        {
            this.drillers = drillerCount;
        }

        /// <summary>
        /// Adds drillers to the carrier
        /// </summary>
        /// <param name="drillersToAdd"></param>
        public void AddDrillers(int drillersToAdd)
        {
            this.drillers += drillersToAdd;
        }

        /// <summary>
        /// Removes drillers from the carrier
        /// </summary>
        /// <param name="drillersToRemove">Drillers to remove</param>

        public void RemoveDrillers(int drillersToRemove)
        {
            if (this.drillers >= drillersToRemove)
            {
                this.drillers -= drillersToRemove;
            }
            else
            {
                this.drillers = 0;
                this.isCaptured = true;
            }
        }

        /// <summary>
        /// Check if the carrier contains the specified number of drillers
        /// </summary>
        /// <param name="drillers">The number of drillers to check for</param>
        /// <returns>If the carrier contains the number of drillers</returns>
        public bool HasDrillers(int drillers)
        {
            return this.drillers >= drillers;
        }

        /// <summary>
        /// Sets the driller carrier to have a new owner.
        /// </summary>
        /// <param name="newOwner">The new owner of the carrier</param>
        /// <param name="newDrillerCount">The new driller count of the carrier</param>
        public void SetNewOwner(Player newOwner, int newDrillerCount)
        {
            drillers = newDrillerCount;
            owner = newOwner;
            isCaptured = false;
        }

        public Player GetOwner()
        {
            return this.owner;
        }

        public void SetOwner(Player player)
        {
            this.owner = player;
        }

        /// <summary>
        /// Checks if the carrier is dead
        /// This should only be true if the carrier is not existing in the game.
        /// </summary>
        /// <returns>If the carrier is alive</returns>
        public bool IsCaptured()
        {
            return isCaptured;
        }

        public void SetCaptured(bool isCaptured)
        {
            this.isCaptured = isCaptured;
        }

        public bool IsDestroyed()
        {
            return this.isDestroyed;
        }

        public void Destroy()
        {
            this.isDestroyed = true;
            this.drillers = 0;
        }
        
    }
}