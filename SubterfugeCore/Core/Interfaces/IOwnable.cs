namespace SubterfugeCore.Core.Interfaces
{
    /// <summary>
    /// Anything that can be owned by a player
    /// </summary>
    public interface IOwnable
    {
        /// <summary>
        /// Get the owner
        /// </summary>
        /// <returns>The owner</returns>
        Players.Player getOwner();
        
        /// <summary>
        /// Set the owner
        /// </summary>
        /// <param name="newOwner">The new owner</param>
        void setOwner(Players.Player newOwner);
    }
}
