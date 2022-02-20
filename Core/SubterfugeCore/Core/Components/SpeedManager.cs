using System;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.EventArgs;

namespace SubterfugeCore.Core.Components
{
    public class SpeedManager : EntityComponent
    {
        private float _speed;
        
        public event EventHandler<OnSpeedChangedEventArgs> OnSpeedChanged;

        public SpeedManager(IEntity parent, float initialSpeed) : base(parent)
        {
            _speed = initialSpeed;
        }

        public float GetSpeed()
        {
            return _speed;
        }

        public void IncreaseSpeed(float increaseBy)
        {
            var previousSpeed = _speed;
            _speed += increaseBy;
            OnSpeedChanged?.Invoke(this, new OnSpeedChangedEventArgs()
            {
                previousSpeed = previousSpeed,
                newSpeed = _speed,
                SpeedManager = this,
            });
        }

        public void DecreaseSpeed(float decreaseBy)
        {
            var previousSpeed = _speed;
            _speed -= decreaseBy;
            OnSpeedChanged?.Invoke(this, new OnSpeedChangedEventArgs()
            {
                previousSpeed = previousSpeed,
                newSpeed = _speed,
                SpeedManager = this,
            });
        }

        public void SetSpeed(float newSpeed)
        {
            var previousSpeed = _speed;
            _speed = newSpeed;
            OnSpeedChanged?.Invoke(this, new OnSpeedChangedEventArgs()
            {
                previousSpeed = previousSpeed,
                newSpeed = newSpeed,
                SpeedManager = this,
            });
        }
    }
}