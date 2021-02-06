using System;
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
        
        /// <summary>
        /// The event id
        /// </summary>
        public string EventId;
        
        /// <summary>
        /// The name of the event
        /// </summary>
        public string EventName;

        /// <summary>
        /// The player who issued the command
        /// </summary>
        public Player IssuedBy;
        
        /// <summary>
        /// The player who issued the command
        /// </summary>
        public long UnixTimeIssued;

        /// <summary>
        /// This function will be executed when determing the game's state for the time machine.
        /// This function will check all conditions required to perform the command as well as perform the command
        /// to show the outcome of the command.
        /// </summary>
        public abstract bool ForwardAction(GameState state);

        /// <summary>
        /// This function will be executed when going back in time in order to undo an action.
        /// For example, this will un-hire a specialist returning the hire point to the queen, or un-launch a sub returning the drillers to the owner.
        /// </summary>
        public abstract bool BackwardAction(GameState state);

        /// <summary>
        /// Gets the event's name
        /// </summary>
        /// <returns>The event name</returns>
        public string GetEventName()
        {
            return this.EventName;
        }

        /// <summary>
        /// Get the tick the event happens on
        /// </summary>
        /// <returns>The tick of the event</returns>
        public abstract GameTick GetTick();

        /// <summary>
        /// Comparison override for the priority queue sorting
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>if the object is before, after another event</returns>
        public int CompareTo(object obj)
        {
            GameEvent comparedEvent = obj as GameEvent;
            if (comparedEvent == null) return 1;
            if (this.GetTick() > comparedEvent.GetTick())
            {
                return 1;
            } else if (this.GetTick() == comparedEvent.GetTick())
            {
                // Do further comparison. The game events should NEVER be the same!
                if(String.Compare(EventId, comparedEvent.EventId, StringComparison.Ordinal) > 0)
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

        public abstract bool WasEventSuccessful();

        public abstract GameEventModel ToEventModel();
    }
}
