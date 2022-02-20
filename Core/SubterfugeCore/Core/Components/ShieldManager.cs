using System;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.EventArgs;

namespace SubterfugeCore.Core.Entities
{
    public class ShieldManager : EntityComponent
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

        
        // Shield Toggle Events:
        public event EventHandler<OnShieldEnableEventArgs> OnShieldEnable;
        public event EventHandler<OnShieldDisableEventArgs> OnShieldDisable;
        
        // Shield Capacity Events:
        public event EventHandler<OnShieldCapacityChangeEventArgs> OnShieldCapacityChange;
        
        // Shield Modification Events:
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
            if(shieldValue > this._shieldCapacity)
            {
                this._shields = this._shieldCapacity;
            } else
            {
                this._shields = shieldValue;
            }
            OnShieldValueChange?.Invoke(this, new OnShieldValueChangeEventArgs()
            {
                previousValue = previousValue,
                ShieldManager = this,
            });
        }

        public void RemoveShields(int shieldsToRemove)
        {
            var previousValue = _shields;
            if (this._shields - shieldsToRemove < 0)
            {
                this._shields = 0;
            }
            else
            {
                this._shields -= shieldsToRemove;
            }
            OnShieldValueChange?.Invoke(this, new OnShieldValueChangeEventArgs()
            {
                previousValue = previousValue,
                ShieldManager = this,
            });
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

        public void AddShield(int shields)
        {
            var previousValue = _shields;
            if(this._shields + shields > this._shieldCapacity)
            {
                this._shields = this._shieldCapacity;
            } else
            {
                this._shields += shields;
            }
            OnShieldValueChange?.Invoke(this, new OnShieldValueChangeEventArgs()
            {
                previousValue = previousValue,
                ShieldManager = this,
            });
        }

        public int GetShieldCapacity()
        {
            return this._shieldCapacity;
        }

        public void SetShieldCapacity(int capactiy)
        {
            var previousCapacity = _shieldCapacity;
            this._shieldCapacity = capactiy;
            if (_shields > _shieldCapacity)
            {
                this._shields = _shieldCapacity;
            }
            
            OnShieldCapacityChange?.Invoke(this, new OnShieldCapacityChangeEventArgs()
            {
                previousCapacity = previousCapacity,
                ShieldManager = this,
            });
        }
    }
}