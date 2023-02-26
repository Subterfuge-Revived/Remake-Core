using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class SubToOutpostResolution : CombatResolution
    {
        private Sub _sub;
        private Outpost _outpost;

        private readonly NaturalDrillerCombatEffect _drillerCombat;
        private readonly NaturalShieldCombatEffect _shieldCombat;
        private readonly SpecialistCaptureEffect _specialistCapture;
        private readonly OwnershipTransferEffect _ownershipTransfer;
        private readonly RemoveLostCombatantsEffect _removeLostCombatants;
        
        public SubToOutpostResolution(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, CombatType.SUB_TO_OUTPOST)
        {
            _sub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            _outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
            _drillerCombat = new NaturalDrillerCombatEffect(occursAt, _sub, _outpost);
            _shieldCombat = new NaturalShieldCombatEffect(occursAt, _sub, _outpost);
            _specialistCapture = new SpecialistCaptureEffect(occursAt, _sub, _outpost);
            _ownershipTransfer = new OwnershipTransferEffect(occursAt, _sub, _outpost);
            _removeLostCombatants = new RemoveLostCombatantsEffect(occursAt, _sub, _outpost, this);
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _shieldCombat.ForwardAction(timeMachine, state);
            _drillerCombat.ForwardAction(timeMachine, state);
            _specialistCapture.ForwardAction(timeMachine, state);
            _ownershipTransfer.ForwardAction(timeMachine, state);
            _removeLostCombatants.ForwardAction(timeMachine, state);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _removeLostCombatants.BackwardAction(timeMachine, state);
            _ownershipTransfer.BackwardAction(timeMachine, state);
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