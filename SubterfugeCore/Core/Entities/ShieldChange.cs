using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.Entities
{
    public class ShieldChange
    {
        public bool IsActive { get; } = true;
        public GameTick Tick { get; }
        public int ShieldValue { get; }
        
        public int ShieldCapacity { get; }

        public ShieldChange(GameTick time, int value)
        {
            Tick = time;
            ShieldValue = value;
        }

        public ShieldChange(GameTick time, int value, bool isActive)
        {
            IsActive = isActive;
            Tick = time;
            ShieldValue = value;
        }

        public ShieldChange(GameTick time, int value, int shieldCapacity)
        {
            Tick = time;
            ShieldValue = value;
            ShieldCapacity = shieldCapacity;
        }
        
    }
}