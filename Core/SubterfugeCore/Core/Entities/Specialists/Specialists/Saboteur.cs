using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Saboteur : Specialist
    {
        public List<float> slowPercent = new List<float>() { 0, 0.1f, 0.2f };
        
        public Saboteur(Player owner) : base(owner, false)
        {
        }
        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects += RedirectOnCombatEffects;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= RedirectOnCombatEffects;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= RedirectOnCombatEffects;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Saboteur;
        }

        public override string GetDescription()
        {
            return $"Redirects enemy subs on combat. Slows redirected subs by {slowPercent} units.";
        }

        public void RedirectOnCombatEffects(object? sender, OnRegisterCombatEventArgs combatArgs)
        {
            var enemyEntity = combatArgs.CombatEvent.GetEnemyEntity(_owner);
            
            combatArgs.CombatEvent.AddEffectToCombat(new RedirectEntityEffect(
                combatArgs.CombatEvent.OccursAt,
                enemyEntity
            ));
            
            combatArgs.CombatEvent.AddEffectToCombat(new SlowTargetEffect(
                combatArgs.CombatEvent.OccursAt,
                enemyEntity,
                slowPercent[_level]
            ));
        }
    }
}