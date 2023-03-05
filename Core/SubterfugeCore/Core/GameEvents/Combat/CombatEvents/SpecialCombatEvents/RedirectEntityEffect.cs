using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class RedirectEntityEffect : PositionalGameEvent
    {
        private IEntity _enemy;

        private IEntity _originalDestination;
        
        public RedirectEntityEffect(
            GameTick occursAt,
            IEntity enemy
        ) : base(occursAt, Priority.SPECIALIST_SUB_REDIRECT, enemy)
        {
            _enemy = enemy;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            _originalDestination = _enemy.GetComponent<PositionManager>().GetDestination();
            var _source = _enemy.GetComponent<PositionManager>().GetSource();
            _enemy.GetComponent<PositionManager>().SetDestination(_source);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            _enemy.GetComponent<PositionManager>().SetDestination(_originalDestination);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}