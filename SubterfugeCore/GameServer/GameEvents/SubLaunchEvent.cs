using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;

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
    }
}
