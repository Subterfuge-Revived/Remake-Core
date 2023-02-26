using System;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class ShieldManager : EntityComponent, IShieldEventPublisher
    {
        /// <summary>
        /// The outposts shields
        /// </summary>
        int _shields;
        
        /// <summary>
        /// If the outpost's shields are active.
        /// </summary>
        bool _shieldActive;
        
        /// <summary>
        /// The maximum number of shields the outpost can have.
        /// </summary>
        int _shieldCapacity;

        
        public event EventHandler<OnShieldEnableEventArgs> OnShieldEnable;
        public event EventHandler<OnShieldDisableEventArgs> OnShieldDisable;
        public event EventHandler<OnShieldCapacityChangeEventArgs> OnShieldCapacityChange;
        public event EventHandler<OnShieldValueChangeEventArgs> OnShieldValueChange;
        
        
        public ShieldManager(
            IEntity parent,
            int shieldCapacity,
            int initialShields = 0
            ) : base(parent)
        {
            if (initialShields > shieldCapacity)
            {
                _shields = shieldCapacity;
                _shieldActive = true;
                _shieldCapacity = shieldCapacity;
            }
            else
            {
                _shields = initialShields;
                _shieldActive = true;
                _shieldCapacity = shieldCapacity;
            }
        }
        
        public int GetShields()
        {
            return this._shields;
        }

        public void SetShields(int shieldValue)
        {
            var previousValue = _shields;
            this._shields = Math.Max(0, Math.Min(shieldValue, this._shieldCapacity));
            
            OnShieldValueChange?.Invoke(this, new OnShieldValueChangeEventArgs()
            {
                ShieldDelta = this._shields - previousValue,
                ShieldManager = this,
            });
        }

        public void AlterShieldCapacity(int delta)
        {
            _shieldCapacity = Math.Max(0, _shieldCapacity + delta);
        }

        public int RemoveShields(int shieldsToRemove)
        {
            var previousValue = _shields;
            this._shields = Math.Max(0, this._shields - shieldsToRemove);
            
            OnShieldValueChange?.Invoke(this, new OnShieldValueChangeEventArgs()
            {
                ShieldDelta = this._shields - previousValue,
                ShieldManager = this,
            });

            return this._shields - previousValue;
        }

        public void ToggleShield()
        {
            this._shieldActive = !this._shieldActive;
            if (_shieldActive)
            {
                OnShieldEnable?.Invoke(this, new OnShieldEnableEventArgs()
                {
                    ShieldManager = this
                });
            }
            else
            {
                OnShieldDisable?.Invoke(this, new OnShieldDisableEventArgs()
                {
                    ShieldManager = this
                });
            }
        }

        public bool IsShieldActive()
        {
            return this._shieldActive;
        }

        public int AddShield(int shields)
        {
            var previousValue = _shields;
            this._shields = Math.Min(this._shieldCapacity, this._shields + shields);
            
            OnShieldValueChange?.Invoke(this, new OnShieldValueChangeEventArgs()
            {
                ShieldDelta = this._shields - previousValue,
                ShieldManager = this,
            });
            
            return this._shields - previousValue;
        }

        public int GetShieldCapacity()
        {
            return this._shieldCapacity;
        }

        public void SetShieldCapacity(int capactiy)
        {
            var previousCapacity = _shieldCapacity;
            this._shieldCapacity = capactiy;

            this._shields = Math.Min(_shields, _shieldCapacity);
            
            OnShieldCapacityChange?.Invoke(this, new OnShieldCapacityChangeEventArgs()
            {
                CapacityDelta = this._shieldCapacity - previousCapacity,
                ShieldManager = this,
            });
        }
    }
}