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
        
        protected NaturalGameEvent(GameTick occursAt, Priority priority) : base(occursAt, )
        {
            this.EventId = Guid.NewGuid().ToString();
            this._occursAt = occursAt;
            this._priority = priority;
        }

        public override string GetEventId()
        {
            return this.EventId;
        }

		public override Priority GetPriority()
		{
            return this._priority;
		}
	}
}