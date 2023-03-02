using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class DrillerProducer : EntityComponent
    {
        public ResourceProducer Producer { get; set; }
        
        public DrillerProducer(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(parent)
        {
            Producer = new ResourceProducer(parent,
                timeMachine,
                Constants.TicksPerProduction,
                Constants.BaseFactoryProductionAmount,
                ProducedResourceType.Driller
            );
        }
    }
}