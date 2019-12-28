using SubterfugeCore.Components;
using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Players;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Transfers ownership of an outpost after combat
    /// </summary>
    public class OwnershipTransfer : IReversible
    {
        Outpost outpost;
        Sub sub;
        bool eventSuccess = false;
        bool wasOwnershipTransferred = false;
        bool wasSubCaptured = false;

        Player originalOutpostOwner;
        int originalDrillerCount;

        /// <summary>
        /// Constructor to transfer ownership
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public OwnershipTransfer(ICombatable combatant1, ICombatable combatant2)
        {
            this.sub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            this.outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

        /// <summary>
        /// Undoes the ownership transfer
        /// </summary>
        /// <returns>If the event was undone</returns>
        public bool backwardAction()
        {
            if (eventSuccess)
            {
                // Tie or sub won.
                if (wasOwnershipTransferred)
                {
                    this.unTransferOwnership();
                }
                if (wasSubCaptured)
                {
                    this.uncaptureSub();
                }
            }
            return eventSuccess;
        }

        /// <summary>
        /// Switches the control of the outpost to the owner of the sub
        /// </summary>
        public void transferOwnership()
        {
            outpost.setOwner(sub.getOwner());
            outpost.setDrillerCount(sub.getDrillerCount());
            GameServer.timeMachine.getState().removeSub(sub);
            wasOwnershipTransferred = true;
        }

        /// <summary>
        /// Switches control of the outpost back to the original owner.
        /// </summary>
        public void unTransferOwnership()
        {

            outpost.setOwner(originalOutpostOwner);
            outpost.setDrillerCount(originalDrillerCount);
            GameServer.timeMachine.getState().addSub(sub);
        }
        
        /// <summary>
        /// Removes a sub from the game and captures it
        /// </summary>

        public void captureSub()
        {
            // capture sub.
            GameServer.timeMachine.getState().removeSub(sub);
            wasSubCaptured = true;
        }
        
        /// <summary>
        /// Uncaptures a sub
        /// </summary>

        public void uncaptureSub()
        {
            // capture sub.
            GameServer.timeMachine.getState().addSub(sub);
        }

        /// <summary>
        /// Transfers ownership between outpost or specialists.
        /// </summary>
        /// <returns>If the event was successful</returns>
        public bool forwardAction()
        {
            originalOutpostOwner = outpost.getOwner();
            originalDrillerCount = outpost.getDrillerCount();

            // Tie or sub won.
            if (outpost.getDrillerCount() < 0)
            {
                this.transferOwnership();
            } else
            {
                if (outpost.getDrillerCount() == 0)
                {
                    // Was a tie.
                    if (sub.getSpecialistManager().getSpecialistCount() > outpost.getSpecialistManager().getSpecialistCount())
                    {
                        this.transferOwnership();
                    } else
                    {
                        this.captureSub();
                    }
                }
            }

            this.eventSuccess = true;
            return true;
        }
    }
}
