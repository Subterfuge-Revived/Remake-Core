using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;

namespace SubterfugeCore.GameEvents
{
    class SubArriveEvent : GameEvent
    {
        private Sub arrivingSub;
        private GameTick arrivalTime;
        private ITargetable destination;
        public SubArriveEvent(Sub arrivingSub, ITargetable destination, GameTick arrivalTime)
        {
            this.arrivingSub = arrivingSub;
            this.arrivalTime = arrivalTime;
            this.destination = destination;
        }

        public override void eventBackwardAction()
        {
            // GameServer.state.getSubList().Add(new Sub(this.launchEvent.getDestination))
            Sub sub = this.arrivingSub;

            GameServer.state.getSubList().Add(sub);

            if (this.destination.GetType().Equals(typeof(Outpost)))
            {
                Outpost arrivalOutpost = (Outpost)this.destination;
                if (arrivalOutpost.hasDrillers(this.arrivingSub.getDrillerCount()))
                {
                    arrivalOutpost.removeDrillers(this.arrivingSub.getDrillerCount());
                }
            } else
            {
                Console.WriteLine("Sub Combat");
            }
        }

        public override void eventForwardAction()
        {
            if(arrivingSub != null)
            {
                // Remove the sub from the game
                GameServer.state.getSubList().Remove(this.arrivingSub);

                if (this.destination.GetType().Equals(typeof(Outpost)))
                {
                    Outpost arrivalOutpost = (Outpost)this.destination;

                    // check outpost owner.
                    arrivalOutpost.addDrillers(this.arrivingSub.getDrillerCount());
                } else
                {
                    Console.WriteLine("Sub Combat");
                }
            }            
        }

        public override GameTick getTick()
        {
            return this.arrivalTime;
        }
    }
}
