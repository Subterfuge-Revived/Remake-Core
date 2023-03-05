using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class CombatResolveEvent : PositionalGameEvent
    {
        private readonly IEntity _combatant1;
        private readonly IEntity _combatant2;
        
        public CombatResolution CombatResolution;
        private CombatEvent _combatEvent;
        
        public CombatResolveEvent(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2,
            CombatEvent combatEvent
        ) : base(occursAt, Priority.COMBAT_RESOLVE, combatant1)
        {
            _combatant1 = combatant1;
            _combatant2 = combatant2;
            _combatEvent = combatEvent;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            switch (DetermineCombatType())
            {
                case CombatType.FRIENDLY:
                    CombatResolution = new FriendlyCombatResolution(OccursAt, _combatant1, _combatant2);
                    break;
                case CombatType.SUB_TO_SUB:
                    CombatResolution = new SubToSubResolution(OccursAt, _combatant1, _combatant2);
                    break;
                case CombatType.SUB_TO_OUTPOST:
                    CombatResolution = new SubToOutpostResolution(OccursAt, _combatant1, _combatant2);
                    break;
                default:
                    break;
            }

            return CombatResolution.ForwardAction(timeMachine);
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            return CombatResolution.BackwardAction(timeMachine);
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }

        private CombatType DetermineCombatType()
        {
            if (Equals(_combatant1.GetComponent<DrillerCarrier>().GetOwner(), _combatant2.GetComponent<DrillerCarrier>().GetOwner()))
            {
                return CombatType.FRIENDLY;
            }
            
            if (_combatant1 is Sub && _combatant2 is Sub)
            {
                return CombatType.SUB_TO_SUB;
            }

            return CombatType.SUB_TO_OUTPOST;
        }
    }
}

public enum CombatType
{
    Unknown = 0,
    SUB_TO_SUB = 1,
    SUB_TO_OUTPOST = 2,
    FRIENDLY = 3,
}