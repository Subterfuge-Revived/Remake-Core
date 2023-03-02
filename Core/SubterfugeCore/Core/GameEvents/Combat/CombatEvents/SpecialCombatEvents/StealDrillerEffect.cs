using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class StealDrillerEffect : PositionalGameEvent
    {
        
        private IEntity _friendly;
        private IEntity _stealFrom;
        public StealDrillerEffect(
            GameTick occursAt,
            IEntity friendly,
            IEntity stealFrom
        ) : base(occursAt, Priority.SPECIALIST_STEAL_EFFECT, friendly)
        {
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool WasEventSuccessful()
        {
            throw new System.NotImplementedException();
        }
    }
}