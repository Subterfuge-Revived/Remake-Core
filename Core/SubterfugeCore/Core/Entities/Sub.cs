using System;
using System.Numerics;
using SubterfugeCore.Core.Entities.Base;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities
{
    /// <summary>
    /// An instance of a Sub
    /// </summary>
    public class Sub : GameObject, ICombatable, IVision
    {
        /// <summary>
        /// Unique identifier for each sub
        /// </summary>
        private int _id;
        
        /// <summary>
        /// How many drillers are on the sub
        /// </summary>
        private int _drillerCount;
        
        /// <summary>
        /// Where the sub is travelling from
        /// </summary>
        private ILaunchable _source;
        
        /// <summary>
        /// Where the sub is travelling to
        /// </summary>
        private ITargetable _destination;
        
        /// <summary>
        /// The time the sub launched from its source
        /// </summary>
        private GameTick _launchTime;
        
        /// <summary>
        /// The sub's speed
        /// </summary>
        private float _speed = 1f;
        
        /// <summary>
        /// The owner of the sub
        /// </summary>
        private Player _owner;
        
        /// <summary>
        /// Specialist manager for the sub
        /// </summary>
        private SpecialistManager _specialistManager;
        
        /// <summary>
        /// If the sub is captured.
        /// </summary>
        public bool IsCaptured { get; set; }

        /// <summary>
        /// Sub constructor
        /// </summary>
        /// <param name="source">The initial location of the sub</param>
        /// <param name="destination">The destination of the sub</param>
        /// <param name="launchTime">The time of launch</param>
        /// <param name="drillerCount">The amount of drillers to launch</param>
        /// <param name="owner">The owner</param>
        public Sub(ILaunchable source, ITargetable destination, GameTick launchTime, int drillerCount, Player owner) : base()
        {
            this._id = IdGenerator.GetNextId();
            this._source = source;
            this._destination = destination;
            this._launchTime = launchTime;
            this._drillerCount = drillerCount;
            this.Position = source.GetCurrentPosition();
            this._owner = owner;
            this._specialistManager = new SpecialistManager(3);
        }

        /// <summary>
        /// Gets the specialist manager of the sub
        /// </summary>
        /// <returns>The specialist manager</returns>
        public SpecialistManager GetSpecialistManager()
        {
            return this._specialistManager;
        }

        /// <summary>
        /// Returns the sub's owner
        /// </summary>
        /// <returns>The sub's owner</returns>
        public Player GetOwner()
        {
            return this._owner;
        }

        /// <summary>
        /// Gets the estimated arrival time based on known information
        /// </summary>
        /// <returns>The estimated arrival time</returns>
        public GameTick GetExpectedArrival()
        {
            GameTick baseTick;
            RftVector direction;
            int ticksToArrive;
            if(Game.TimeMachine.GetState().GetCurrentTick() < this._launchTime)
            {
                baseTick = this._launchTime;
                // Determine direction vector
                direction = (this._destination.GetInterceptionPosition(this.Position, this.GetSpeed()) - this._source.GetCurrentPosition());
                // Determine the number of ticks to arrive
                ticksToArrive = (int)Math.Floor(direction.Magnitude() / this.GetSpeed());
            } else
            {
                baseTick = Game.TimeMachine.GetState().GetCurrentTick();
                // Determine direction vector
                direction = (this._destination.GetInterceptionPosition(this.Position, this.GetSpeed()) - this.GetCurrentPosition());
                // Determine the number of ticks to arrive
                ticksToArrive = (int)Math.Floor(direction.Magnitude() / this.GetSpeed());
            }
            return baseTick.Advance(ticksToArrive);
        }

        /// <summary>
        /// Get the number of drillers
        /// </summary>
        /// <returns>The number of drillers</returns>
        public int GetDrillerCount()
        {
            return this._drillerCount;
        }

        /// <summary>
        /// Gets the position of the current sub
        /// </summary>
        /// <returns>The sub's position</returns>
        public override RftVector GetPosition()
        {
            // Determine number of ticks after launch:
            int elapsedTicks = Game.TimeMachine.GetState().GetCurrentTick() - this._launchTime;

            // Determine direction vector
            Vector2 direction = (this._destination.GetInterceptionPosition(this.Position, this.GetSpeed()) - this._source.GetCurrentPosition()).Normalize();

            if(elapsedTicks > 0)
            {
                this.Position = this._source.GetCurrentPosition() + new RftVector(RftVector.Map, (direction * (elapsedTicks * this.GetSpeed())));
                return this.Position;
            }
            else
            {
                return new RftVector(RftVector.Map);
            }
        }
        
        /// <summary>
        /// Gets the rotation of the sub
        /// </summary>
        /// <returns>Gets the angle of the sub's path</returns>
        public double GetRotation()
        {
            // Determine direction vector
            Vector2 direction = (this._destination.GetInterceptionPosition(this.GetPosition(), this.GetSpeed()) - this._source.GetCurrentPosition()).Normalize();

            double extraRotation = 0;
            if(direction.X < 0)
            {
                extraRotation = Math.PI;
            }
            return Math.Atan(direction.Y / direction.X) + Math.PI/4.0f + extraRotation;
        }
        
        /// <summary>
        /// Gets the speed of the sub
        /// </summary>
        /// <returns>The sub's speed</returns>
        public float GetSpeed()
        {
            return this._speed;
        }

        /// <summary>
        /// Gets the initial position of the sub
        /// </summary>
        /// <returns>Gets the subs initial position</returns>
        public RftVector GetInitialPosition()
        {
            return this._source.GetCurrentPosition();
        }

        /// <summary>
        /// Gets the position of the sub's destination
        /// </summary>
        /// <returns>The destination of the sub</returns>
        public RftVector GetDestinationLocation()
        {
            return this._destination.GetInterceptionPosition(this.GetPosition(), this.GetSpeed());
        }
        
        /// <summary>
        /// Get the destination
        /// </summary>
        /// <returns>The sub's destination</returns>
        public ITargetable GetDestination()
        {
            return this._destination;
        }

        /// <summary>
        /// Get the tick the sub launched
        /// </summary>
        /// <returns>The tick the sub launched</returns>
        public GameTick GetLaunchTick()
        {
            return this._launchTime;
        }

        /// <summary>
        /// Get the combat location if this object is targeted by another
        /// </summary>
        /// <param name="targetFrom">The location that this sub is being targeted from</param>
        /// <param name="speed">The speed this is being targeted by</param>
        /// <returns>The location of combat</returns>
		public RftVector GetInterceptionPosition(RftVector targetFrom, float speed)
		{
		    if (targetFrom == this.GetPosition())
		        return targetFrom;

		    if (speed == 0)
		        return targetFrom;

		    // Determine target's distance to travel to destination:
		    RftVector targetDestination = this.GetDestinationLocation();

		    // Check if the chaser can get there before it.
		    RftVector chaserDestination = targetDestination - targetFrom;

		    float ticksToDestination = targetDestination.Magnitude()/ this.GetSpeed();
		    float chaserTicksToDestination = chaserDestination.Magnitude() / speed;

		    if (ticksToDestination > chaserTicksToDestination)
		    {
		        // Can intercept.
		        // Determine interception point.

		        int scalar = 1;
		        bool searching = true;
		        while (searching)
		        {
		            Vector2 destination = (this.GetDestinationLocation() - this.GetPosition()).Normalize();

		            RftVector runnerLocation = this.GetPosition() + new RftVector(RftVector.Map, (destination * scalar));
		            Vector2 chaserDirection = (runnerLocation - targetFrom).Normalize();
		            RftVector chaserPosition = targetFrom + new RftVector(RftVector.Map, (chaserDirection * scalar));

		            if((chaserPosition - runnerLocation).Magnitude() < 1)
		            {
		                return chaserPosition;
		            }
		            if(runnerLocation.Magnitude() > this.GetDestinationLocation().Magnitude())
		            {
		                return targetFrom;
		            }
		            scalar++;
		        }
		        return targetFrom;
		    }
		    return targetFrom;
		}
        
        /// <summary>
        /// Set the owner
        /// </summary>
        /// <param name="newOwner">The new owner</param>

        public void SetOwner(Player newOwner)
        {
            this._owner = newOwner;
        }

        /// <summary>
        /// Get the current location
        /// </summary>
        /// <returns>The sub's current location</returns>
        public RftVector GetCurrentPosition()
        {
            return this.GetPosition();
        }

        /// <summary>
        /// The subs direction
        /// </summary>
        /// <returns>The sub's direction vector</returns>
        public Vector2 GetDirection()
        {
            return (this.GetDestinationLocation() - this._source.GetCurrentPosition()).Normalize();
        }

        /// <summary>
        /// Get the launch source
        /// </summary>
        /// <returns>The source of the sub launch</returns>
        public ILaunchable GetSource()
        {
            return this._source;
        }
        
        /// <summary>
        /// Set the driller count
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set</param>
        public void SetDrillerCount(int drillerCount)
        {
            this._drillerCount = drillerCount;
        }

        /// <summary>
        /// Adds drillers to the sub
        /// </summary>
        /// <param name="drillersToAdd">The number of drillers to add</param>
        public void AddDrillers(int drillersToAdd)
        {
            this._drillerCount += drillersToAdd;
        }
        
        /// <summary>
        /// Removes drillers from the sub
        /// </summary>
        /// <param name="drillersToRemove">The number of drillers to remove</param>
        public void RemoveDrillers(int drillersToRemove)
        {
            this._drillerCount -= drillersToRemove;
        }
        
        /// <summary>
        /// If the sub has the specified number of drillers
        /// </summary>
        /// <param name="drillers">The number of drillers to check for</param>
        /// <returns>If the sub has the specified number of drillers</returns>
        public bool HasDrillers(int drillers)
        {
            return this._drillerCount >= drillers;
        }

        /// <summary>
        /// Launches a sub from this object
        /// </summary>
        /// <param name="drillerCount"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public ICombatable LaunchSub(int drillerCount, ITargetable destination)
        {
            // Determine any specialist effects if a specialist left the sub.
            return new Sub(this, destination, Game.TimeMachine.CurrentTick, drillerCount, this._owner);
        }

        /// <summary>
        /// Undoes launching a sub from this object
        /// </summary>
        /// <param name="sub"></param>
        public void UndoLaunch(ICombatable sub)
        {
            return;
        }
        
        /// <summary>
        /// Gets the globally unique indentifier for the Sub.
        /// </summary>
        /// <returns>The Sub's Guid</returns>
        public int GetId()
        {
            return this._id;
        }

        public float getVisionRange()
        {
            return Config.Constants.BASE_OUTPOST_VISION_RADIUS * 0.2f;
        }

        public bool isInVisionRange(IPosition position)
        {
            return Vector2.Distance(this.GetCurrentPosition().ToVector2(), position.GetCurrentPosition().ToVector2()) <= getVisionRange();
        }
    }
}
