using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat.CombatEvents
{
    public class SpecialistStealEffect : NaturalGameEvent
    {
        public SpecialistStealEffect(GameTick occursAt, Priority priority) : base(occursAt, priority)
        {
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool WasEventSuccessful()
        {
            throw new System.NotImplementedException();
        }
    }
}