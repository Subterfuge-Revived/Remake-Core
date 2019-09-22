using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCore.Entities
{
    public class Sub : GameObject, ITargetable, IOwnable
    {
        private int drillerCount;
        private Vector2 initialPosition;
        private ITargetable destination;
        private GameTick launchTime;
        private float speed = 0.25f;
        private Player owner;
        private SubArriveEvent expectedArrival;

        public Sub(Vector2 source, ITargetable destination, GameTick launchTime, int drillerCount) : base()
        {
            this.initialPosition = source;
            this.destination = destination;
            this.launchTime = launchTime;
            this.drillerCount = drillerCount;
            this.expectedArrival = new SubArriveEvent(this, this.destination, this.getExpectedArrival());
            GameServer.state.addEvent(this.expectedArrival);
        }

        public Player getOwner()
        {
            return this.owner;
        }

        public GameTick getExpectedArrival()
        {
            // Determine direction vector
            Vector2 direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.initialPosition);
            // Determine the number of ticks to arrive
            int ticksToArrive = (int)Math.Floor(direction.Length() / this.getSpeed());
            return GameServer.state.getCurrentTick().advance(ticksToArrive);
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
            Vector2 direction = (this.destination.getTargetLocation(this.initialPosition, this.getSpeed()) - this.initialPosition);
            direction.Normalize();

            if(elapsedTicks > 0)
            {
                this.position = this.initialPosition + (direction * (int)(elapsedTicks * this.getSpeed()));
                return this.initialPosition + (direction * (int)(elapsedTicks * this.getSpeed()));
            }
            else
            {
                return new Vector2();
            }
        }

        public double getRotation()
        {
            // Determine direction vector
            Vector2 direction = this.destination.getTargetLocation(this.getPosition(), this.getSpeed()) - this.initialPosition;

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
            return this.initialPosition;
        }

        public Vector2 getDestination()
        {
            return this.destination.getTargetLocation(this.getPosition(), this.getSpeed());
        }

        public GameTick getLaunchTick()
        {
            return this.launchTime;
        }

        public Vector2 getTargetLocation(Vector2 targetFrom, double speed)
        {
            throw new NotImplementedException();
        }

        public void setOwner(Player newOwner)
        {
            this.owner = newOwner;
        }
    }
}
