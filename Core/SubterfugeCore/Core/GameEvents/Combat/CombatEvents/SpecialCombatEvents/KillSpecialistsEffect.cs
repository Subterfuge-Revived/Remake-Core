using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class KillSpecialistsEffect : PositionalGameEvent
    {
        private IEntity _killSpecialistsAt;
        
        public List<Specialist> killedSpecialists = new List<Specialist>();
        
        public KillSpecialistsEffect(
            GameTick occursAt,
            IEntity killSpecialistsAt
        ) : base(occursAt, Priority.SPECIALIST_KILL_SPECIALISTS, killSpecialistsAt)
        {
            _killSpecialistsAt = killSpecialistsAt;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            killedSpecialists = _killSpecialistsAt.GetComponent<SpecialistManager>().GetSpecialists();
            _killSpecialistsAt.GetComponent<SpecialistManager>().KillAll();
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _killSpecialistsAt.GetComponent<SpecialistManager>().AddFriendlySpecialists(killedSpecialists);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}