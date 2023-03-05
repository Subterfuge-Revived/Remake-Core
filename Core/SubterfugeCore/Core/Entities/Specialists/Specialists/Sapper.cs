using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Sapper : Specialist
    {
        public List<float> haltDrillerPercent = new List<float>() { 0, 0.50f, 1.00f };
        
        public Sapper(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (_isCaptured)
            {
                ToggleEntityShieldProduction(true, entity);
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            
            if (_isCaptured)
            {
                ToggleEntityShieldProduction(false, entity);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            if (isCaptured)
            {
                ToggleEntityShieldProduction(true, entity);
            }
            else
            {
                ToggleEntityShieldProduction(false, entity);
            }
        }

        public void ToggleEntityShieldProduction(bool shouldPause, IEntity entity)
        {
            if (!Equals(entity.GetComponent<DrillerCarrier>().GetOwner(), _owner))
            {
                entity.GetComponent<ShieldProducer>().Producer.SetPaused(shouldPause);
                
                // Also alter the production events.
                entity.GetComponent<DrillerProducer>().Producer.OnResourceProduced += ReduceProductionAmount;
            }
        }

        public void ReduceProductionAmount(object? sender, ProductionEventArgs productionEventArgs)
        {
            productionEventArgs.ProductionEvent.ValueProduced -= (int)(productionEventArgs.ProductionEvent.ValueProduced * haltDrillerPercent[_level]);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Sapper;
        }

        public override string GetDescription()
        {
            return $"While captured at an enemy base, prevents shield regeneration. " +
                   $"Also halts driller production by {haltDrillerPercent} percent.";
        }
    }
}