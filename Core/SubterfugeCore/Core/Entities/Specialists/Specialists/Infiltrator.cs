using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Infiltrator : Specialist
    {
        public Infiltrator(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat += OnPreCombat;
                if (GetLevel() >= 2)
                {
                    entity.GetComponent<PositionManager>().OnPostCombat += OnPostCombat;
                }
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnPreCombat -= OnPreCombat;
                if (GetLevel() >= 2)
                {
                    entity.GetComponent<PositionManager>().OnPostCombat -= OnPostCombat;
                }
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<PositionManager>().OnPreCombat -= OnPreCombat;
            if (GetLevel() >= 2)
            {
                captureLocation.GetComponent<PositionManager>().OnPostCombat -= OnPostCombat;
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Infiltrator;
        }
        
        private void OnPreCombat(object positionManager, OnPreCombatEventArgs preCombatEventArgs)
        {
            var friendlyEntity = preCombatEventArgs.CombatEvent.GetEntityOwnedBy(_owner);
            var enemyEntity = preCombatEventArgs.CombatEvent.GetEnemyEntity(_owner);
            
            preCombatEventArgs.CombatEvent.AddEffectToCombat(new SpecialistShieldEffect(
                preCombatEventArgs.CombatEvent,
                friendlyEntity,
                0,
                GetShieldDelta(enemyEntity)
            ));
        }

        private void OnPostCombat(object positionManager, PostCombatEventArgs postCombatEventArgs)
        {
            if (postCombatEventArgs.Direction == TimeMachineDirection.FORWARD)
            {
                if (GetLevel() >= 2)
                {
                    var winningEntity = postCombatEventArgs.CombatResolution.Winner;
                    var losingEntity = postCombatEventArgs.CombatResolution.Loser;
                    if (Equals(winningEntity.GetComponent<DrillerCarrier>().GetOwner(), _owner) && losingEntity is Outpost)
                    {
                        RestoreLostShields(losingEntity);
                    }
                }
            }
            else
            {
                if (GetLevel() >= 2)
                {
                    var winningEntity = postCombatEventArgs.CombatResolution.Winner;
                    var losingEntity = postCombatEventArgs.CombatResolution.Loser;
                    if (Equals(winningEntity.GetComponent<DrillerCarrier>().GetOwner(), _owner) && losingEntity is Outpost)
                    {
                        UndoShieldRestore(losingEntity);
                    }
                }
            }
        }

        private void RestoreLostShields(IEntity restoreLocation)
        {
            var shieldManager = restoreLocation.GetComponent<ShieldManager>();
            var capacity = shieldManager.GetShieldCapacity();
            if (GetLevel() >= 2)
            {
                shieldManager.SetShields(capacity);
            }
        }

        private void UndoShieldRestore(IEntity restoreLocation)
        {
            var shieldManager = restoreLocation.GetComponent<ShieldManager>();
            shieldManager.SetShields(0);
        }

        private int GetShieldDelta(IEntity targetLocation)
        {
            return _level switch
            {
                1 => 15,
                2 => 20,
                3 => targetLocation.GetComponent<ShieldManager>().GetShields(),
                _ => 0
            };
        }
    }
}