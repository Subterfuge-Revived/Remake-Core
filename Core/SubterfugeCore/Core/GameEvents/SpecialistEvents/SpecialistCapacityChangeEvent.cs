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
        private IEntity _locationToApply;
        private int _delta;
        
        public SpecialistCapacityChangeEvent(
            GameTick occursAt,
            IEntity location,
            int delta
        ) : base(occursAt, Priority.NaturalPriority9, location)
        {
            _locationToApply = location;
            _delta = delta;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _locationToApply.GetComponent<SpecialistManager>().AlterCapacity(_delta);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _locationToApply.GetComponent<SpecialistManager>().AlterCapacity(_delta * -1);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}