using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Pirate : Specialist
    {
        public Pirate(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SubLauncher>().CanTargetSubs = true;
                if (entity is Sub)
                {
                    entity.GetComponent<PositionManager>().OnPreCombat += DamageIfOnSub;
                }
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SubLauncher>().CanTargetSubs = false;
                if (entity is Sub)
                {
                    entity.GetComponent<PositionManager>().OnPreCombat -= DamageIfOnSub;
                }
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<SubLauncher>().CanTargetSubs = false;
            if (captureLocation is Sub)
            {
                captureLocation.GetComponent<PositionManager>().OnPreCombat -= DamageIfOnSub;
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Pirate;
        }

        public void DamageIfOnSub(object? sender, OnPreCombatEventArgs preCombatEventArgs)
        {
            var friendlyEntity = preCombatEventArgs.CombatEvent.GetEntityOwnedBy(_owner);
            
            preCombatEventArgs.CombatEvent.AddEffectToCombat(new SpecialistDrillerEffect(
                preCombatEventArgs.CombatEvent,
                friendlyEntity,
                0,
                GetSubDamange() * -1
            ));
        }

        public int GetSubDamange()
        {
            return _level switch
            {
                0 => 0,
                1 => 15,
                2 => 25,
                _ => 0,
            };
        }
    }
}