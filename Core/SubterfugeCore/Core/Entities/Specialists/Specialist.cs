using System;
using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    /// <summary>
    /// Base class for a specialist.
    /// </summary>
    public abstract class Specialist
    {
        /// <summary>
        /// The player who owns the specialist
        /// </summary>
        protected Player _owner;

        /// <summary>
        /// Is the specialist captured by another player?
        /// </summary>
        protected bool _isCaptured = false;

        /// <summary>
        /// Determines the level of the effects applied
        /// </summary>
        protected int _level = 1;
        
        public bool IsHero { get; }

        /// <summary>
        /// Abstract constructor for a specialist. All inherited specialist classes require implementing this.
        /// </summary>
        /// <param name="name">The name of the specialist</param>
        /// <param name="priority">The specialist priority</param>
        /// <param name="owner">The player that owns the specialist</param>
        public Specialist(
            Player owner,
            bool isHero
        ) {
            _owner = owner;
            IsHero = isHero;
        }

        /// <summary>
        /// Gets the owner of the specialist.
        /// </summary>
        /// <returns>The specialist's owner</returns>
        public Player GetOwner()
        {
            return this._owner;
        }

        /// <summary>
        /// Sets the owner of the specialist
        /// </summary>
        /// <param name="newOwner">The new owner of the specialist</param>
        public void SetOwner(Player newOwner)
        {
            this._owner = newOwner;
        }

        /// <summary>
        /// Captures the specialist
        /// </summary>
        /// <param name="timeMachine"></param>
        /// <param name="captureLocation"></param>
        public void SetCaptured(
            bool isCaptured,
            TimeMachine timeMachine,
            IEntity captureLocation
        ) {
            this._isCaptured = isCaptured;
            OnCapture(isCaptured, captureLocation, timeMachine);
        }

        /// <summary>
        /// Check if the specialist is captured
        /// </summary>
        /// <returns>If the specialist if captured</returns>
        public bool IsCaptured()
        {
            return _isCaptured;
        }

        public int GetLevel()
        {
            return _level;
        }

        public void Promote()
        {
            _level++;
        }

        public void UndoPromote()
        {
            _level--;
        }

        public abstract void ArriveAtLocation(IEntity entity, TimeMachine timeMachine);
        public abstract void LeaveLocation(IEntity entity, TimeMachine timeMachine);
        public abstract void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine);

        public abstract SpecialistTypeId GetSpecialistId();

        public abstract String GetDescription();
    }
}