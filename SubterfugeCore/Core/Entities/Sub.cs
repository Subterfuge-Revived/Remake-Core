using System;
using System.Numerics;
using SubterfugeCore.Core.Entities.Base;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.Entities
{
    /// <summary>
    /// An instance of a Sub
    /// </summary>
    public class Sub : GameObject, ITargetable, IDrillerCarrier, ISpecialistCarrier, ICombatable
    {
        private Guid guid;
        private int drillerCount;
        private ILaunchable source;
        private ITargetable destination;
        private GameTick launchTime;
        private float speed = 1f;
        private Player owner;
        private SpecialistManager specialistManager;

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
            this.guid = Guid.NewGuid();
            this.source = source;
            this.destination = destination;
            this.launchTime = launchTime;
            this.drillerCount = drillerCount;
            this.position = source.getCurrentLocation();
            this.owner = owner;
            this.specialistManager = new SpecialistManager(3);
        }

        /// <summary>
        /// Gets the specialist manager of the sub
        /// </summary>
        /// <returns>The specialist manager</returns>
        public SpecialistManager getSpecialistManager()
        {
            return this.specialistManager;
        }

        /// <summary>
        /// Returns the sub's owner
        /// </summary>
        /// <returns>The sub's owner</returns>
        public Player getOwner()
        {
            return this.owner;
        }

        /// <summary>
        /// Gets the estimated arrival time based on known information
        /// </summary>
        /// <returns>The estimated arrival time</returns>
        public GameTick getExpectedArrival()
        {
            GameTick baseTick;
            Vector2 direction;
            int ticksToArrive;
            if(Game.timeMachine.getState().getCurrentTick() < this.launchTime)
            {
                baseTick = this.launchTime;
                // Determine direction vector
                direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.source.getCurrentLocation());
                // Determine the number of ticks to arrive
                ticksToArrive = (int)Math.Floor(direction.Length() / this.getSpeed());
            } else
            {
                baseTick = Game.timeMachine.getState().getCurrentTick();
                // Determine direction vector
                direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.getCurrentLocation());
                // Determine the number of ticks to arrive
                ticksToArrive = (int)Math.Floor(direction.Length() / this.getSpeed());
            }
            return baseTick.advance(ticksToArrive);
        }

        /// <summary>
        /// Get the number of drillers
        /// </summary>
        /// <returns>The number of drillers</returns>
        public int getDrillerCount()
        {
            return this.drillerCount;
        }

        /// <summary>
        /// Gets the position of the current sub
        /// </summary>
        /// <returns>The sub's position</returns>
        public override Vector2 getPosition()
        {

            // Determine number of ticks after launch:
            int elapsedTicks = Game.timeMachine.getState().getCurrentTick() - this.launchTime;

            // Determine direction vector
            Vector2 direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.source.getCurrentLocation());
            direction = Vector2.Normalize(direction);

            if(elapsedTicks > 0)
            {
                this.position = this.source.getCurrentLocation() + (direction * (int)(elapsedTicks * this.getSpeed()));
                return this.source.getCurrentLocation() + (direction * (int)(elapsedTicks * this.getSpeed()));
            }
            else
            {
                return new Vector2();
            }
        }
        
        /// <summary>
        /// Gets the rotation of the sub
        /// </summary>
        /// <returns>Gets the angle of the sub's path</returns>
        public double getRotation()
        {
            // Determine direction vector
            Vector2 direction = this.destination.getTargetLocation(this.getPosition(), this.getSpeed()) - this.source.getCurrentLocation();

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
        public double getSpeed()
        {
            return this.speed;
        }

        /// <summary>
        /// Gets the initial position of the sub
        /// </summary>
        /// <returns>Gets the subs initial position</returns>
        public Vector2 getInitialPosition()
        {
            return this.source.getCurrentLocation();
        }

        /// <summary>
        /// Gets the position of the sub's destination
        /// </summary>
        /// <returns>The destination of the sub</returns>
        public Vector2 getDestinationLocation()
        {
            return this.destination.getTargetLocation(this.getPosition(), this.getSpeed());
        }
        
        /// <summary>
        /// Get the destination
        /// </summary>
        /// <returns>The sub's destination</returns>
        public ITargetable getDestination()
        {
            return this.destination;
        }

        /// <summary>
        /// Get the tick the sub launched
        /// </summary>
        /// <returns>The tick the sub launched</returns>
        public GameTick getLaunchTick()
        {
            return this.launchTime;
        }

        /// <summary>
        /// Get the combat location if this object is targeted by another
        /// </summary>
        /// <param name="targetFrom">The location that this sub is being targeted from</param>
        /// <param name="speed">The speed this is being targeted by</param>
        /// <returns>The location of combat</returns>
        public Vector2 getTargetLocation(Vector2 targetFrom, double speed)
        {
            if (targetFrom == this.getPosition())
                return targetFrom;

            if (speed == 0)
                return targetFrom;

            // Determine target's distance to travel to destination:
            Vector2 targetDestination = this.getDestinationLocation();

            // Check if the chaser can get there before it.
            Vector2 chaserDestination = targetDestination - targetFrom;

            float ticksToDestination = Vector2.Multiply(targetDestination, (float)(1.0 / this.getSpeed())).Length();
            float chaserTicksToDestination = Vector2.Multiply(chaserDestination, (float)(1.0 / speed)).Length();

            if (ticksToDestination > chaserTicksToDestination)
            {
                // Can intercept.
                // Determine interception point.

                int scalar = 1;
                bool searching = true;
                while (searching)
                {
                    Vector2 destination = this.getDestinationLocation() - this.getPosition();
                    destination = Vector2.Normalize(destination);

                    Vector2 runnerLocation = this.getPosition() + (destination * scalar);
                    Vector2 chaserDirection = runnerLocation - targetFrom;
                    chaserDirection = Vector2.Normalize(chaserDestination);
                    Vector2 chaserPosition = targetFrom + (chaserDirection * scalar);

                    if((chaserPosition - runnerLocation).Length() < 1)
                    {
                        return chaserPosition;
                    }
                    if(runnerLocation.Length() > this.getDestinationLocation().Length())
                    {
                        return targetFrom;
                    }
                    scalar = scalar + 1;
                }
                return targetFrom;
            }
            return targetFrom;
        }
        
        /// <summary>
        /// Set the owner
        /// </summary>
        /// <param name="newOwner">The new owner</param>

        public void setOwner(Player newOwner)
        {
            this.owner = newOwner;
        }

        /// <summary>
        /// Get the current location
        /// </summary>
        /// <returns>The sub's current location</returns>
        public Vector2 getCurrentLocation()
        {
            return this.getPosition();
        }

        /// <summary>
        /// The subs direction
        /// </summary>
        /// <returns>The sub's direction vector</returns>
        public Vector2 getDirection()
        {
            Vector2 direction = this.getDestinationLocation() - this.source.getCurrentLocation();
            direction = Vector2.Normalize(direction);
            return direction;
        }

        /// <summary>
        /// Get the launch source
        /// </summary>
        /// <returns>The source of the sub launch</returns>
        public ILaunchable getSource()
        {
            return this.source;
        }
        
        /// <summary>
        /// Set the driller count
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set</param>
        public void setDrillerCount(int drillerCount)
        {
            this.drillerCount = drillerCount;
        }

        /// <summary>
        /// Adds drillers to the sub
        /// </summary>
        /// <param name="drillersToAdd">The number of drillers to add</param>
        public void addDrillers(int drillersToAdd)
        {
            this.drillerCount += drillersToAdd;
        }
        
        /// <summary>
        /// Removes drillers from the sub
        /// </summary>
        /// <param name="drillersToRemove">The number of drillers to remove</param>
        public void removeDrillers(int drillersToRemove)
        {
            this.drillerCount -= drillersToRemove;
        }
        
        /// <summary>
        /// If the sub has the specified number of drillers
        /// </summary>
        /// <param name="drillers">The number of drillers to check for</param>
        /// <returns>If the sub has the specified number of drillers</returns>
        public bool hasDrillers(int drillers)
        {
            return this.drillerCount >= drillers;
        }

        /// <summary>
        /// Launches a sub from this object
        /// </summary>
        /// <param name="drillerCount"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Sub launchSub(int drillerCount, ITargetable destination)
        {
            // Determine any specialist effects if a specialist left the sub.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Undoes launching a sub from this object
        /// </summary>
        /// <param name="sub"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void undoLaunch(Sub sub)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Gets the globally unique indentifier for the Sub.
        /// </summary>
        /// <returns>The Sub's Guid</returns>
        public Guid getGuid()
        {
            return this.guid;
        }
    }
}
