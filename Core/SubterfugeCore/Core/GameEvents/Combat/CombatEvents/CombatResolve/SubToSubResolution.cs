using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class SubToSubResolution: CombatResolution
    {
        private Sub _sub1;
        private Sub _sub2;

        private readonly NaturalDrillerCombatEffect _drillerCombat;
        private readonly NaturalShieldCombatEffect _shieldCombat;
        private readonly SpecialistCaptureEffect _specialistCapture;
        private readonly RemoveLostCombatantsEffect _removeLostCombatants;
        
        public SubToSubResolution(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, CombatType.SUB_TO_SUB)
        {
            _sub1 = combatant1 as Sub;
            _sub2 = combatant2 as Sub;
            _drillerCombat = new NaturalDrillerCombatEffect(occursAt, _sub1, _sub2);
            _shieldCombat = new NaturalShieldCombatEffect(occursAt, _sub1, _sub2);
            _specialistCapture = new SpecialistCaptureEffect(occursAt, _sub1, _sub2);
            _removeLostCombatants = new RemoveLostCombatantsEffect(occursAt, _sub1, _sub2, this);
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _shieldCombat.ForwardAction(timeMachine, state);
            _drillerCombat.ForwardAction(timeMachine, state);
            _specialistCapture.ForwardAction(timeMachine, state);
            _removeLostCombatants.ForwardAction(timeMachine, state);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _removeLostCombatants.BackwardAction(timeMachine, state);
            _specialistCapture.BackwardAction(timeMachine, state);
            _drillerCombat.BackwardAction(timeMachine, state);
            _shieldCombat.BackwardAction(timeMachine, state);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}