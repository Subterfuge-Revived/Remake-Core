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

        private readonly CombatDrillersEffect _drillerCombatDrillers;
        private readonly CombatShieldEffect _shield;
        private readonly CombatSpecialistCaptureEffect _combatSpecialistCapture;
        private readonly CombatOwnershipTransferEffect _combatOwnershipTransfer;
        private readonly CombatRemoveEmptyEntitiesEffect _combatRemoveEmptyEntities;
        
        public SubToOutpostResolution(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, CombatType.SUB_TO_OUTPOST, combatant1)
        {
            _sub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            _outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
            _drillerCombatDrillers = new CombatDrillersEffect(occursAt, _sub, _outpost);
            _shield = new CombatShieldEffect(occursAt, _sub, _outpost);
            _combatSpecialistCapture = new CombatSpecialistCaptureEffect(occursAt, _sub, _outpost);
            _combatOwnershipTransfer = new CombatOwnershipTransferEffect(occursAt, _sub, _outpost);
            _combatRemoveEmptyEntities = new CombatRemoveEmptyEntitiesEffect(occursAt, _sub, _outpost, this);
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            _shield.ForwardAction(timeMachine);
            _drillerCombatDrillers.ForwardAction(timeMachine);
            _combatSpecialistCapture.ForwardAction(timeMachine);
            _combatOwnershipTransfer.ForwardAction(timeMachine);
            _combatRemoveEmptyEntities.ForwardAction(timeMachine);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            _combatRemoveEmptyEntities.BackwardAction(timeMachine);
            _combatOwnershipTransfer.BackwardAction(timeMachine);
            _combatSpecialistCapture.BackwardAction(timeMachine);
            _drillerCombatDrillers.BackwardAction(timeMachine);
            _shield.BackwardAction(timeMachine);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}