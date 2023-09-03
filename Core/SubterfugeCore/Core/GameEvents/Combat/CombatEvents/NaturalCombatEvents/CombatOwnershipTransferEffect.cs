using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class CombatOwnershipTransferEffect : PositionalGameEvent
    {
        // Ownership transfer only applies to sub-to-outpost combat.
        private readonly Sub _sub;
        private readonly Outpost _outpost;

        private IEntity _winner;
        private readonly CombatType _combatType;

        private Player _initialOwner;
        private List<Specialist> specialistsTransferred;
        private int drillersTransferred;

        public CombatOwnershipTransferEffect(
            GameTick occursAt,
            IEntity combatant1,
            IEntity combatant2
        ) : base(occursAt, Priority.COMBAT_RESOLVE, combatant1)
        {
            _combatType = DetermineCombatType(combatant1, combatant2);
            
            if (_combatType == CombatType.SUB_TO_OUTPOST)
            {
                _sub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
                _outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
            }
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            if (_combatType == CombatType.SUB_TO_OUTPOST)
            {
                _winner = GetWinner();
                if (_winner is Sub)
                {
                    // Swap ownership
                    _initialOwner = _outpost.GetComponent<DrillerCarrier>().GetOwner();
                    _outpost.GetComponent<DrillerCarrier>().SetOwner(_winner.GetComponent<DrillerCarrier>().GetOwner());

                    // Transfer drillers
                    drillersTransferred = _sub.GetComponent<DrillerCarrier>().GetDrillerCount();
                    _outpost.GetComponent<DrillerCarrier>().AlterDrillers(drillersTransferred);
                    _sub.GetComponent<DrillerCarrier>().AlterDrillers(drillersTransferred * -1);
                    
                    // Transfer specialists
                    specialistsTransferred = _sub.GetComponent<SpecialistManager>().GetSpecialists(); 
                    _sub.GetComponent<SpecialistManager>()
                        .TransferSpecialistsTo(_outpost.GetComponent<SpecialistManager>(), timeMachine);
                }
                else
                {
                    // Defender won.
                    
                    // Transfer specialists
                    specialistsTransferred = _sub.GetComponent<SpecialistManager>().GetSpecialists(); 
                    _sub.GetComponent<SpecialistManager>()
                        .TransferSpecialistsTo(_outpost.GetComponent<SpecialistManager>(), timeMachine);
                }
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            if (_combatType == CombatType.SUB_TO_OUTPOST)
            {
                if (_winner is Sub)
                {
                    // Put specialists back on the sub.
                    _outpost.GetComponent<SpecialistManager>()
                        .TransferSpecialistsById(_sub.GetComponent<SpecialistManager>(), specialistsTransferred.Select(it => it.GetSpecialistId().ToString()).ToList(), timeMachine);
                    
                    // Put drillers back on the sub.
                    _sub.GetComponent<DrillerCarrier>().AlterDrillers(drillersTransferred);
                    _outpost.GetComponent<DrillerCarrier>().AlterDrillers(drillersTransferred * -1);
                    
                    // Restore ownership.
                    _outpost.GetComponent<DrillerCarrier>().SetOwner(_initialOwner);
                }
                else
                {
                    // Put specialists back on the sub.
                    _outpost.GetComponent<SpecialistManager>()
                        .TransferSpecialistsById(_sub.GetComponent<SpecialistManager>(), specialistsTransferred.Select(it => it.GetSpecialistId().ToString()).ToList(), timeMachine);
                }
            }

            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
        
        private IEntity? GetWinner()
        {
            var subDrillerCount = _sub.GetComponent<DrillerCarrier>().GetDrillerCount();
            var outpostDrillerCount = _outpost.GetComponent<DrillerCarrier>().GetDrillerCount();
            if (subDrillerCount <= 0 && outpostDrillerCount <= 0)
            {
                // Defender wins in the case of a tie
                return _outpost;
            }

            if (subDrillerCount <= 0)
            {
                return _outpost;
            }

            if (outpostDrillerCount <= 0)
            {
                return _sub;
            }

            return _sub;
        }
        
        private CombatType DetermineCombatType(IEntity _combatant1, IEntity _combatant2)
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