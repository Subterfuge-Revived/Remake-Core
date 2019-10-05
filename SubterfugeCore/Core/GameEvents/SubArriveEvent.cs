using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Players;
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

        private bool subArrived = false;

        // Before Combat saves
        private Player originalDesitnationOwner;
        private int originalDestinationDrillers;
        public SubArriveEvent(Sub arrivingSub, ITargetable destination, GameTick arrivalTime)
        {
            this.arrivingSub = arrivingSub;
            this.arrivalTime = arrivalTime;
            this.destination = destination;
        }

        public override void eventBackwardAction()
        {
            if(subArrived)
            {
                // GameServer.state.getSubList().Add(new Sub(this.launchEvent.getDestination))
                Sub sub = this.arrivingSub;

                GameServer.state.getSubList().Add(sub);

                if (this.destination.GetType().Equals(typeof(Outpost)))
                {
                    Outpost arrivalOutpost = (Outpost)this.destination;

                    // Set outpost to original owner and driller count
                    arrivalOutpost.setOwner(this.originalDesitnationOwner);
                    arrivalOutpost.setDrillerCount(this.originalDestinationDrillers);
                }
                else
                {
                    Console.WriteLine("Sub Combat");
                }
            }
        }

        public override void eventForwardAction()
        {
            // Check if the sub is still alive
            if(GameServer.state.getSubList().Contains(arrivingSub))
            {
                // Remove the sub from the game
                GameServer.state.getSubList().Remove(this.arrivingSub);
                subArrived = true;

                if (this.destination.GetType().Equals(typeof(Outpost)))
                {
                    Outpost arrivalOutpost = (Outpost)this.destination;
                    this.originalDesitnationOwner = arrivalOutpost.getOwner();
                    this.originalDestinationDrillers = arrivalOutpost.getDrillerCount();

                    if (arrivalOutpost.getOwner() != this.arrivingSub.getOwner())
                    {
                        if (arrivalOutpost.hasDrillers(this.arrivingSub.getDrillerCount()))
                        {
                            arrivalOutpost.removeDrillers(this.arrivingSub.getDrillerCount());
                        }
                        else
                        {
                            arrivalOutpost.setDrillerCount(this.arrivingSub.getDrillerCount() - arrivalOutpost.getDrillerCount());
                            arrivalOutpost.setOwner(this.arrivingSub.getOwner());
                        }
                    }
                    else
                    {
                        arrivalOutpost.addDrillers(this.arrivingSub.getDrillerCount());
                    }
                }
                else
                {
                    Console.WriteLine("Sub-targeting-Sub Combat");
                }
            } 
        }

        public override GameTick getTick()
        {
            return this.arrivalTime;
        }
    }
}
