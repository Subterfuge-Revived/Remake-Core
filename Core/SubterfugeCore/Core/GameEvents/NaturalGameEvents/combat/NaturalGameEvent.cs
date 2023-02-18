using System;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat
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