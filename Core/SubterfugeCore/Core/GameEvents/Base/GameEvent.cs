using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.Base
{
    /// <summary>
    /// An instave of a GameEvent that can be added to the TimeMachine.
    /// </summary>
    public abstract class GameEvent : IComparable, IReversible
    {
        /// <summary>
        /// If the event was successfully triggered
        /// </summary>
        protected bool EventSuccess;

        protected GameEvent()
        {
        }

        /// <summary>
        /// This function will be executed when determing the game's state for the time machine.
        /// This function will check all conditions required to perform the command as well as perform the command
        /// to show the outcome of the command.
        /// </summary>
        public abstract bool ForwardAction(TimeMachine timeMachine, GameState state);

        /// <summary>
        /// This function will be executed when going back in time in order to undo an action.
        /// For example, this will un-hire a specialist returning the hire point to the queen, or un-launch a sub returning the drillers to the owner.
        /// </summary>
        public abstract bool BackwardAction(TimeMachine timeMachine, GameState state);

        /// <summary>
        /// Get the tick the game event occurs at
        /// </summary>
        /// <returns>The tick the game event occurs at.</returns>
        public abstract GameTick GetOccursAt();
        
        /// <summary>
        /// Get the tick the game event occurs at
        /// </summary>
        /// <returns>The tick the game event occurs at.</returns>
        public abstract string GetEventId();
        
        /// <summary>
        /// Get the tick the game event occurs at
        /// </summary>
        /// <returns>The tick the game event occurs at.</returns>
        public abstract Priority GetPriority();

        /// <summary>
        /// Comparison override for the priority queue sorting
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>if the object is before, after another event</returns>
        public int CompareTo(object obj)
        {
            // 1 = this event occurs last.
            // -1 = this event occurs first.
            GameEvent comparedEvent = obj as GameEvent;
            if (comparedEvent == null) return 1;
            if (this.GetOccursAt() > comparedEvent.GetOccursAt())
            {
                return 1;
            }
            if (this.GetOccursAt() < comparedEvent.GetOccursAt())
            {
                return -1;
            }
            
            if (GetPriority() > comparedEvent.GetPriority())
            {
                return -1;
            }
            
            if (GetPriority() < comparedEvent.GetPriority())
            {
                return 1;
            }
            
            // Do further comparison. The game events should NEVER be the same!
            return String.Compare(GetEventId(), comparedEvent.GetEventId(), StringComparison.Ordinal);
        }

        public abstract bool WasEventSuccessful();
    }
}
