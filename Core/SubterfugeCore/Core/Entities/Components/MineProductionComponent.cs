using Subterfuge.Remake.Core.Resources.Producers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
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