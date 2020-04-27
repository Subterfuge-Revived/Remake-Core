using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.Entities
{
    public class ShieldManager
    {
        /// <summary>
        /// The current value of the shields.
        /// </summary>
        private int _shields = 0;

        /// <summary>
        /// If the shields are currently enabled.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The number of ticks required for 1 shield to regenerate.
        /// </summary>
        public float RechargeRate { get; set; } = 1;

        /// <summary>
        /// The maximum capacity of shields that the object can have.
        /// </summary>
        public int ShieldCapacity { get; set; } = 30;

        /// <summary>
        /// The last time the location's shields were altered. This is used to track what the current shield value
        /// should be.
        /// </summary>
        private Stack<ShieldChange> _shieldDeltas = new Stack<ShieldChange>();

        public ShieldManager()
        {
            // Add an initial delta.
            _shieldDeltas.Push(new ShieldChange(Game.TimeMachine.CurrentTick, 0, true));
        }

        /// <summary>
        /// Determines the current shield level
        /// </summary>
        /// <returns>The current shield level</returns>
        public int GetShields()
        {
            // Peek the top to get the last shield delta.
            while (_shieldDeltas.Peek().Tick > Game.TimeMachine.CurrentTick)
            {
                _shieldDeltas.Pop();
            }
            ShieldChange lastDelta = _shieldDeltas.Peek();
            
            // Interpolate shield value based on the last delta.
            int lastShield = lastDelta.ShieldValue;
            GameTick lastDeltaTime = lastDelta.Tick;
            int newShields = lastShield + (int)Math.Round((Game.TimeMachine.CurrentTick - lastDeltaTime) * RechargeRate);
            if (newShields > ShieldCapacity)
            {
                return ShieldCapacity;
            }

            return newShields;
        }

        /// <summary>
        /// Sets the value of the shields at an outpost.
        /// </summary>
        /// <param name="shieldValue"></param>
        public void SetShields(int shieldValue)
        {
            this._shields = shieldValue;
            if (_shields > ShieldCapacity)
            {
                _shields = ShieldCapacity;
            }

            if (_shields < 0)
            {
                _shields = 0;
            }
            _shieldDeltas.Push(new ShieldChange(Game.TimeMachine.CurrentTick, _shields, IsActive));
        }

        /// <summary>
        /// Performs combat on the shields. Adds a "ShieldChange" event for future shield calculations.
        /// </summary>
        /// <param name="shieldsToRemove">The number of enemy drillers in combat.</param>
        /// <returns>If the enemy won the combat.</returns>
        public bool CombatShields(int shieldsToRemove)
        {
            if (IsActive)
            {
                _shields -= shieldsToRemove;
                if (_shields < 0)
                {
                    _shields = 0;
                    _shieldDeltas.Push(new ShieldChange(Game.TimeMachine.CurrentTick, _shields));
                    return true;
                }

                _shieldDeltas.Push(new ShieldChange(Game.TimeMachine.CurrentTick, _shields));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Turn shields off or on
        /// </summary>
        public void ToggleShield()
        {
            IsActive = !IsActive;
        }

        /// <summary>
        /// Determines if the shields are off or on
        /// </summary>
        /// <returns>if the shields are enabled</returns>
        public bool IsShieldActive()
        {
            return IsActive;
        }

        /// <summary>
        /// Get the shield capacity.
        /// </summary>
        /// <returns>the maximum amount of shields possible</returns>
        public int GetShieldCapacity()
        {
            return ShieldCapacity;
        }

        /// <summary>
        /// Sets the shield capacity
        /// </summary>
        /// <param name="capactiy">The capacity of shields</param>
        public void SetShieldCapacity(int capactiy)
        {
            ShieldCapacity = capactiy;
            if (_shields > ShieldCapacity)
            {
                _shields = ShieldCapacity;
            }
            _shieldDeltas.Push(new ShieldChange(Game.TimeMachine.CurrentTick, _shields, ShieldCapacity));
        }
    }
}