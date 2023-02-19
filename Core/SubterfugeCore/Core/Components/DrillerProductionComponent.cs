using System;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class DrillerProductionComponent : EntityComponent
    {
        public DrillerProducer DrillerProducer { get; }
		    
        public DrillerProductionComponent(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(parent)
        {
            DrillerProducer = new DrillerProducer(parent, timeMachine);
        }
    }
}