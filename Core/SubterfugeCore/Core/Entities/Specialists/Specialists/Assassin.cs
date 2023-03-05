using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Assassin : Specialist
    {
        public Assassin(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects += KillSpecialistRegisterCombatEffects;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= KillSpecialistRegisterCombatEffects;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= KillSpecialistRegisterCombatEffects;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Assasin;
        }

        public override string GetDescription()
        {
            return "Kills all Specialists in combat.";
        }

        public void KillSpecialistRegisterCombatEffects(object? sender, OnRegisterCombatEventArgs combatArgs)
        {
            var friendlyEntity = combatArgs.CombatEvent.GetEntityOwnedBy(_owner);
            var enemyEntity = combatArgs.CombatEvent.GetEnemyEntity(_owner);
            
            combatArgs.CombatEvent.AddEffectToCombat(new KillSpecialistsEffect(
                combatArgs.CombatEvent.OccursAt,
                enemyEntity
            ));
        }
    }
}