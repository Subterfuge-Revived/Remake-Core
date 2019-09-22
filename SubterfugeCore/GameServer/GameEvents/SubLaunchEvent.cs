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
        private ITargetable destination;
        private int drillerCount;
        private Sub launchedSub;
        public SubLaunchEvent(GameTick launchTime, Outpost sourceOutpost, int drillerCount, ITargetable destination)
        {
            this.launchTime = launchTime;
            this.sourceOutpost = sourceOutpost;
            this.drillerCount = drillerCount;
            this.destination = destination;
            this.launchedSub = new Sub(sourceOutpost.getPosition(), destination, launchTime, drillerCount);
        }

        public override void eventBackwardAction()
        {
            GameState state = GameServer.state;

            sourceOutpost.addDrillers(this.drillerCount);
            state.getSubList().Remove(this.launchedSub);
        }

        public override void eventForwardAction()
        {
            if (sourceOutpost.hasDrillers(drillerCount))
            {
                GameState state = GameServer.state;

                sourceOutpost.removeDrillers(drillerCount);
                state.getSubList().Add(launchedSub);
            }
        }

        public override GameTick getTick()
        {
            return this.launchTime;
        }

        public Sub getActiveSub()
        {
            return this.launchedSub;
        }

        public ITargetable getDestination()
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
    }
}
