using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class NeptuniumProducer : EntityComponent
    {
        public ResourceProducer Producer { get; set; }
        
        public NeptuniumProducer(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(parent)
        {
            Producer = new ResourceProducer(
                parent,
                timeMachine, 
                (int)(1440 / GameTick.MinutesPerTick), 
                1,
                ProducedResourceType.Neptunium
            );
        }
    }
}