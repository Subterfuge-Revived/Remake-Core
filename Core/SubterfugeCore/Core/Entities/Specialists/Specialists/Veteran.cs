using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Veteran : Specialist
    {
        private List<int> drillersPerLevel = new List<int>() { 10, 20, 20 };
        private List<float> ExtraDrillersPerLevel = new List<float>() { 0, 0, 0.2f };
        
        public Veteran(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects += AlterDrilleresOnCombat;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= AlterDrilleresOnCombat;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= AlterDrilleresOnCombat;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Veteran;
        }

        public override string GetDescription()
        {
            return $"Destroy an additional {drillersPerLevel} drillers in combat. " +
                   $"Increase friendly drillers in combat by {ExtraDrillersPerLevel} percent.";
        }

        private void AlterDrilleresOnCombat(object sender, OnRegisterCombatEventArgs eventArgs)
        {
            var friendlyEntity = eventArgs.CombatEvent.GetEntityOwnedBy(_owner);
            var friendlyDrillers = friendlyEntity.GetComponent<DrillerCarrier>().GetDrillerCount();
            var extraDrillers = (int)(ExtraDrillersPerLevel[_level] * friendlyDrillers);
            
            eventArgs.CombatEvent.AddEffectToCombat(new AlterDrillerEffect(
                eventArgs.CombatEvent,
                friendlyEntity,
                0,
                -1 * (drillersPerLevel[_level] + extraDrillers)
            ));
        }
    }
}