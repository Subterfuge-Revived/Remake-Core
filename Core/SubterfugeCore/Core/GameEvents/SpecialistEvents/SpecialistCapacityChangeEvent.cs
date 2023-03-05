using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.Timing;
using PositionalGameEvent = Subterfuge.Remake.Core.GameEvents.Combat.PositionalGameEvent;

namespace Subterfuge.Remake.Core.GameEvents.SpecialistEvents
{
    public class SpecialistCapacityChangeEvent : PositionalGameEvent
    {
        private IEntity _expectedWinLocationToApply;
        private int _delta;
        
        public SpecialistCapacityChangeEvent(
            GameTick occursAt,
            IEntity expectedWinLocation,
            int delta
        ) : base(occursAt, Priority.SPECIALIST_CAPCITY_CHANGE, expectedWinLocation)
        {
            _expectedWinLocationToApply = expectedWinLocation;
            _delta = delta;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            _expectedWinLocationToApply.GetComponent<SpecialistManager>().AlterCapacity(_delta);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            _expectedWinLocationToApply.GetComponent<SpecialistManager>().AlterCapacity(_delta * -1);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}