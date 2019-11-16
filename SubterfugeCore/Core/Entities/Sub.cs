using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCore.Entities
{
    public class Sub : GameObject, ITargetable, IOwnable, IHasDrillers
    {
        private int drillerCount;
        private Outpost source;
        private ITargetable destination;
        private GameTick launchTime;
        private float speed = 0.25f;
        private Player owner;
        private Outpost sourceOutpost;
        private Player player;

        public Sub(Outpost source, ITargetable destination, GameTick launchTime, int drillerCount, Player owner) : base()
        {
            this.source = source;
            this.destination = destination;
            this.launchTime = launchTime;
            this.drillerCount = drillerCount;
            this.position = source.getCurrentLocation();
            this.owner = owner;
        }

        public Player getOwner()
        {
            return this.owner;
        }

        public GameTick getExpectedArrival()
        {
            GameTick baseTick;
            if(GameServer.state.getCurrentTick() < this.launchTime)
            {
                baseTick = this.launchTime;
            } else
            {
                baseTick = GameServer.state.getCurrentTick();
            }
            // Determine direction vector
            Vector2 direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.source.getCurrentLocation());
            // Determine the number of ticks to arrive
            int ticksToArrive = (int)Math.Floor(direction.Length() / this.getSpeed());
            return baseTick.advance(ticksToArrive);
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }

        public override Vector2 getPosition()
        {

            // Determine number of ticks after launch:
            int elapsedTicks = GameServer.state.getCurrentTick() - this.launchTime;

            // Determine direction vector
            Vector2 direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.source.getCurrentLocation());
            direction.Normalize();

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

        public double getSpeed()
        {
            return this.speed;
        }

        public Vector2 getInitialPosition()
        {
            return this.source.getCurrentLocation();
        }

        public Vector2 getDestinationLocation()
        {
            return this.destination.getTargetLocation(this.getPosition(), this.getSpeed());
        }

        public ITargetable getDestination()
        {
            return this.destination;
        }

        public GameTick getLaunchTick()
        {
            return this.launchTime;
        }

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
                    destination.Normalize();

                    Vector2 runnerLocation = this.getPosition() + (destination * scalar);
                    Vector2 chaserDirection = runnerLocation - targetFrom;
                    chaserDirection.Normalize();
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

                // Interception will occur at a distance 'd' where the "ticksToArrive" at that distance are the same.

                // If both arrive at the same time,
                // tta1 = tta1
                // The ticks required to arrive at a location is given by:
                // tta = distanceToLocation / speed
                // Thus, DistanceFromRunner / RunnerSpeed = DistanceFromChaser / ChaserSpeed
                // Thus, ChaserSpeed / RunnerSpeed = DistanceFromChaser / DistanceFromRunner
                // The distance from the chaser is given as the magnitude of: targetFrom - this.Position()
                // Of course, the position of the runner changes based on the ticks to arrive.

            }
            return targetFrom;
        }

        public void setOwner(Player newOwner)
        {
            this.owner = newOwner;
        }

        public Vector2 getCurrentLocation()
        {
            return this.getPosition();
        }

        public Vector2 getDirection()
        {
            Vector2 direction = this.getDestinationLocation() - this.source.getCurrentLocation();
            direction.Normalize();
            return direction;
        }

        public Outpost getSourceOutpost()
        {
            return this.source;
        }

        public void setDrillerCount(int drillerCount)
        {
            this.drillerCount = drillerCount;
        }

        public void addDrillers(int drillersToAdd)
        {
            this.drillerCount += drillersToAdd;
        }

        public void removeDrillers(int drillersToRemove)
        {
            this.drillerCount -= drillersToRemove;
        }

        public bool hasDrillers(int drillers)
        {
            return this.drillerCount >= drillers;
        }
    }
}
