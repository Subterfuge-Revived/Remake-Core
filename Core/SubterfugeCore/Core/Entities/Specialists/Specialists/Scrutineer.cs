using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Scrutineer : Specialist
    {

        public List<int> TickReductionPerLevel = new List<int>() { Constants.BASE_SHIELD_REGENERATION_TICKS / 4, Constants.BASE_SHIELD_REGENERATION_TICKS / 2, Constants.BASE_SHIELD_REGENERATION_TICKS / 2 };
        
        public Scrutineer(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<ShieldProducer>().Producer.ChangeTicksPerProductionCycle(TickReductionPerLevel[_level]);
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects += RegisterPostCombatShieldRegeneration;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<ShieldProducer>().Producer.ChangeTicksPerProductionCycle( TickReductionPerLevel[_level]);
                entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= RegisterPostCombatShieldRegeneration;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<ShieldProducer>().Producer.ChangeTicksPerProductionCycle( TickReductionPerLevel[_level]);
            entity.GetComponent<PositionManager>().OnRegisterCombatEffects -= RegisterPostCombatShieldRegeneration;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Scrutineer;
        }

        public override string GetDescription()
        {
            return $"Reduces the shield regeneration rate by {TickReductionPerLevel} ticks." +
                   $"Regenerates all shields after a successful combat.";
        }

        private void RegisterPostCombatShieldRegeneration(object positionManager, OnRegisterCombatEventArgs postCombatEventArgs)
        {
            if (GetLevel() >= 3)
            {
                postCombatEventArgs.CombatEvent.AddEffectToCombat(new RegenerateShieldPostCombatEffect(
                    postCombatEventArgs.CombatEvent,
                    _owner,
                    1.0f
                ));
            }
        }
    }
}