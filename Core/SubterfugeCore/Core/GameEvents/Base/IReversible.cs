using System;
using SubterfugeCore.Core.Timing;

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
        bool ForwardAction(TimeMachine timeMachine, GameState state);
        
        /// <summary>
        /// Applies the backward action
        /// </summary>
        /// <returns>If the backward action was successful</returns>
        bool BackwardAction(TimeMachine timeMachine, GameState state);

        /// <summary>
        /// Function to verify if the event was successful during launch.
        /// </summary>
        /// <returns>If the event occured</returns>
        bool WasEventSuccessful();
    }
}