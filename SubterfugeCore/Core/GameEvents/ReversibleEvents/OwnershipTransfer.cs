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
    public class OwnershipTransfer : IReversible
    {
        Outpost outpost;
        Sub sub;
        bool eventSuccess = false;
        bool wasOwnershipTransferred = false;
        bool wasSubCaptured = false;

        Player originalOutpostOwner;
        int originalDrillerCount;

        public OwnershipTransfer(ICombatable combatant1, ICombatable combatant2)
        {
            this.sub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            this.outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

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

        public void transferOwnership()
        {
            outpost.setOwner(sub.getOwner());
            outpost.setDrillerCount(sub.getDrillerCount());
            GameServer.timeMachine.getState().removeSub(sub);
            wasOwnershipTransferred = true;
        }

        public void unTransferOwnership()
        {

            outpost.setOwner(originalOutpostOwner);
            outpost.setDrillerCount(originalDrillerCount);
            GameServer.timeMachine.getState().addSub(sub);
        }

        public void captureSub()
        {
            // capture sub.
            GameServer.timeMachine.getState().removeSub(sub);
            wasSubCaptured = true;
        }

        public void uncaptureSub()
        {
            // capture sub.
            GameServer.timeMachine.getState().addSub(sub);
        }

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
