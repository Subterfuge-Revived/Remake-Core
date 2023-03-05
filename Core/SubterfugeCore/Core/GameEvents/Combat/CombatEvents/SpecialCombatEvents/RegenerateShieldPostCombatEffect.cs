using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class RegenerateShieldPostCombatEffect : PositionalGameEvent
    {
        private Player _expectedWinner;
        private CombatEvent _combatEvent;
        private float _percentRegen;
        
        private int shieldsBeforeRestore;
        
        public RegenerateShieldPostCombatEffect(
            CombatEvent combatEvent,
            Player expectedPlayerToWin,
            float percentageToRegenerate
        ) : base(combatEvent.OccursAt, Priority.POST_COMBAT, combatEvent._combatant1)
        {
            _combatEvent = combatEvent;
            _expectedWinner = _expectedWinner;
            _percentRegen = percentageToRegenerate;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            var winningLocation = _combatEvent.GetCombatResolution().Winner;
            if (Equals(winningLocation.GetComponent<DrillerCarrier>().GetOwner(), _expectedWinner))
            {
                var maxShields = winningLocation.GetComponent<ShieldManager>().GetShieldCapacity();
                winningLocation.GetComponent<ShieldManager>().SetShields((int)(maxShields * _percentRegen));
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            var winningLocation = _combatEvent.GetCombatResolution().Winner;
            if (Equals(winningLocation.GetComponent<DrillerCarrier>().GetOwner(), _expectedWinner))
            {
                winningLocation.GetComponent<ShieldManager>().SetShields(shieldsBeforeRestore);
            }

            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}