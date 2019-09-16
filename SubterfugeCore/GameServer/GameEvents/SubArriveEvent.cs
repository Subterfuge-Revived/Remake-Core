using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCore.GameEvents
{
    class SubArriveEvent : GameEvent
    {
        private SubLaunchEvent launchEvent;
        public SubArriveEvent(SubLaunchEvent subLaunch)
        {
            this.launchEvent = subLaunch;
        }

        public override void eventBackwardAction()
        {
            // GameServer.state.getSubList().Add(new Sub(this.launchEvent.getDestination))
        }

        public override void eventForwardAction()
        {
            GameServer.state.getSubList().Remove(this.launchEvent.getActiveSub());

            // if(this.launchEvent.getDestination().getOwner() == this.launchEvent.getActiveSub().getOwner())
            // {
                this.launchEvent.getDestination().addDrillers(this.launchEvent.getActiveSub().getDrillerCount());
            // } else
            // {
                // this.launchEvent.getDestination().removeDrillers(this.launchEvent.getActiveSub().getDrillerCount());
            // }
            
        }

        public override GameTick getTick()
        {
            throw new NotImplementedException();
        }
    }
}
