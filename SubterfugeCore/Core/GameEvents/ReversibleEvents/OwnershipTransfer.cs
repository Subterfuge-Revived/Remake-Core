using SubterfugeCore.Core.GameEvents.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Transfers ownership of an outpost after combat
    /// </summary>
    public class OwnershipTransfer : IReversible
    {
        Outpost _outpost;
        Sub _sub;
        bool _eventSuccess = false;
        bool _wasOwnershipTransferred = false;
        bool _wasSubCaptured = false;

        Player _originalOutpostOwner;
        int _originalDrillerCount;

        /// <summary>
        /// Constructor to transfer ownership
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public OwnershipTransfer(ICombatable combatant1, ICombatable combatant2)
        {
            this._sub = (Sub)(combatant1 is Sub ? combatant1 : combatant2);
            this._outpost = (Outpost)(combatant1 is Outpost ? combatant1 : combatant2);
        }

        /// <summary>
        /// Undoes the ownership transfer
        /// </summary>
        /// <returns>If the event was undone</returns>
        public bool BackwardAction()
        {
            if (_eventSuccess)
            {
                // Tie or sub won.
                if (_wasOwnershipTransferred)
                {
                    this.UnTransferOwnership();
                }
                if (_wasSubCaptured)
                {
                    this.UncaptureSub();
                }
            }
            return _eventSuccess;
        }

        /// <summary>
        /// Determines if the event was successful.
        /// </summary>
        /// <returns>If the event is successful</returns>
        public bool WasEventSuccessful()
        {
            return this._eventSuccess;
        }

        /// <summary>
        /// Switches the control of the outpost to the owner of the sub
        /// </summary>
        public void TransferOwnership()
        {
            _outpost.SetOwner(_sub.GetOwner());
            _outpost.SetDrillerCount(_sub.GetDrillerCount());
            Game.TimeMachine.GetState().RemoveSub(_sub);
            _wasOwnershipTransferred = true;
        }

        /// <summary>
        /// Switches control of the outpost back to the original owner.
        /// </summary>
        public void UnTransferOwnership()
        {

            _outpost.SetOwner(_originalOutpostOwner);
            _outpost.SetDrillerCount(_originalDrillerCount);
            Game.TimeMachine.GetState().AddSub(_sub);
        }
        
        /// <summary>
        /// Removes a sub from the game and captures it
        /// </summary>

        public void CaptureSub()
        {
            // capture sub.
            Game.TimeMachine.GetState().RemoveSub(_sub);
            _wasSubCaptured = true;
        }
        
        /// <summary>
        /// Uncaptures a sub
        /// </summary>

        public void UncaptureSub()
        {
            // capture sub.
            Game.TimeMachine.GetState().AddSub(_sub);
        }

        /// <summary>
        /// Transfers ownership between outpost or specialists.
        /// </summary>
        /// <returns>If the event was successful</returns>
        public bool ForwardAction()
        {
            _originalOutpostOwner = _outpost.GetOwner();
            _originalDrillerCount = _outpost.GetDrillerCount();

            // Tie or sub won.
            if (_outpost.GetDrillerCount() < 0)
            {
                this.TransferOwnership();
            } else
            {
                if (_outpost.GetDrillerCount() == 0)
                {
                    // Was a tie.
                    if (_sub.GetSpecialistManager().GetSpecialistCount() > _outpost.GetSpecialistManager().GetSpecialistCount())
                    {
                        this.TransferOwnership();
                    } else
                    {
                        this.CaptureSub();
                    }
                }
            }

            this._eventSuccess = true;
            return true;
        }
    }
}
