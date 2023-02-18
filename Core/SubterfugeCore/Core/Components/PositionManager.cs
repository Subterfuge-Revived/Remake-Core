using System;
using System.Numerics;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Components
{
    public class PositionManager : EntityComponent
    {
        private readonly IEntity _destination;
        private readonly RftVector _initialLocation;
        private readonly GameTick _startTime;
        private readonly SpeedManager _speedManager;
        
        public PositionManager(
            IEntity parent,
            RftVector initialLocation,
            IEntity destination,
            GameTick startTime
        ) : base(parent) {
            this._destination = destination;
            this._initialLocation = initialLocation;
            _speedManager = parent.GetComponent<SpeedManager>();
            this._startTime = startTime;
        }

        /// <summary>
        /// Gets the rotation of the sub
        /// </summary>
        /// <returns>Gets the angle of the sub's path</returns>
        public double GetRotationRadians()
        {
            RftVector direction = GetDirection();
            return Math.Atan2(direction.Y, direction.X);
        }

        public GameTick GetSpawnTick()
        {
            return _startTime;
        }

        public RftVector GetExpectedDestination()
        {
            // If there is no destination, return the current position.
            if (_destination == null)
            {
                return _initialLocation;
            }
            
            // If the target is not moving, interpolate the current position from the destination.
            // Linear interpolation
            if (Math.Abs(_destination.GetComponent<SpeedManager>().GetSpeed()) < 0.001)
            {
                // If the target is not moving, interpolate the current position from the destination.
                // Linear interpolation
                return _destination.GetComponent<PositionManager>().GetPositionAt(_startTime);
            }
            else
            {
                // If this is not moving, but the target is, show no destination...
                if (Math.Abs(_speedManager.GetSpeed()) < 0.001)
                {
                    return null;
                }
                // If the target is moving....
                // Get the target's interception point using the initial position & the speed.
                var interceptionPoint = _destination.GetComponent<PositionManager>()
                    .GetInterceptionPosition(_initialLocation, _speedManager.GetSpeed());
                return interceptionPoint;
            }
        }

        public RftVector GetPositionAt(GameTick currentTick)
        {
            // If this is not moving, or there is no destination, return the current position.
            if (Math.Abs(_speedManager.GetSpeed()) < 0.001 | _destination == null)
            {
                return _initialLocation;
            }
            
            if (_destination.GetComponent<SpeedManager>().GetSpeed() < 0.001)
            {
                // If the target is not moving, interpolate the current position from the destination.
                // Linear interpolation
                return InterpolateCurrentPosition(currentTick);
            }
            else
            {
                // If the target is moving....
                // Get the target's interception point using the initial position & the speed.
                var interceptionPoint = _destination.GetComponent<PositionManager>().GetInterceptionPosition(_initialLocation, _speedManager.GetSpeed());
                return interceptionPoint;
            }
        }

        /// <summary>
        /// Get the combat location if this object is targeted by another
        /// </summary>
        /// <param name="targetFrom">The location that this sub is being targeted from</param>
        /// <param name="chaserSpeed">The speed this is being targeted by</param>
        /// <returns>The location of combat</returns>
		public RftVector GetInterceptionPosition(RftVector targetFrom, float chaserSpeed)
        {
            // https://stackoverflow.com/questions/37250215/intersection-of-two-moving-objects-with-latitude-longitude-coordinates
            
            var distanceBetweenTargets = (_initialLocation - targetFrom).Magnitude();
            var directionBetweenTargets = Math.Atan2(_initialLocation.Y - targetFrom.Y, _initialLocation.X - targetFrom.X);
            var alpha = Math.PI + directionBetweenTargets - Math.Atan2(GetDirection().Y, GetDirection().X);

            if (Math.Abs(chaserSpeed - _speedManager.GetSpeed()) < 0.001)
            {
                if (Math.Cos(alpha) < 0)
                {
                    return null;
                }
                // This determines what direction the chaser needs to go.
                // Create a Normalized vector from this.
                var radians = (directionBetweenTargets + alpha) % (Math.PI * 2);
                // With isosceles triangle, determine the other two angles:
                var otherAngles = ((Math.PI * 2) - radians) / 2.0;
                var distanceToTravelBeforeInterception = (float)(Math.Abs(Math.Cos(otherAngles) * distanceBetweenTargets));
                var timeToReach = distanceToTravelBeforeInterception / _speedManager.GetSpeed();
                var dir = GetDirection().Normalize();
                return new RftVector(_initialLocation.X + (dir.X * timeToReach), _initialLocation.Y + (dir.Y * timeToReach));
            }

            // Solve with quadratic formula
            var a = Math.Pow(chaserSpeed, 2) - Math.Pow(this._speedManager.GetSpeed(), 2);
            var b = 2 * distanceBetweenTargets * this._speedManager.GetSpeed() * Math.Cos(alpha);
            var c = -Math.Pow(distanceBetweenTargets, 2);

            var discriminant = Math.Pow(b, 2) - (4 * a * c);
            if (discriminant < 0)
                return null;

            var time = (Math.Sqrt(discriminant) - b) / (2 * a);
            var x = _initialLocation.X +
                    (this._speedManager.GetSpeed() * time) * Math.Cos(Math.Atan2(GetDirection().Y, GetDirection().X));
            var y = _initialLocation.Y +
                    (this._speedManager.GetSpeed() * time) * Math.Sin(Math.Atan2(GetDirection().Y, GetDirection().X));
            
            return new RftVector((float)x, (float)y);
        }

        private RftVector InterpolateCurrentPosition(GameTick currentTick)
        {
            // Determine number of ticks after launch:
            int elapsedTicks = currentTick - _startTime;

            // Determine direction vector
            Vector2 direction = (GetExpectedDestination() - _initialLocation).Normalize();

            if(elapsedTicks > 0)
            {
                return _initialLocation + new RftVector(RftVector.Map, (direction * (elapsedTicks * _speedManager.GetSpeed())));
            }
            return _initialLocation;
        }

        public RftVector GetDirection()
        {
            if (_destination == null)
            {
                return new RftVector(0, 0);
            }

            return (GetExpectedDestination() - _initialLocation);
        }

        public GameTick GetExpectedArrival()
        {
            if (_destination == null || _speedManager.GetSpeed() < 0.001)
            {
                return null;
            }
            RftVector direction = GetDirection();
            int ticksToArrive = (int) Math.Floor(direction.Magnitude() / _speedManager.GetSpeed());
            return _startTime.Advance(ticksToArrive);
        }

        public IEntity GetDestination()
        {
            return _destination;
        }
    }
}