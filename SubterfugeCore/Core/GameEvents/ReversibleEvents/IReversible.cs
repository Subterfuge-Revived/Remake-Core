namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Any reversible action
    /// </summary>
    public interface IReversible
    {
        /// <summary>
        /// Applies the forward action
        /// </summary>
        /// <returns>If the action was successful</returns>
        bool forwardAction();
        
        /// <summary>
        /// Applies the backward action
        /// </summary>
        /// <returns>If the backward action was successful</returns>
        bool backwardAction();
    }
}