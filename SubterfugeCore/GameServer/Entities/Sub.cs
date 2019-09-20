using Microsoft.Xna.Framework;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCore.Entities
{
    public class Sub : GameObject, ITargetable
    {
        private int drillerCount;
        private Vector2 initialPosition;
        private Vector2 destination;
        private GameTick launchTime;
        private float speed = 0.25f;
        private Player owner;

        public Sub(Vector2 source, Vector2 destination, GameTick launchTime, int drillerCount) : base()
        {
            this.initialPosition = source;
            this.destination = destination;
            this.launchTime = launchTime;
            this.drillerCount = drillerCount;
        }

        public Player getOwner()
        {
            return this.owner;
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }

        public override Vector2 getPosition()
        {
            // Determine number of ticks after launch:
            int elapsedTicks = GameState.getCurrentTick() - this.launchTime;

            // Determine direction vector
            Vector2 direction = (this.destination - this.initialPosition);
            direction.Normalize();

            if(elapsedTicks > 0)
            {
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
            Vector2 direction = this.destination - this.initialPosition;

            double extraRotation = 0;
            if(direction.X < 0)
            {
                extraRotation = Math.PI;
            }
            return Math.Atan(direction.Y / direction.X) + Math.PI/4.0f + extraRotation;
        }

        public Vector2 getTargetLocation(GameObject targeter)
        {
            // Gotta get the targeter's poly & determine shortest route.
            // Deteremine if, by the time you reach the location they have past it or not.
            throw new System.NotImplementedException();
        }

        public double getSpeed()
        {
            return this.speed;
        }
    }
}
