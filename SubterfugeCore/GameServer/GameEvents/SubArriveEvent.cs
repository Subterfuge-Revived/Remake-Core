using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;

namespace SubterfugeCore.GameEvents
{
    class SubArriveEvent : GameEvent
    {
        private SubLaunchEvent launchEvent;
        private Sub landedSub;
        private GameTick arrivalTime;
        public SubArriveEvent(SubLaunchEvent subLaunch, GameTick arrivalTime)
        {
            this.launchEvent = subLaunch;
            this.arrivalTime = arrivalTime;
        }

        public override void eventBackwardAction()
        {
            // GameServer.state.getSubList().Add(new Sub(this.launchEvent.getDestination))
            Sub sub = new Sub(this.launchEvent.getSourceOutpost().getPosition(), this.launchEvent.getDestination().getPosition(), this.launchEvent.getTick(), this.landedSub.getDrillerCount());

            GameServer.state.getSubList().Add(sub);

            this.launchEvent.getDestination().addDrillers(sub.getDrillerCount());
        }

        public override void eventForwardAction()
        {
            // Ensure the sub launched to begin with
            if (this.launchEvent.getActiveSub() != null)
            {
                // Save the state of the sub that landed with the event.
                this.landedSub = this.launchEvent.getActiveSub();

                // Remove the sub from the game
                GameServer.state.getSubList().Remove(this.launchEvent.getActiveSub());

                // if(this.launchEvent.getDestination().getOwner() == this.launchEvent.getActiveSub().getOwner())
                // {
                this.launchEvent.getDestination().addDrillers(this.launchEvent.getActiveSub().getDrillerCount());
                // } else
                // {
                // this.launchEvent.getDestination().removeDrillers(this.launchEvent.getActiveSub().getDrillerCount());
                // }
            }
            
        }

        public override List<GameEvent> getResultingEvents()
        {
            throw new NotImplementedException();
        }

        public override GameTick getTick()
        {
            return this.arrivalTime;
        }
    }
}
