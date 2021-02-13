using System;

namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can be owned by a player
    /// </summary>
    public interface IOwnable
    {
        string GetId();
    
        /// <summary>
        /// Get the owner
        /// </summary>
        /// <returns>The owner</returns>
        Players.Player GetOwner();
        
        /// <summary>
        /// Set the owner
        /// </summary>
        /// <param name="newOwner">The new owner</param>
        void SetOwner(Players.Player newOwner);
    }
}
