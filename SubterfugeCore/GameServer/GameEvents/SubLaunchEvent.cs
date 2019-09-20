using Microsoft.Xna.Framework;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;

namespace SubterfugeCore.GameEvents
{
    class SubLaunchEvent : GameEvent
    {
        private GameTick launchTime;
        private Outpost sourceOutpost;
        private Outpost destination;
        private int drillerCount;
        private Sub activeSub;
        public SubLaunchEvent(GameTick launchTime, Outpost sourceOutpost, int drillerCount, Outpost destination)
        {
            this.launchTime = launchTime;
            this.sourceOutpost = sourceOutpost;
            this.drillerCount = drillerCount;
            this.destination = destination;
        }

        public override void eventBackwardAction()
        {
            // Check if the sub was launched
            if(activeSub != null)
            {
                GameState state = GameServer.state;

                sourceOutpost.addDrillers(this.drillerCount);
                state.getSubList().Remove(this.activeSub);
                this.activeSub = null;                
            }
        }

        public override void eventForwardAction()
        {
            if (sourceOutpost.hasDrillers(drillerCount))
            {
                GameState state = GameServer.state;

                sourceOutpost.removeDrillers(drillerCount);
                Sub launchedSub = new Sub(sourceOutpost.getPosition(), destination.getPosition(), launchTime, drillerCount);
                state.getSubList().Add(launchedSub);
                this.activeSub = launchedSub;
            }
        }

        public Vector2 getLaunchDirection()
        {
            return this.getDestination().getPosition() - this.getSourceOutpost().getPosition();
        }

        public override GameTick getTick()
        {
            return this.launchTime;
        }

        public Sub getActiveSub()
        {
            return this.activeSub;
        }

        public Outpost getDestination()
        {
            return this.destination;
        }

        public Outpost getSourceOutpost()
        {
            return this.sourceOutpost;
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }

        public override List<GameEvent> getResultingEvents()
        {
            List<GameEvent> resultEvents = new List<GameEvent>();


            // Determine the expected arrival time.
            // Get the sub's current speed
            double subSpeed = 0.25;
            if(this.getActiveSub() != null)
            {
                // Use player's speed buff or default.
                subSpeed = this.getActiveSub().getSpeed();
            }

            // Get the magnitude of the destination
            Vector2 destination = this.getLaunchDirection();
            double distance = destination.Length();

            // distance is total travel time. Divide by sub speed to determine the number of ticks to arrival
            int ticksToArrive = (int)Math.Ceiling(distance / subSpeed);

            SubArriveEvent arrivalEvent = new SubArriveEvent(this, this.launchTime.advance(ticksToArrive));
            resultEvents.Add(arrivalEvent);
            return resultEvents;
        }
    }
}
