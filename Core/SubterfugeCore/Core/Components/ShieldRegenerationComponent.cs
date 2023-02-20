using System;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Components
{
    public class ShieldRegenerationComponent : EntityComponent
    {
        public ShieldRegenerator ShieldRegenerator { get; }
		    
        public ShieldRegenerationComponent(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(parent)
        {
            ShieldRegenerator = new ShieldRegenerator(parent, timeMachine);
        }
    }
}