using System.Collections.Generic;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat
{
    public class CombatModifier
    {

        private IEntity _friendly;
        private IEntity _enemy;
        
        public int Priority { get; set; }
        public int EnemyDrillersDestroyed { get; set; } = 0;
        public int FriendlyDrillersAdded { get; set; } = 0; // From stealing.
        
        public bool KillSpecialists { get; set; } = false;
        private List<Specialist> KilledSpecialists = new List<Specialist>();
        
        public bool RedirectEnemy { get; set; } = false;
        public IEntity OriginalTarget;
        
        
        public bool PreventFutureModifiers { get; set; } = false;
        public float SlowEnemyBy { get; set; } = 0.0f;

        public CombatModifier(IEntity friendly, IEntity enemy)
        {
            _friendly = friendly;
            _enemy = enemy;
        }


        public void ApplyForward()
        {
            _enemy.GetComponent<DrillerCarrier>().AlterDrillers(EnemyDrillersDestroyed * -1);
            _friendly.GetComponent<DrillerCarrier>().AlterDrillers(FriendlyDrillersAdded);
            if (KillSpecialists)
            {
                KilledSpecialists = _enemy.GetComponent<SpecialistManager>().GetSpecialists();
                _enemy.GetComponent<SpecialistManager>().KillAll();
            }

            if (RedirectEnemy)
            {
                OriginalTarget = _enemy.GetComponent<PositionManager>().GetDestination();
                var Source = _enemy.GetComponent<PositionManager>().GetSource();
                _enemy.GetComponent<PositionManager>().SetDestination(Source);
            }

            _enemy.GetComponent<SpeedManager>().DecreaseSpeed(SlowEnemyBy);
        }

        public void BackwardAction()
        {
            _enemy.GetComponent<DrillerCarrier>().AlterDrillers(EnemyDrillersDestroyed);
            _friendly.GetComponent<DrillerCarrier>().AlterDrillers(FriendlyDrillersAdded * -1);
            if (KillSpecialists)
            {
                _enemy.GetComponent<SpecialistManager>().AddFriendlySpecialists(KilledSpecialists);
            }

            if (RedirectEnemy)
            {
                _enemy.GetComponent<PositionManager>().SetDestination(OriginalTarget);
            }

            _enemy.GetComponent<SpeedManager>().IncreaseSpeed(SlowEnemyBy);
        }
    }
}