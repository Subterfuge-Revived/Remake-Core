using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class AlterShieldEffect : PositionalGameEvent
    {
        private IEntity _location;

        private int _shieldDelta;
        public AlterShieldEffect(
            CombatEvent combatEvent,
            IEntity location,
            int shieldDelta
        ) : base(combatEvent.OccursAt, Priority.SPECIALIST_SHIELD_EFFECT, location)
        {
            _location = location;
            _shieldDelta = shieldDelta;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _shieldDelta = _location.GetComponent<ShieldManager>().AlterShields(_shieldDelta);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _shieldDelta = _location.GetComponent<ShieldManager>().AlterShields(-1 * _shieldDelta);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}