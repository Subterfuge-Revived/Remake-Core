using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class SpecialistShieldEffect : NaturalGameEvent
    {
        private IEntity _friendly;
        private IEntity _enemy;

        private int _friendlyShieldDelta;
        private int _enemyShieldDelta;
        public SpecialistShieldEffect(
            CombatEvent combatEvent,
            IEntity friendly,
            int friendlyShieldDelta,
            int enemyShieldDelta
        ) : base(combatEvent.GetOccursAt(), Priority.SPECIALIST_SHIELD_EFFECT)
        {
            _friendly = friendly;
            _enemy = combatEvent._combatant1 == _friendly ? combatEvent._combatant1 : combatEvent._combatant2;
            _friendlyShieldDelta = friendlyShieldDelta;
            _enemyShieldDelta = enemyShieldDelta;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _friendlyShieldDelta = _friendly.GetComponent<ShieldManager>().AddShield(_friendlyShieldDelta);
            _enemyShieldDelta = _friendly.GetComponent<ShieldManager>().RemoveShields(_enemyShieldDelta);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _friendlyShieldDelta = _friendly.GetComponent<ShieldManager>().RemoveShields(_friendlyShieldDelta);
            _enemyShieldDelta = _friendly.GetComponent<ShieldManager>().AddShield(_enemyShieldDelta);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}