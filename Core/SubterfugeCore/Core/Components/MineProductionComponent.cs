using System;
using System.Collections.Generic;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class MineProductionComponent : EntityComponent
    {
	    public MineProducer MineProducer;
		    
	    public MineProductionComponent(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(parent)
	    {
		    MineProducer = new MineProducer(parent, timeMachine);
	    }
    }
}