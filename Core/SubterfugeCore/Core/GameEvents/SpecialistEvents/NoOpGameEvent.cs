using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.Timing;
using PositionalGameEvent = Subterfuge.Remake.Core.GameEvents.Combat.PositionalGameEvent;

namespace Subterfuge.Remake.Core.GameEvents.SpecialistEvents
{
    public class NoOpGameEvent : GameEvent
    {
        public NoOpGameEvent(GameTick occursAt) : base(occursAt, Priority.COMBAT_RESOLVE)
        {
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            this.EventSuccess = true;
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            return true;
        }

        public override string GetEventId()
        {
            return "NoOp";
        }

        public override bool Equals(object other)
        {
            NoOpGameEvent asEvent = other as NoOpGameEvent;
            if (other == null)
                return false;

            return asEvent.OccursAt == this.OccursAt;
        }

        public override int GetHashCode()
        {
            return OccursAt.GetHashCode();
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }
    }
}