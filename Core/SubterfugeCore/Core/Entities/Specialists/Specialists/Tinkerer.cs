using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Tinkerer : Specialist
    {
        private List<int> DecreasedProductionTicks = new List<int>()
        {
            0,
            Constants.TicksPerProduction / 10, // 10%
            Constants.TicksPerProduction / 5 // 20%
        }; 
        
        public Tinkerer(Player owner, bool isHero) : base(owner, isHero)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            
            if (!_isCaptured)
            {
                entity.GetComponent<DrillerProducer>().Producer.IgnoresCapacity = true;
                entity.GetComponent<DrillerProducer>().Producer.ChangeTicksPerProductionCycle(-1 * DecreasedProductionTicks[_level]);
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<DrillerProducer>().Producer.IgnoresCapacity = false;
                entity.GetComponent<DrillerProducer>().Producer.ChangeTicksPerProductionCycle(DecreasedProductionTicks[_level]);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            if (isCaptured)
            {
                entity.GetComponent<DrillerProducer>().Producer.IgnoresCapacity = false;
                entity.GetComponent<DrillerProducer>().Producer.ChangeTicksPerProductionCycle(DecreasedProductionTicks[_level]);
            }
            else
            {
                entity.GetComponent<DrillerProducer>().Producer.IgnoresCapacity = true;
                entity.GetComponent<DrillerProducer>().Producer.ChangeTicksPerProductionCycle(DecreasedProductionTicks[_level]);
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Tinkerer;
        }

        public override string GetDescription()
        {
            return $"The factory where the Tinkerer is stationed is allowed th exceed the driller cap. " +
                   $"Increases driller production rate by {DecreasedProductionTicks} ticks.";
        }
    }
}