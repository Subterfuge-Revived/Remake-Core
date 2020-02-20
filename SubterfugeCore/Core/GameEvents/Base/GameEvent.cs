using System;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Base
{
    /// <summary>
    /// An instave of a GameEvent that can be added to the TimeMachine.
    /// </summary>
    public abstract class GameEvent : IComparable
    {
        /// <summary>
        /// The time when the event occurs
        /// </summary>
        private GameTick gameTick;
        
        /// <summary>
        /// If the event was successfully triggered
        /// </summary>
        protected bool eventSuccess;
        
        /// <summary>
        /// The name of the event
        /// </summary>
        public string eventName;

        /// <summary>
        /// This function will be executed when determing the game's state for the time machine.
        /// This function will check all conditions required to perform the command as well as perform the command
        /// to show the outcome of the command.
        /// </summary>
        public abstract void eventForwardAction();

        /// <summary>
        /// This function will be executed when going back in time in order to undo an action.
        /// For example, this will un-hire a specialist returning the hire point to the queen, or un-launch a sub returning the drillers to the owner.
        /// </summary>
        public abstract void eventBackwardAction();

        /// <summary>
        /// Gets the event's name
        /// </summary>
        /// <returns>The event name</returns>
        public string getEventName()
        {
            return this.eventName;
        }

        /// <summary>
        /// Get the tick the event happens on
        /// </summary>
        /// <returns>The tick of the event</returns>
        public abstract GameTick getTick();

        /// <summary>
        /// Comparison override for the priority queue sorting
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>if the object is before, after another event</returns>
        public int CompareTo(object obj)
        {
            GameEvent comparedEvent = obj as GameEvent;
            if (comparedEvent == null) return 1;
            if (this.getTick() > comparedEvent.getTick())
            {
                return 1;
            } else if (this.getTick() == comparedEvent.getTick())
            {
                // Do further comparison. The game events should NEVER be the same!
                if(this.getTick().getDate() > comparedEvent.getTick().getDate())
                {
                    return 1;
                } else
                {
                    return -1;
                }
            } else
            {
                return -1;
            }
        }
        
    }
}
