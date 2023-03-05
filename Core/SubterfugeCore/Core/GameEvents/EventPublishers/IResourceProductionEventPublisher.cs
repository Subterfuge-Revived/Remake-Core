using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface IResourceProductionEventPublisher
    {
        event EventHandler<ProductionEventArgs> OnResourceProduced;
    }

    public class ProductionEventArgs: DirectionalEventArgs
    {
        public GameTick NextProduction { get; set; }
        public GameTick TickProducedAt { get; set; }
        public ResourceProductionEvent ProductionEvent { get; set; }
        public ProducedResourceType ProducedResourceType { get; set; }
    }
}