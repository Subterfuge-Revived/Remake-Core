using Subterfuge.Remake.Core.Resources.Producers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
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