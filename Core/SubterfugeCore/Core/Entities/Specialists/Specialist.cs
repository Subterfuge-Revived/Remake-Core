using System;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists
{
    /// <summary>
    /// Base class for a specialist.
    /// </summary>
    public abstract class Specialist
    {
        /// <summary>
        /// The name of the specialist
        /// </summary>
        /// TODO: Generate the id from a known seed.
        private readonly string _specialistId = Guid.NewGuid().ToString();
        
        /// <summary>
        /// The player who owns the specialist
        /// </summary>
        protected Player _owner;

        /// <summary>
        /// Is the specialist captured by another player?
        /// </summary>
        protected bool _isCaptured = false;

        /// <summary>
        /// Determines priority in combat
        /// </summary>
        protected Priority Priority;

        /// <summary>
        /// Determines the level of the effects applied
        /// </summary>
        protected int _level = 1;

        /// <summary>
        /// Abstract constructor for a specialist. All inherited specialist classes require implementing this.
        /// </summary>
        /// <param name="name">The name of the specialist</param>
        /// <param name="priority">The specialist priority</param>
        /// <param name="owner">The player that owns the specialist</param>
        public Specialist(Player owner, Priority priority)
        {
            Priority = priority;
            _owner = owner;
        }

        /// <summary>
        /// Returns the specialist's priority.
        /// </summary>
        /// <returns>The combat priority of the specialist</returns>
        public Priority GetPriority()
        {
            return Priority;
        }

        /// <summary>
        /// Returns the specialist id.
        /// </summary>
        /// <returns>The specialist's id</returns>
        public string GetId()
        {
            return _specialistId;
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
        /// <param name="isCaptured">Sets the specialist captured state</param>
        public void SetCaptured(bool isCaptured)
        {
            this._isCaptured = isCaptured;
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
        
        public abstract void ArriveAt(IEntity entity);
        public abstract void LeaveLocation(IEntity entity);
        public abstract SpecialistIds GetSpecialistId();
    }
}