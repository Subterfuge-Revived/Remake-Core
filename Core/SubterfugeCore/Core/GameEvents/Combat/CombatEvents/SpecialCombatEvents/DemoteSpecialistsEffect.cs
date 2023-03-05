using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class DemoteSpecialistsEffect : PositionalGameEvent
    {
        
        private IEntity _combatant1;
        public DemoteSpecialistsEffect(
            GameTick occursAt,
            IEntity demoteSpecialistsAtEntity
        ) : base(occursAt, Priority.SPECIALIST_DEMOTE_EFFECT, demoteSpecialistsAtEntity)
        {
            _combatant1 = demoteSpecialistsAtEntity;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            throw new System.NotImplementedException();
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            throw new System.NotImplementedException();
        }

        public override bool WasEventSuccessful()
        {
            throw new System.NotImplementedException();
        }
    }
}