using System;
using System.Numerics;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Components
{
    public class PositionManager : EntityComponent
    {
        private IEntity destination;
        
        // Values set on initial launch and that get updated any time the position/destination/etc. is checked.
        private RftVector initialLocation;
        private RftVector finalLocation;

        private GameTick startTime;
        private SpeedManager speedManager;
        
        // TODO: Add a non-linear position manager
        public PositionManager(
            IEntity parent,
            RftVector initialLocation,
            IEntity destination,
            GameTick startTime
        ) : base(parent) {
            this.destination = destination;
            this.initialLocation = initialLocation;
            speedManager = parent.GetComponent<SpeedManager>();
            this.startTime = startTime;
            finalLocation = GetExpectedDestination(startTime);
        }

        /// <summary>
        /// Gets the rotation of the sub
        /// </summary>
        /// <returns>Gets the angle of the sub's path</returns>
        public double GetRotationRadians(GameTick currentTick)
        {
            RftVector direction = GetDirection(currentTick);
            return Math.Atan2(direction.Y, direction.X);
        }

        public GameTick GetSpawnTick()
        {
            return startTime;
        }

        public RftVector GetExpectedDestination(GameTick currentTick)
        {
            // If this is not moving, or there is no destination, return the current position.
            if (Math.Abs(speedManager.GetSpeed()) < 0.001 | destination == null)
            {
                return initialLocation;
            }
            
            // If the target is not moving, interpolate the current position from the destination.
            // Linear interpolation
            if (destination.GetComponent<SpeedManager>().GetSpeed() < 0.001)
            {
                // If the target is not moving, interpolate the current position from the destination.
                // Linear interpolation
                return destination.GetComponent<PositionManager>().GetPositionAt(startTime);
            }
            else
            {
                // If the target is moving....
                // Get the target's interception point using the initial position & the speed.
                var interceptionPoint = destination.GetComponent<PositionManager>().GetInterceptionPosition(initialLocation, speedManager.GetSpeed(), currentTick);
                return interceptionPoint;
            }
        }

        public SpeedManager GetSpeedManager()
        {
            return this.speedManager;
        }

        /// <summary>
        /// ONLY USE THIS IF YOU ARE GETTING THE POSITION OF SOMETHING THAT DOES NOT MOVE.
        /// </summary>
        /// <returns></returns>
        public RftVector GetStaticPosition()
        {
            if (speedManager.GetSpeed() >= 0.001)
            {
                throw new MovingObjectException("ERROR! Attempting to get the position of a moving object without specifying the tick number.");
            }
            return initialLocation;
        }

        public RftVector GetPositionAt(GameTick currentTick)
        {
            // If this is not moving, or there is no destination, return the current position.
            if (Math.Abs(speedManager.GetSpeed()) < 0.001 | destination == null)
            {
                return initialLocation;
            }
            
            if (destination.GetComponent<SpeedManager>().GetSpeed() < 0.001)
            {
                // If the target is not moving, interpolate the current position from the destination.
                // Linear interpolation
                return InterpolateCurrentPosition(currentTick);
            }
            else
            {
                // If the target is moving....
                // Get the target's interception point using the initial position & the speed.
                var interceptionPoint = destination.GetComponent<PositionManager>().GetInterceptionPosition(initialLocation, speedManager.GetSpeed(), currentTick);
                return interceptionPoint;
            }
        }

        /// <summary>
        /// Get the combat location if this object is targeted by another
        /// </summary>
        /// <param name="targetFrom">The location that this sub is being targeted from</param>
        /// <param name="chaserSpeed">The speed this is being targeted by</param>
        /// <returns>The location of combat</returns>
		public RftVector GetInterceptionPosition(RftVector targetFrom, float chaserSpeed, GameTick currentTick)
        {
            // https://stackoverflow.com/questions/37250215/intersection-of-two-moving-objects-with-latitude-longitude-coordinates
            
            var distanceBetweenTargets = (initialLocation - targetFrom).Magnitude();
            var directionBetweenTargets = Math.Atan2(initialLocation.Y - targetFrom.Y, initialLocation.X - targetFrom.X);
            var alpha = Math.PI + directionBetweenTargets - Math.Atan2(GetDirection(currentTick).Y, GetDirection(currentTick).X);

            if (Math.Abs(chaserSpeed - speedManager.GetSpeed()) < 0.001)
            {
                if (Math.Cos(alpha) < 0)
                {
                    return null;
                }
                var radians = (directionBetweenTargets + alpha) % (Math.PI * 2);
                return new RftVector((float)(Math.Cos(radians) * distanceBetweenTargets), (float)(Math.Sin(radians) * distanceBetweenTargets));
            }

            // Solve with quadratic formula
            var a = Math.Pow(chaserSpeed, 2) - Math.Pow(this.speedManager.GetSpeed(), 2);
            var b = 2 * distanceBetweenTargets * this.speedManager.GetSpeed() * Math.Cos(alpha);
            var c = -Math.Pow(distanceBetweenTargets, 2);

            var discriminant = Math.Pow(b, 2) - (4 * a * c);
            if (discriminant < 0)
                return null;

            var time = (Math.Sqrt(discriminant) - b) / (2 * a);
            var x = initialLocation.X +
                    (this.speedManager.GetSpeed() * time) * Math.Cos(Math.Atan2(GetDirection(currentTick).Y, GetDirection(currentTick).X));
            var y = initialLocation.Y +
                    (this.speedManager.GetSpeed() * time) * Math.Sin(Math.Atan2(GetDirection(currentTick).Y, GetDirection(currentTick).X));
            
            return new RftVector((float)x, (float)y);
        }

        private RftVector InterpolateCurrentPosition(GameTick currentTick)
        {
            // Determine number of ticks after launch:
            int elapsedTicks = currentTick - startTime;

            // Determine direction vector
            Vector2 direction = (finalLocation - initialLocation).Normalize();

            if(elapsedTicks > 0)
            {
                return initialLocation + new RftVector(RftVector.Map, (direction * (elapsedTicks * speedManager.GetSpeed())));
            }
            return initialLocation;
        }

        public RftVector GetDirection(GameTick currentTick)
        {
            if (destination == null)
            {
                return new RftVector(0, 0);
            }
            return (destination.GetComponent<PositionManager>().GetPositionAt(GetExpectedArrival(currentTick)) - initialLocation);
        }

        public GameTick GetExpectedArrival(GameTick currentTick)
        {
            if (destination == null || speedManager.GetSpeed() < 0.001)
            {
                return currentTick;
            }
            
            if (destination.GetComponent<SpeedManager>().GetSpeed() > 0.001)
            {
                // If target is moving....
                // TODO?
                return startTime;
            }
            else
            {
                RftVector direction = GetDirection(currentTick);
                int ticksToArrive = (int) Math.Floor(direction.Magnitude() / speedManager.GetSpeed());
                return startTime.Advance(ticksToArrive);
            }
        }

        public IEntity GetDestination()
        {
            return destination;
        }
    }

    class MovingObjectException : Exception
    {
        public MovingObjectException(string message) : base(message)
        {
            
        }
    }
}