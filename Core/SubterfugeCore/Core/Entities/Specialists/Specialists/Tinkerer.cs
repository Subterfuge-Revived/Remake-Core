using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Tinkerer : Specialist
    {
        public Tinkerer(Player owner, bool isHero) : base(owner, isHero)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<DrillerProductionComponent>().DrillerProducer.IgnoresDrillerCapacity = true;
                entity.GetComponent<DrillerProductionComponent>().DrillerProducer.ChangeTicksPerProductionCycle(-1 * GetDecreasedProductionTicks());
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<DrillerProductionComponent>().DrillerProducer.IgnoresDrillerCapacity = false;
                entity.GetComponent<DrillerProductionComponent>().DrillerProducer.ChangeTicksPerProductionCycle(GetDecreasedProductionTicks());
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<DrillerProductionComponent>().DrillerProducer.IgnoresDrillerCapacity = false;
            captureLocation.GetComponent<DrillerProductionComponent>().DrillerProducer.ChangeTicksPerProductionCycle(GetDecreasedProductionTicks());
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Tinkerer;
        }

        public int GetDecreasedProductionTicks()
        {
            return _level switch
            {
                0 => 0,
                1 => Constants.TicksPerProduction / 10, // Increase by 10%
                2 => Constants.TicksPerProduction / 5, // Increase by 20%
                _ => 0,
            };
        }
    }
}