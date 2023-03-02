using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class SlowTargetEffect : PositionalGameEvent
    {
        
        private IEntity _enemy;
        private float _slowBy;
        
        public SlowTargetEffect(
            GameTick occursAt,
            IEntity enemy,
            float slowBy
        ) : base(occursAt, Priority.SPECIALIST_SLOW_EFFECT, enemy)
        {
            _enemy = enemy;
            _slowBy = slowBy;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _enemy.GetComponent<SpeedManager>().DecreaseSpeed(_slowBy);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _enemy.GetComponent<SpeedManager>().IncreaseSpeed(_slowBy);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}