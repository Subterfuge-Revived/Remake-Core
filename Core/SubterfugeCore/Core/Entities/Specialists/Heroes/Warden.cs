using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Warden : Specialist
    {
        private List<int> ExtraDrillersPerLevel = new List<int>() { 0, 1, 2 };
        
        public Warden(Player owner) : base(owner, true)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().CanLoadCapturedSpecialists = true;
                entity.GetComponent<DrillerProducer>().Producer.OnResourceProduced += ProducePerCapturedSpecialist;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpecialistManager>().CanLoadCapturedSpecialists = false;
                entity.GetComponent<DrillerProducer>().Producer.OnResourceProduced -= ProducePerCapturedSpecialist;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<SpecialistManager>().CanLoadCapturedSpecialists = false;
            entity.GetComponent<DrillerProducer>().Producer.OnResourceProduced -= ProducePerCapturedSpecialist;
            // Kill player.
            entity.GetComponent<DrillerCarrier>().GetOwner().SetEliminated(true);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Warden;
        }

        public override string GetDescription()
        {
            return $"The Warden can transport captured specialists. " +
                   $"While at a factory, each captured specialist produces an extra {ExtraDrillersPerLevel} drillers. " +
                   $"If the Warden dies, the owner is eliminated.";
        }

        public void ProducePerCapturedSpecialist(object? sender, ProductionEventArgs productionEventArgs)
        {
            // Get total number of captured specs are current location.
            var capturedTotal = productionEventArgs
                .ProductionEvent
                .ProductionLocation
                .GetComponent<SpecialistManager>()
                .GetCapturedSpecialistCount();

            productionEventArgs.ProductionEvent.ValueProduced += capturedTotal * GetExtraDrillersPerSpecialist();
        }

        private int GetExtraDrillersPerSpecialist()
        {
            return ExtraDrillersPerLevel[_level];
        }
    }
}