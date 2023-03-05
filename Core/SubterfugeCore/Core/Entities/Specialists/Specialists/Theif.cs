using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Theif : Specialist
    {

        private List<float> _stealPerLevel = new List<float>() { 0.1f, 0.15f, 0.20f };
        
        public Theif(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects += StealEnemyDrillers;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= StealEnemyDrillers;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= StealEnemyDrillers;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Theif;
        }

        private void StealEnemyDrillers(object? combat, OnRegisterCombatEventArgs combatEventArgs)
        {
            var friendlyEntity = combatEventArgs.CombatEvent.GetEntityOwnedBy(_owner);
            var enemyCarrier = combatEventArgs.CombatEvent.GetEnemyEntity(_owner).GetComponent<DrillerCarrier>();
            
            combatEventArgs.CombatEvent.AddEffectToCombat(new AlterDrillerEffect(
                combatEventArgs.CombatEvent,
                friendlyEntity,
                (int)(enemyCarrier.GetDrillerCount() * _stealPerLevel[_level]),
                (int)(enemyCarrier.GetDrillerCount() * _stealPerLevel[_level] * -1)
            ));
        }

        public override string GetDescription()
        {
            return $"Steals {_stealPerLevel} percent of enemy drillers on combat.";
        }
    }
}