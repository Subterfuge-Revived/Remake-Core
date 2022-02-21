using System;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.combat
{
    public abstract class NaturalGameEvent : GameEvent
    {
        private readonly string _eventId;
        private readonly GameTick _occursAt;
        private readonly Priority _priority;
        
        protected NaturalGameEvent(GameTick occursAt, Priority priority)
        {
            this._eventId = Guid.NewGuid().ToString();
            this._occursAt = occursAt;
            this._priority = priority;
        }
        
        public override GameTick GetOccursAt()
        {
            return this._occursAt;
        }

        public override string GetEventId()
        {
            return this._eventId;
        }

		public override Priority GetPriority()
		{
            return this._priority;
		}
	}
}