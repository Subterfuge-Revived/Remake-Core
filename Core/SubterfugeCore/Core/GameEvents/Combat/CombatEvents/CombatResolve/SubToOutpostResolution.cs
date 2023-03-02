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

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            _shield.ForwardAction(timeMachine, state);
            _drillerCombatDrillers.ForwardAction(timeMachine, state);
            _combatSpecialistCapture.ForwardAction(timeMachine, state);
            _combatOwnershipTransfer.ForwardAction(timeMachine, state);
            _combatRemoveEmptyEntities.ForwardAction(timeMachine, state);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            _combatRemoveEmptyEntities.BackwardAction(timeMachine, state);
            _combatOwnershipTransfer.BackwardAction(timeMachine, state);
            _combatSpecialistCapture.BackwardAction(timeMachine, state);
            _drillerCombatDrillers.BackwardAction(timeMachine, state);
            _shield.BackwardAction(timeMachine, state);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}