using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Foreman : Specialist
    {

        public List<int> ExtraDrillersPerLevel = new List<int>() { 4, 6, 6 };

        public List<int> TickReductionPerLevel = new List<int>()
        {
            0,
            0,
            Constants.TicksPerProduction / 4
        };

        public Foreman(Player owner) : base(owner, false)
        {
        }

        private void ProduceMore(object? sender, ProductionEventArgs productionEventArgs)
        {
            productionEventArgs.ProductionEvent.ValueProduced += ExtraDrillersPerLevel[_level];
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                if (entity is Factory)
                {
                    entity.GetComponent<DrillerProducer>().Producer.OnResourceProduced += ProduceMore;

                    if (_level == 3)
                    {
                        entity.GetComponent<DrillerProducer>().Producer.ChangeTicksPerProductionCycle(-1 * TickReductionPerLevel[_level]);
                    }
                }
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                if (entity is Factory)
                {
                    entity.GetComponent<DrillerProducer>().Producer.OnResourceProduced -= ProduceMore;
                    if (_level == 3)
                    {
                        entity.GetComponent<DrillerProducer>().Producer.ChangeTicksPerProductionCycle(TickReductionPerLevel[_level]);
                    }
                }
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<DrillerProducer>().Producer.OnResourceProduced -= ProduceMore;
            if (_level == 3)
            {
                entity.GetComponent<DrillerProducer>().Producer.ChangeTicksPerProductionCycle(TickReductionPerLevel[_level]);
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Foreman;
        }

        public override string GetDescription()
        {
            return $"Produces an additional {ExtraDrillersPerLevel} drillers while at a factory." +
                   $"Decreases production by {TickReductionPerLevel} ticks while at a factory.";
        }
    }
}