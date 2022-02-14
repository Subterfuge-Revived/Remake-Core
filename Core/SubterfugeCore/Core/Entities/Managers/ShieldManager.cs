using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities
{
    public class ShieldManager
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
        
        public ShieldManager(int shieldCapacity)
        {
            _shields = 0;
            _shieldActive = true;
            _shieldCapacity = shieldCapacity;
        }
        
        public int GetShields()
        {
            return this._shields;
        }

        public void SetShields(int shieldValue)
        {
            if(shieldValue > this._shieldCapacity)
            {
                this._shields = this._shieldCapacity;
            } else
            {
                this._shields = shieldValue;
            }
        }

        public void RemoveShields(int shieldsToRemove)
        {
            if (this._shields - shieldsToRemove < 0)
            {
                this._shields = 0;
            }
            else
            {
                this._shields -= shieldsToRemove;
            }
        }

        public void ToggleShield()
        {
            this._shieldActive = !this._shieldActive;
        }

        public bool IsShieldActive()
        {
            return this._shieldActive;
        }

        public void AddShield(int shields)
        {
            if(this._shields + shields > this._shieldCapacity)
            {
                this._shields = this._shieldCapacity;
            } else
            {
                this._shields += shields;
            }
        }

        public int GetShieldCapacity()
        {
            return this._shieldCapacity;
        }

        public void SetShieldCapacity(int capactiy)
        {
            this._shieldCapacity = capactiy;
        }
    }
}