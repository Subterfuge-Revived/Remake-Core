using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class SwapSpecialistEffect : PositionalGameEvent
    {
        private IEntity _combatant1;
        private IEntity _combatant2;
        public SwapSpecialistEffect(
            GameTick occursAt,
            IEntity entityOne,
            IEntity entityTwo
        ) : base(occursAt, Priority.SPECIALIST_SWAP_SPECIALISTS_EFFECT, entityOne)
        {
            _combatant1 = entityOne;
            _combatant2 = entityTwo;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            throw new System.NotImplementedException();
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            throw new System.NotImplementedException();
        }

        public override bool WasEventSuccessful()
        {
            throw new System.NotImplementedException();
        }
    }
}