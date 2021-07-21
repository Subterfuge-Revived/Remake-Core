using System;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using SubterfugeCore.Core.GameEvents.Reversible;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.Base
{
    /// <summary>
    /// An instave of a GameEvent that can be added to the TimeMachine.
    /// </summary>
    public abstract class GameEvent : IComparable
    {
        /// <summary>
        /// If the event was successfully triggered
        /// </summary>
        protected bool EventSuccess;

        public readonly GameTick GameTick;
        public readonly GameStateEffect GameStateEffect;

        protected GameEvent(GameTick tick, GameStateEffect gameStateEffect)
        {
            this.GameTick = tick;
            this.GameStateEffect = gameStateEffect;
        }
        
        /// <summary>
        /// Get the id of this game event.
        /// </summary>
        /// <returns>The id of this game event.</returns>
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
            if (this.GameTick > comparedEvent.GameTick)
            {
                return 1;
            }
            if (this.GameTick < comparedEvent.GameTick)
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
    }
}
