using System;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Base
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
        
        /// <summary>
        /// Sets the tick when this event occurs.
        /// </summary>
        public GameTick OccursAt { get; }
        
        public Priority Priority { get; }

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
        /// Get the id of this game event.
        /// </summary>
        /// <returns>The id of this game event.</returns>
        public abstract string GetEventId();

        public GameEvent(
            GameTick occursAt,
            Priority priority
        )
        {
            this.OccursAt = occursAt;
            this.Priority = priority;
        }

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
            
            if (this.OccursAt > comparedEvent.OccursAt)
            {
                return 1;
            }
            if (this.OccursAt < comparedEvent.OccursAt)
            {
                return -1;
            }
            
            if (Priority > comparedEvent.Priority)
            {
                return -1;
            }
            
            if (Priority < comparedEvent.Priority)
            {
                return 1;
            }
            
            // Do further comparison. The game events should NEVER be the same!
            return String.Compare(GetEventId(), comparedEvent.GetEventId(), StringComparison.Ordinal);
        }

        public abstract override bool Equals(Object other);
        public abstract override int  GetHashCode();
        
        public static bool operator ==(GameEvent? left, GameEvent? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(GameEvent? left, GameEvent? right)
        {
            return !Equals(left, right);
        }

        public abstract bool WasEventSuccessful();
    }
}
