using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    public class CombatCleanup : IReversible
    {

        private ICombatable winner;
        private ICombatable loser;
        private bool isSuccess = false;
        private bool isTie = false;
        
        private int initialLoserDrillerCount = 0;
        private Player losingPlayer;
        private List<Specialist> loserSpecialists;

        public CombatCleanup(ICombatable combatant1, ICombatable combatant2)
        {
            // Determine the losing sub:
            if (combatant1.GetDrillerCount() < combatant2.GetDrillerCount())
            {
                loser = combatant1;
                winner = combatant2;
            } else if (combatant1.GetDrillerCount() > combatant2.GetDrillerCount())
            {
                loser = combatant2;
                winner = combatant1;
            }
            else
            {
                // Tie. Compare specialist count.
                if (combatant1.GetSpecialistManager().GetSpecialistCount() <
                    combatant2.GetSpecialistManager().GetSpecialistCount())
                {
                    loser = combatant1;
                    winner = combatant2;
                }
                else if (combatant2.GetSpecialistManager().GetSpecialistCount() >
                         combatant2.GetSpecialistManager().GetSpecialistCount())
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

            initialLoserDrillerCount = loser.GetDrillerCount();
            losingPlayer = loser.GetOwner();
            loserSpecialists = loser.GetSpecialistManager().GetSpecialists();
        }
        
        public bool ForwardAction(GameState state)
        {
            if (isTie)
            {
                // TODO: Handle tie.
                return false;
            }

            if (loser is Sub)
            {
                // Cleanup the sub.
                if (loser.GetSpecialistManager().GetSpecialistCount() > 0)
                {
                    loser.GetSpecialistManager().captureAll();
                    ((Sub) loser).IsCaptured = true;
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
                loser.SetOwner(winner.GetOwner());
                
                // Remove the winning sub and make it arrive at the outpost.
                loser.SetDrillerCount(0);
                loser.AddDrillers(winner.GetDrillerCount());
                
                // Transfer any specialists to the outpost.
                loser.GetSpecialistManager().captureAll();
                winner.GetSpecialistManager().transferSpecialistsTo(loser.GetSpecialistManager());
                
                // Remove the incoming sub.
                state.RemoveSub((Sub)winner);
            }

            this.isSuccess = true;
            return isSuccess;
        }

        public bool BackwardAction(GameState state)
        {
            if (isTie)
            {
                // TODO: Handle tie.
                return false;
            }

            if (loser is Sub)
            {
                // Undo the sub cleanup
                if (loser.GetSpecialistManager().GetSpecialistCount() > 0)
                {
                    loser.GetSpecialistManager().uncaptureAll();
                    ((Sub) loser).IsCaptured = false;
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
                loser.SetOwner(losingPlayer);
                loser.SetDrillerCount(initialLoserDrillerCount);
                
                // Put the winner's specialists back
                winner.GetSpecialistManager().AddSpecialists(loser.GetSpecialistManager().GetPlayerSpecialists(winner.GetOwner()));
                // Uncapture the losing player's specialists
                loser.GetSpecialistManager().uncaptureAll();
                
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