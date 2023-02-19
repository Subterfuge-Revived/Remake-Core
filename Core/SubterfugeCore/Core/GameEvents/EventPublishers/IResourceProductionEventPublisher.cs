using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface IResourceProductionEventPublisher
    {
        event EventHandler<ProductionEventArgs> OnResourceProduced;
    }

    public class ProductionEventArgs
    {
        public int ValueProduced { get; set; }
        public GameTick NextProduction { get; set; }
        public GameTick TickProducedAt { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }
}