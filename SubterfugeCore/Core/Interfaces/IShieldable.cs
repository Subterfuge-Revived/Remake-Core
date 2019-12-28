using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Interfaces.Outpost
{
    /// <summary>
    /// Anything that can be shielded
    /// </summary>
    public interface IShieldable
    {
        /// <summary>
        /// Determines the current shield level
        /// </summary>
        /// <returns>The current shield level</returns>
        int getShields();
        
        /// <summary>
        /// Set the shield level 
        /// </summary>
        /// <param name="shieldValue">The shield level to set</param>
        void setShields(int shieldValue);
        
        /// <summary>
        /// Removes a defined amount of shields
        /// </summary>
        /// <param name="shieldsToRemove">The number of shields to remove</param>
        void removeShields(int shieldsToRemove);
        
        /// <summary>
        /// Turn sheilds off or on
        /// </summary>
        void toggleShield();
        
        /// <summary>
        /// Determines if the shields are off or on
        /// </summary>
        /// <returns>if the shields are enabled</returns>
        bool isShieldActive();
        
        /// <summary>
        /// Adds shields
        /// </summary>
        /// <param name="shields">The amount of shields to add</param>
        void addShield(int shields);
        
        /// <summary>
        /// Get the shield capacity.
        /// </summary>
        /// <returns>the maximum amount of shields possible</returns>
        int getShieldCapacity();
        
        /// <summary>
        /// Sets the shield capacity
        /// </summary>
        /// <param name="capactiy">The capacity of shields</param>
        void setShieldCapacity(int capactiy);
    }
}
