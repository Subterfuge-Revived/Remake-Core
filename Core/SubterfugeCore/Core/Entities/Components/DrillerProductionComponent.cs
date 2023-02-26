using Subterfuge.Remake.Core.Resources.Producers;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
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