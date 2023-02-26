using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Assasin : Specialist
    {
        public Assasin(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat += KillSpecialistPreCombat;
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat -= KillSpecialistPreCombat;
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<PositionManager>().OnPreCombat -= KillSpecialistPreCombat;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Assasin;
        }

        public void KillSpecialistPreCombat(object? sender, OnPreCombatEventArgs combatArgs)
        {
            var friendlyEntity = combatArgs.CombatEvent.GetEntityOwnedBy(_owner);
            var enemyEntity = combatArgs.CombatEvent.GetEnemyEntity(_owner);
            
            combatArgs.CombatEvent.AddEffectToCombat(new SpecialistKillSpecialistsEffect(
                combatArgs.CombatEvent.GetOccursAt(),
                friendlyEntity,
                enemyEntity,
                _level < 2
            ));
        }
    }
}