using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class CombatRemoveEmptyEntitiesEffect : PositionalGameEvent
    {
        private readonly IEntity _combatant1;
        private readonly IEntity _combatant2;
        private readonly CombatResolution _combatResolution;

        public CombatRemoveEmptyEntitiesEffect(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2,
            CombatResolution combatResolution
        ) : base(occursAt, Priority.COMBAT_RESOLVE, combatant1)
        {
            _combatant1 = combatant1;
            _combatant2 = combatant2;
            _combatResolution = combatResolution;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            if (_combatant1.GetComponent<DrillerCarrier>().GetDrillerCount() <= 0 &&
                _combatant2.GetComponent<DrillerCarrier>().GetDrillerCount() <= 0)
            {
                // Tie.
                _combatResolution.IsTie = true;
            }
            else
            {
                if (_combatant1 is Sub sub1 && sub1.GetComponent<DrillerCarrier>().GetDrillerCount() <= 0)
                {
                    state.RemoveSub(sub1);
                    _combatResolution.Winner = _combatant2;
                    _combatResolution.Loser = _combatant1;
                }
                if (_combatant2 is Sub sub2 && sub2.GetComponent<DrillerCarrier>().GetDrillerCount() <= 0)
                {
                    state.RemoveSub(sub2);
                    _combatResolution.Winner = _combatant1;
                    _combatResolution.Loser = _combatant2;
                }
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            if (_combatant1 is Sub sub1 && !state.SubExists(sub1))
            {
                state.AddSub(sub1);
            }
            if (_combatant2 is Sub sub2 && !state.SubExists(sub2))
            {
                state.AddSub(sub2);
            }

            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}