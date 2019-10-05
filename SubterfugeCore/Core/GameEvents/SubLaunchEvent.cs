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
            this.launchedSub = new Sub(sourceOutpost, destination, launchTime, drillerCount, sourceOutpost.getOwner());

            if (destination.GetType().Equals(typeof(Outpost)))
            {
                SubArriveEvent arrivalEvent = new SubArriveEvent(launchedSub, this.destination, launchedSub.getExpectedArrival());
                GameServer.state.addEvent(arrivalEvent);
            } else
            {
                Vector2 targetLocation = this.destination.getTargetLocation(sourceOutpost.getPosition(), this.launchedSub.getSpeed());
                GameTick arrival = this.launchTime.advance((int)Math.Floor((targetLocation - sourceOutpost.getPosition()).Length() / this.launchedSub.getSpeed()));
                SubCombatEvent combatEvent = new SubCombatEvent(this.launchedSub, (Sub)destination, arrival,  targetLocation);
                GameServer.state.addEvent(combatEvent);
            }
            this.createCombatEvents();
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

        public void createCombatEvents()
        {
            // Determine any combat events that may exist along the way.
            // First determine if any subs are on the same path.
            if (this.destination.GetType().Equals(typeof(Outpost)))
            {
                // Interpolate to launch time to determine combats!
                GameTick currentTick = GameServer.state.getCurrentTick();
                GameServer.state.goTo(this);


                foreach (Sub sub in GameServer.state.getSubsOnPath(this.sourceOutpost, (Outpost)this.destination))
                {
                    // Don't combat with yourself
                    if(sub == this.getActiveSub())
                        continue;

                    // Determine if we combat it
                    if (sub.getDirection() == this.getActiveSub().getDirection())
                    {
                        if (this.getActiveSub().getExpectedArrival() < sub.getExpectedArrival())
                        {
                            // We can catch it. Determine when and create a combat event.
                        }
                    }
                    else
                    {
                        // Sub is moving towards us.
                        if (sub.getOwner() != this.getActiveSub().getOwner())
                        {
                            // Combat will occur
                            // Determine when and create a combat event.

                            // Determine the number of ticks between the incoming sub & the launched sub.
                            int ticksBetweenSubs = sub.getExpectedArrival() - this.launchTime;

                            // Determine the speed ratio as a number between 0-0.5
                            double speedRatio = (sub.getSpeed() / this.getActiveSub().getSpeed()) - 0.5;

                            int ticksUntilCombat = (int)Math.Floor(speedRatio * ticksBetweenSubs);

                            // Determine collision location:
                            Vector2 combatLocation = Vector2.Multiply(this.getActiveSub().getDirection(), (float)ticksUntilCombat);

                            SubCombatEvent combatEvent = new SubCombatEvent(sub, this.getActiveSub(), GameServer.state.getCurrentTick().advance(ticksUntilCombat), combatLocation);
                            GameServer.state.addEvent(combatEvent);
                        }
                    }
                }


                // Go back to original time.
                GameServer.state.interpolateTick(currentTick);
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
