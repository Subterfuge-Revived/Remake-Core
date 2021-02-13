namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can hold drillers
    /// </summary>
    public interface IDrillerCarrier
    {
        /// <summary>
        /// Get the number of drillers at this location
        /// </summary>
        /// <returns>The number of drillers</returns>
        int GetDrillerCount();
        
        /// <summary>
        /// Set the number of drillers at this location
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set at the location</param>
        void SetDrillerCount(int drillerCount);
        
        /// <summary>
        /// Adds drillers to the location
        /// </summary>
        /// <param name="drillersToAdd">The number of drillers to add</param>
        void AddDrillers(int drillersToAdd);
        
        /// <summary>
        /// Removes drillers from the location
        /// </summary>
        /// <param name="drillersToRemove">The number of drillers to remove</param>
        void RemoveDrillers(int drillersToRemove);
        
        /// <summary>
        /// Checks it the location has the indicated number of drillers
        /// </summary>
        /// <param name="drillers">The number of drillers to check</param>
        /// <returns>if the location has an equal or greater number of drillers</returns>
        bool HasDrillers(int drillers);
    }
}
