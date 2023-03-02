/*using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Foreman : Specialist
    {
        private DrillerProductionComponent _currentDrillerProducer;
        
        public Foreman(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                if (entity is Factory)
                {
                    _currentDrillerProducer = entity.GetComponent<DrillerProductionComponent>();
                    _currentDrillerProducer.DrillerProducer.OnResourceProduced += ProduceMore;

                    if (_level == 3)
                    {
                        _currentDrillerProducer.DrillerProducer.ChangeTicksPerProductionCycle(-1 * Constants.TicksPerProduction / 4);
                    }
                }
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                if (entity is Factory)
                {
                    entity.GetComponent<DrillerProductionComponent>().DrillerProducer.OnResourceProduced -= ProduceMore;
                    if (_level == 3)
                    {
                        entity.GetComponent<DrillerProductionComponent>().DrillerProducer.ChangeTicksPerProductionCycle(Constants.TicksPerProduction / 4);
                    }
                }
            }
        }

        public override void OnCapturedEvent(IEntity captureLocation)
        {
            _currentDrillerProducer.DrillerProducer.OnResourceProduced -= ProduceMore;
            if (_level == 3)
            {
                _currentDrillerProducer.DrillerProducer.ChangeTicksPerProductionCycle(Constants.TicksPerProduction / 4);
            }
        }

        private void ProduceMore(object? sender, ProductionEventArgs productionEventArgs)
        {
            _currentDrillerProducer.Parent.GetComponent<DrillerCarrier>().AlterDrillers(GetDrillerDelta());
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Foreman;
        }

        public int GetDrillerDelta()
        {
            return _level switch
            {
                1 => 4,
                2 => 6,
                _ => 4
            };
        }
    }
}*/