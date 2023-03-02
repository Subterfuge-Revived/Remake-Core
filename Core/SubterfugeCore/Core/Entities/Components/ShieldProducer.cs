using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class ShieldProducer : EntityComponent
    {
        public ResourceProducer Producer { get; set; }
        
        public ShieldProducer(
            IEntity parent,
            TimeMachine timeMachine
        ) : base(parent)
        {
            Producer = new ResourceProducer(
                parent,
                timeMachine, 
                Constants.BASE_SHIELD_REGENERATION_TICKS, 
                1,
                ProducedResourceType.Shield
            );
        }
    }
}