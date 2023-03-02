using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class FriendlyCombatResolution : CombatResolution
    {
        private Sub _sub;
        private Outpost _outpost;

        private int _drillersTransferred;
        private List<Specialist> _specialistsTransferred;

        public FriendlyCombatResolution(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, CombatType.FRIENDLY, combatant1)
        {
            _sub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            _outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _drillersTransferred = _sub.GetComponent<DrillerCarrier>().GetDrillerCount();
            _specialistsTransferred = _sub.GetComponent<SpecialistManager>().GetSpecialists();
            
            _outpost.GetComponent<DrillerCarrier>().AlterDrillers(_drillersTransferred);
            _sub.GetComponent<DrillerCarrier>().AlterDrillers(_drillersTransferred * -1);
            _sub.GetComponent<SpecialistManager>().TransferSpecialistsTo(_outpost.GetComponent<SpecialistManager>());
            state.RemoveSub(_sub);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _outpost.GetComponent<DrillerCarrier>().AlterDrillers(_drillersTransferred * -1);
            _outpost.GetComponent<SpecialistManager>()
                .TransferSpecialistsById(_sub.GetComponent<SpecialistManager>(), _specialistsTransferred.Select(it => it.GetId()).ToList());
            state.AddSub(_sub);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}