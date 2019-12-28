using SubterfugeCore.Components;
using SubterfugeCore.Core.Interfaces.Outpost;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Components.Outpost
{
    /// <summary>
    /// Anything that can hold drillers
    /// </summary>
    public interface IDrillerCarrier : ILocation, IOwnable, ILaunchable
    {
        /// <summary>
        /// Get the number of drillers at this location
        /// </summary>
        /// <returns>The number of drillers</returns>
        int getDrillerCount();
        
        /// <summary>
        /// Set the number of drillers at this location
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set at the location</param>
        void setDrillerCount(int drillerCount);
        
        /// <summary>
        /// Adds drillers to the location
        /// </summary>
        /// <param name="drillersToAdd">The number of drillers to add</param>
        void addDrillers(int drillersToAdd);
        
        /// <summary>
        /// Removes drillers from the location
        /// </summary>
        /// <param name="drillersToRemove">The number of drillers to remove</param>
        void removeDrillers(int drillersToRemove);
        
        /// <summary>
        /// Checks it the location has the indicated number of drillers
        /// </summary>
        /// <param name="drillers">The number of drillers to check</param>
        /// <returns>if the location has an equal or greater number of drillers</returns>
        bool hasDrillers(int drillers);
    }
}
