using System;
using Microsoft.DotNet.PlatformAbstractions;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat
{
    public abstract class PositionalGameEvent : GameEvent
    {
        private readonly IEntity _expectedRegenerationLocation;
        
        protected PositionalGameEvent(GameTick occursAt, Priority priority, IEntity expectedRegenerationLocation) : base(occursAt, priority)
        {
            _expectedRegenerationLocation = expectedRegenerationLocation;
        }

        public override string GetEventId()
        {
            return GetHashCode().ToString();
        }
        
        public override bool Equals(object other)
        {
            PositionalGameEvent asEvent = other as PositionalGameEvent;
            if (asEvent == null)
                return false;

            return asEvent.OccursAt == this.OccursAt &&
                   asEvent.Priority == this.Priority &&
                   asEvent._expectedRegenerationLocation == this._expectedRegenerationLocation;
        }

        public override int GetHashCode()
        {
            var hashBuilder = new HashCodeCombiner();
            hashBuilder.Add(_expectedRegenerationLocation);
            hashBuilder.Add(OccursAt);
            hashBuilder.Add(Priority);
            return hashBuilder.GetHashCode();
        }
        
    }
}