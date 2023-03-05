using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Pirate : Specialist
    {
        public List<int> damagePerLevel = new List<int>() { 0, 15, 25 };
        
        public Pirate(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SubLauncher>().CanTargetSubs = true;
                if (entity is Sub)
                {
                    entity.GetComponent<PositionManager>().OnRegisterCombatEffects += DamageIfOnSub;
                }
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SubLauncher>().CanTargetSubs = false;
                if (entity is Sub)
                {
                    entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= DamageIfOnSub;
                }
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<SubLauncher>().CanTargetSubs = false;
            if (entity is Sub)
            {
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= DamageIfOnSub;
            }
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Pirate;
        }

        public override string GetDescription()
        {
            return $"The Pirate can directly target enemy subs." +
                   $"Returns home after combat." +
                   $"When attacking enemy subs, deals an additional {damagePerLevel} damage";
        }

        public void DamageIfOnSub(object? sender, OnRegisterCombatEventArgs registerCombatEventArgs)
        {
            var friendlyEntity = registerCombatEventArgs.CombatEvent.GetEntityOwnedBy(_owner);
            
            registerCombatEventArgs.CombatEvent.AddEffectToCombat(new AlterDrillerEffect(
                registerCombatEventArgs.CombatEvent,
                friendlyEntity,
                0,
                damagePerLevel[_level] * -1
            ));
        }
    }
}