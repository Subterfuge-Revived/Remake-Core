using System;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class DrillerProductionComponent : EntityComponent
    {
        private DrillerProducer MineProducer;
		    
        public DrillerProductionComponent(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(parent)
        {
            MineProducer = new DrillerProducer(parent, timeMachine);
        }
    }
}