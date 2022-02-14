using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Managers
{
    public class SubLauncher : IDrillerCarrier
    {
        
        /// <summary>
        /// The number of drillers at the outpost
        /// </summary>
        int _drillerCount;

        public SubLauncher()
        {
            _drillerCount = 0;
        }

        public SubLauncher(int drillerCount)
        {
            _drillerCount = drillerCount;
        }
        
        /// <summary>
        /// Adds drillers to the outpost
        /// </summary>
        /// <param name="drillers">The number of drillers to add to the outpost</param>
        public void AddDrillers(int drillers)
        {
            this._drillerCount += drillers;
        }
        
        /// <summary>
        /// Remove drillers from the outpost
        /// </summary>
        /// <param name="drillers">The number of drillers to remove</param>
        public void RemoveDrillers(int drillers)
        {
            this._drillerCount -= drillers;
        }
        
        /// <summary>
        /// Get the number of drillers at the position
        /// </summary>
        /// <returns>The number of drillers at the outpost</returns>
        public int GetDrillerCount()
        {
            return this._drillerCount;
        }

        /// <summary>
        /// Set the number of drillers
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set</param>
        public void SetDrillerCount(int drillerCount)
        {
            this._drillerCount = drillerCount;
        }

        /// <summary>
        /// Checks if the outpost has the drillers specified
        /// </summary>
        /// <param name="drillers">The number of drillers to check for</param>
        /// <returns>if the outpost has the drillers</returns>
        public bool HasDrillers(int drillers)
        {
            return this._drillerCount >= drillers;
        }
    }
}