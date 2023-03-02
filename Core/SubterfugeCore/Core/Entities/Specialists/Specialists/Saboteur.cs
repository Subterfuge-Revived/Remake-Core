/*using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Saboteur : Specialist
    {
        public Saboteur(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat += RedirectOnCombat;
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat -= RedirectOnCombat;
            }
        }

        public override void OnCapturedEvent(IEntity captureLocation)
        {
            captureLocation.GetComponent<PositionManager>().OnPreCombat -= RedirectOnCombat;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Saboteur;
        }

        public void RedirectOnCombat(object? sender, OnPreCombatEventArgs combatArgs)
        {
            var enemyEntity = combatArgs.CombatEvent.GetEnemyEntity(_owner);
            
            combatArgs.CombatEvent.AddEffectToCombat(new SpecialistSubRedirectEffect(
                combatArgs.CombatEvent.GetOccursAt(),
                enemyEntity
            ));
            
            combatArgs.CombatEvent.AddEffectToCombat(new SpecialistSlowEffect(
                combatArgs.CombatEvent.GetOccursAt(),
                enemyEntity,
                GetSlowAmount()
            ));
        }

        public float GetSlowAmount()
        {
            return _level switch
            {
                0 => 0,
                1 => 0,
                2 => 0.1f,
                3 => 0.2f,
            };
        }
    }
}*/