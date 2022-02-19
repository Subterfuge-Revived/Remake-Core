using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    public class CombatCleanup : IReversible
    {

        private Entity winner;
        private Entity loser;
        private bool isSuccess = false;
        private bool isTie = false;
        
        private int initialLoserDrillerCount = 0;
        private Player losingPlayer;
        private List<Specialist> loserSpecialists;

        public CombatCleanup(Entity combatant1, Entity combatant2)
        {
            // Determine the losing sub:
            if (combatant1.GetComponent<DrillerCarrier>().GetDrillerCount() < combatant2.GetComponent<DrillerCarrier>().GetDrillerCount())
            {
                loser = combatant1;
                winner = combatant2;
            } else if (combatant1.GetComponent<DrillerCarrier>().GetDrillerCount() > combatant2.GetComponent<DrillerCarrier>().GetDrillerCount())
            {
                loser = combatant2;
                winner = combatant1;
            }
            else
            {
                // Tie. Compare specialist count.
                if (combatant1.GetComponent<SpecialistManager>().GetSpecialistCount() <
                    combatant2.GetComponent<SpecialistManager>().GetSpecialistCount())
                {
                    loser = combatant1;
                    winner = combatant2;
                }
                else if (combatant1.GetComponent<SpecialistManager>().GetSpecialistCount() >
                         combatant2.GetComponent<SpecialistManager>().GetSpecialistCount())
                {
                    loser = combatant2;
                    winner = combatant1;
                } else if (combatant1 is Outpost || combatant2 is Outpost)
                {
                    winner = combatant1 is Outpost ? combatant1 : combatant2;
                    loser = combatant1 is Outpost ? combatant2 : combatant1;
                }
                else
                {
                    
                    // Complete tie.
                    isTie = true;
                    // winner & loser don't matter in a tie.
                    winner = combatant1;
                    loser = combatant2;
                }
            }

            initialLoserDrillerCount = loser.GetComponent<DrillerCarrier>().GetDrillerCount();
            losingPlayer = loser.GetComponent<DrillerCarrier>().GetOwner();
            loserSpecialists = loser.GetComponent<SpecialistManager>().GetSpecialists();
        }
        
        public bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            if (isTie)
            {
                // TODO: Handle tie.
                return false;
            }

            if (loser is Sub)
            {
                // Cleanup the sub.
                if (loser.GetComponent<SpecialistManager>().GetSpecialistCount() > 0)
                {
                    loser.GetComponent<SpecialistManager>().captureAll();
                    ((Sub) loser).GetComponent<DrillerCarrier>().SetCaptured(true);
                }
                else
                {
                    // Remove the sub
                    state.RemoveSub((Sub)loser);    
                }
            }

            if (loser is Outpost)
            {
                // Transfer Ownership and give drillers.
                loser.GetComponent<DrillerCarrier>().SetOwner(winner.GetComponent<DrillerCarrier>().GetOwner());
                
                // Remove the winning sub and make it arrive at the outpost.
                loser.GetComponent<DrillerCarrier>().SetDrillerCount(0);
                loser.GetComponent<DrillerCarrier>().AddDrillers(winner.GetComponent<DrillerCarrier>().GetDrillerCount());
                
                // Transfer any specialists to the outpost.
                loser.GetComponent<SpecialistManager>().captureAll();
                winner.GetComponent<SpecialistManager>().transferSpecialistsTo(loser.GetComponent<SpecialistManager>());
                
                // Remove the incoming sub.
                state.RemoveSub((Sub)winner);
            }

            this.isSuccess = true;
            return isSuccess;
        }

        public bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            if (isTie)
            {
                // TODO: Handle tie.
                return false;
            }

            if (loser is Sub)
            {
                // Undo the sub cleanup
                if (loser.GetComponent<SpecialistManager>().GetSpecialistCount() > 0)
                {
                    loser.GetComponent<SpecialistManager>().uncaptureAll();
                    ((Sub) loser).GetComponent<DrillerCarrier>().SetCaptured(false);
                }
                else
                {
                    // Put the sub back
                    state.AddSub((Sub)loser);    
                }
            }

            if (loser is Outpost)
            {
                // Transfer Ownership and give drillers.
                loser.GetComponent<DrillerCarrier>().SetOwner(losingPlayer);
                loser.GetComponent<DrillerCarrier>().SetDrillerCount(initialLoserDrillerCount);
                
                // Put the winner's specialists back
                winner.GetComponent<SpecialistManager>().AddSpecialists(loser.GetComponent<SpecialistManager>().GetPlayerSpecialists(winner.GetComponent<DrillerCarrier>().GetOwner()));
                // Uncapture the losing player's specialists
                loser.GetComponent<SpecialistManager>().uncaptureAll();
                
                // Put the incoming sub back
                state.AddSub((Sub)winner);
            }

            this.isSuccess = true;
            return isSuccess;
        }

        public bool WasEventSuccessful()
        {
            return isSuccess;
        }
    }
}