using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class AlterShieldEffect : PositionalGameEvent
    {
        private IEntity _expectedWinLocation;

        private int _shieldDelta;
        public AlterShieldEffect(
            CombatEvent combatEvent,
            IEntity expectedWinLocation,
            int shieldDelta
        ) : base(combatEvent.OccursAt, Priority.SPECIALIST_SHIELD_EFFECT, expectedWinLocation)
        {
            _expectedWinLocation = expectedWinLocation;
            _shieldDelta = shieldDelta;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            _shieldDelta = _expectedWinLocation.GetComponent<ShieldManager>().AlterShields(_shieldDelta);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            _shieldDelta = _expectedWinLocation.GetComponent<ShieldManager>().AlterShields(-1 * _shieldDelta);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}