using System;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public interface IGameEvent : IComparable
    {
        /// <summary>
        /// This function will be executed when determing the game's state for the time machine.
        /// This function will check all conditions required to perform the command as well as perform the command
        /// to show the outcome of the command.
        /// </summary>
        bool ForwardAction(TimeMachine timeMachine, GameState.GameState state);

        /// <summary>
        /// This function will be executed when going back in time in order to undo an action.
        /// For example, this will un-hire a specialist returning the hire point to the queen, or un-launch a sub returning the drillers to the owner.
        /// </summary>
        bool BackwardAction(TimeMachine timeMachine, GameState.GameState state);
        
        /// <summary>
        /// Get the tick the game event occurs at
        /// </summary>
        /// <returns>The tick the game event occurs at.</returns>
        GameTick GetOccursAt();
        
        /// <summary>
        /// Get the id of this game event.
        /// </summary>
        /// <returns>The id of this game event.</returns>
        string GetEventId();
        
        /// <summary>
        /// Get the tick the game event occurs at
        /// </summary>
        /// <returns>The tick the game event occurs at.</returns>
        Priority GetPriority();
        
        /// <summary>
        /// A function to determine if the event was valid and if the event got triggered in the game.
        /// This should be used to determine if the reverse action is applied.
        /// </summary>
        /// <returns>If the event was successfully applied in the forward direction</returns>
        bool WasEventSuccessful();
    }
}