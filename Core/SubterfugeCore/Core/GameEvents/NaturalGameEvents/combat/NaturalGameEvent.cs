using System;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents
{
    public abstract class NaturalGameEvent : GameEvent
    {
        private string EventId;
        private GameTick _occursAt;
        private Priority _priority;
        
        protected NaturalGameEvent(GameTick occursAt, Priority priority) : base()
        {
            this.EventId = Guid.NewGuid().ToString();
            this._occursAt = occursAt;
        }
        
        public override GameTick GetOccursAt()
        {
            return _occursAt;
        }

        public override string GetEventId()
        {
            return EventId;
        }
    }
}