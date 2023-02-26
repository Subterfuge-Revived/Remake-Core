using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class SpecialistKillSpecialistsEffect : NaturalGameEvent
    {
        private IEntity _friendly;
        private IEntity _enemy;
        private bool _killAll;

        public List<Specialist> killedFriendlySpecs = new List<Specialist>();
        public List<Specialist> killedEnemySpecs = new List<Specialist>();
        
        public SpecialistKillSpecialistsEffect(
            GameTick occursAt,
            IEntity friendly,
            IEntity enemy,
            bool killAll
        ) : base(occursAt, Priority.SPECIALIST_KILL_SPECIALISTS)
        {
            _enemy = enemy;
            _friendly = friendly;
            _killAll = killAll;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            if (_killAll)
            {
                killedFriendlySpecs = _friendly.GetComponent<SpecialistManager>().GetSpecialists();
                killedEnemySpecs = _enemy.GetComponent<SpecialistManager>().GetSpecialists();
                
                _enemy.GetComponent<SpecialistManager>().KillAll();
                _friendly.GetComponent<SpecialistManager>().KillAll();
            }
            else
            {
                killedEnemySpecs = _enemy.GetComponent<SpecialistManager>().GetSpecialists();
                _enemy.GetComponent<SpecialistManager>().KillAll();
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            if (_killAll)
            {
                _enemy.GetComponent<SpecialistManager>().AddFriendlySpecialists(killedEnemySpecs);
                _friendly.GetComponent<SpecialistManager>().AddFriendlySpecialists(killedFriendlySpecs);
            }
            else
            {
                _enemy.GetComponent<SpecialistManager>().AddFriendlySpecialists(killedEnemySpecs);
            }

            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}