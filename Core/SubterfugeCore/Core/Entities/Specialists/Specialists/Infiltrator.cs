using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Infiltrator : Specialist
    {
        public Infiltrator(Player owner) : base(owner, false)
        {
        }
        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects += OnRegisterCombatEffects;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= OnRegisterCombatEffects;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= OnRegisterCombatEffects;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Infiltrator;
        }

        public override string GetDescription()
        {
            return $"When participating in combat, the Infiltrator destroys {{15, 20, All}} enemy shields.";
        }

        private void OnRegisterCombatEffects(object positionManager, OnRegisterCombatEventArgs registerCombatEventArgs)
        {
            var friendlyEntity = registerCombatEventArgs.CombatEvent.GetEntityOwnedBy(_owner);
            var enemyEntity = registerCombatEventArgs.CombatEvent.GetEnemyEntity(_owner);
            
            registerCombatEventArgs.CombatEvent.AddEffectToCombat(new AlterShieldEffect(
                registerCombatEventArgs.CombatEvent,
                friendlyEntity,
                GetShieldDelta(enemyEntity)
            ));

            if (GetLevel() >= 2)
            {
                registerCombatEventArgs.CombatEvent.AddEffectToCombat(new RegenerateShieldPostCombatEffect(
                    registerCombatEventArgs.CombatEvent,
                    _owner,
                    1.0f
                ));
            }
        }

        private int GetShieldDelta(IEntity targetLocation)
        {
            return _level switch
            {
                1 => 15,
                2 => 25,
                3 => targetLocation.GetComponent<ShieldManager>().GetShields(),
                _ => 0
            };
        }
    }
}