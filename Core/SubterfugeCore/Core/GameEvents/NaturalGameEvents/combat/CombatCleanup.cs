using System.Collections.Generic;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat
{
    public class CombatCleanup : IReversible
    {

        private readonly Entity _winner;
        private readonly Entity _loser;
        private bool _isSuccess;
        private readonly bool _isTie;

        private readonly int _initialLoserDrillerCount;
        private readonly Player _losingPlayer;
        private List<Specialist> _loserSpecialists;

        public CombatCleanup(Entity combatant1, Entity combatant2)
        {
            // Determine the losing sub:
            if (combatant1.GetComponent<DrillerCarrier>().GetDrillerCount() < combatant2.GetComponent<DrillerCarrier>().GetDrillerCount())
            {
                _loser = combatant1;
                _winner = combatant2;
            } else if (combatant1.GetComponent<DrillerCarrier>().GetDrillerCount() > combatant2.GetComponent<DrillerCarrier>().GetDrillerCount())
            {
                _loser = combatant2;
                _winner = combatant1;
            }
            else
            {
                // Tie. Compare specialist count.
                if (combatant1.GetComponent<SpecialistManager>().GetSpecialistCount() <
                    combatant2.GetComponent<SpecialistManager>().GetSpecialistCount())
                {
                    _loser = combatant1;
                    _winner = combatant2;
                }
                else if (combatant1.GetComponent<SpecialistManager>().GetSpecialistCount() >
                         combatant2.GetComponent<SpecialistManager>().GetSpecialistCount())
                {
                    _loser = combatant2;
                    _winner = combatant1;
                } else if (combatant1 is Outpost || combatant2 is Outpost)
                {
                    _winner = combatant1 is Outpost ? combatant1 : combatant2;
                    _loser = combatant1 is Outpost ? combatant2 : combatant1;
                }
                else
                {
                    
                    // Complete tie.
                    _isTie = true;
                    // winner & loser don't matter in a tie.
                    _winner = combatant1;
                    _loser = combatant2;
                }
            }

            _initialLoserDrillerCount = _loser.GetComponent<DrillerCarrier>().GetDrillerCount();
            _losingPlayer = _loser.GetComponent<DrillerCarrier>().GetOwner();
            _loserSpecialists = _loser.GetComponent<SpecialistManager>().GetSpecialists();
        }
        
        public bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (_isTie)
            {
                // TODO: Handle tie.
                return false;
            }

            if (_loser is Sub)
            {
                // Cleanup the sub.
                if (_loser.GetComponent<SpecialistManager>().GetSpecialistCount() > 0)
                {
                    _loser.GetComponent<SpecialistManager>().CaptureAll();
                    ((Sub) _loser).GetComponent<DrillerCarrier>().SetCaptured(true);
                }
                else
                {
                    // Remove the sub
                    state.RemoveSub((Sub)_loser);    
                }
            }

            if (_loser is Outpost)
            {
                // Transfer Ownership and give drillers.
                _loser.GetComponent<DrillerCarrier>().SetOwner(_winner.GetComponent<DrillerCarrier>().GetOwner());
                
                // Remove the winning sub and make it arrive at the outpost.
                _loser.GetComponent<DrillerCarrier>().SetDrillerCount(0);
                _loser.GetComponent<DrillerCarrier>().AddDrillers(_winner.GetComponent<DrillerCarrier>().GetDrillerCount());
                
                // Transfer any specialists to the outpost.
                _loser.GetComponent<SpecialistManager>().CaptureAll();
                _winner.GetComponent<SpecialistManager>().TransferSpecialistsTo(_loser.GetComponent<SpecialistManager>());
                
                // Remove the incoming sub.
                state.RemoveSub((Sub)_winner);
            }

            this._isSuccess = true;
            return _isSuccess;
        }

        public bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (_isTie)
            {
                // TODO: Handle tie.
                return false;
            }

            if (_loser is Sub)
            {
                // Undo the sub cleanup
                if (_loser.GetComponent<SpecialistManager>().GetSpecialistCount() > 0)
                {
                    _loser.GetComponent<SpecialistManager>().UncaptureAll();
                    ((Sub) _loser).GetComponent<DrillerCarrier>().SetCaptured(false);
                }
                else
                {
                    // Put the sub back
                    state.AddSub((Sub)_loser);    
                }
            }

            if (_loser is Outpost)
            {
                // Transfer Ownership and give drillers.
                _loser.GetComponent<DrillerCarrier>().SetOwner(_losingPlayer);
                _loser.GetComponent<DrillerCarrier>().SetDrillerCount(_initialLoserDrillerCount);
                
                // Put the winner's specialists back
                _winner.GetComponent<SpecialistManager>().AddSpecialists(_loser.GetComponent<SpecialistManager>().GetPlayerSpecialists(_winner.GetComponent<DrillerCarrier>().GetOwner()));
                // Uncapture the losing player's specialists
                _loser.GetComponent<SpecialistManager>().UncaptureAll();
                
                // Put the incoming sub back
                state.AddSub((Sub)_winner);
            }

            this._isSuccess = true;
            return _isSuccess;
        }

        public bool WasEventSuccessful()
        {
            return _isSuccess;
        }
    }
}