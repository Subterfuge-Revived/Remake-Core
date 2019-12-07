using Microsoft.Xna.Framework;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;

namespace SubterfugeCore.GameEvents
{
    public class LaunchEvent : GameEvent
    {
        private GameTick launchTime;
        private ILaunchable source;
        private ICombatable destination;
        private int drillerCount;

        private Sub launchedSub;
        List<CombatEvent> combatEvents = new List<CombatEvent>();

        public LaunchEvent(GameTick launchTime, ILaunchable source, int drillerCount, ICombatable destination)
        {
            this.launchTime = launchTime;
            this.source = source;
            this.drillerCount = drillerCount;
            this.destination = destination;
            this.eventName = "Launch Event";
        }

        public override void eventBackwardAction()
        {
            if (this.eventSuccess)
            {
                this.source.undoLaunch(this.launchedSub);
                this.removeCombatEvents();
            }
        }

        public override void eventForwardAction()
        {
            this.launchedSub = source.launchSub(drillerCount, destination);
            if (launchedSub != null)
            {
                this.createCombatEvents();
                this.eventSuccess = true;
            } else
            {
                this.eventSuccess = false;
            }
        }

        public void createCombatEvents()
        {
            // Create the combat event for arrival
            Vector2 targetLocation = this.destination.getTargetLocation(source.getCurrentLocation(), this.launchedSub.getSpeed());
            GameTick arrival = this.launchTime.advance((int)Math.Floor((targetLocation - source.getCurrentLocation()).Length() / this.launchedSub.getSpeed()));

            CombatEvent arriveCombat = new CombatEvent(this.launchedSub, this.destination, arrival, targetLocation);
            GameServer.timeMachine.addEvent(arriveCombat);

            // Determine any combat events that may exist along the way.
            // First determine if any subs are on the same path.
            // Subs will only be on the same path if it is outpost to outpost
            if (this.destination.GetType().Equals(typeof(Outpost)) && this.source.GetType().Equals(typeof(Outpost)))
            {
                // Interpolate to launch time to determine combats!
                GameTick currentTick = GameServer.timeMachine.getCurrentTick();
                GameServer.timeMachine.goTo(this.getTick());
                GameState interpolatedState = GameServer.timeMachine.getState();


                foreach (Sub sub in interpolatedState.getSubsOnPath((Outpost)this.source, (Outpost)this.destination))
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

                            CombatEvent combatEvent = new CombatEvent(sub, this.getActiveSub(), this.launchTime.advance(ticksUntilCombat), combatLocation);
                            GameServer.timeMachine.addEvent(combatEvent);
                        }
                    }
                }
                // Go back to the original point in time.
                GameServer.timeMachine.goTo(currentTick);
            }
        }

        public void removeCombatEvents()
        {
            foreach(CombatEvent combatEvent in this.combatEvents){
                GameServer.timeMachine.removeEvent(combatEvent);
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

        public ILaunchable getSource()
        {
            return this.source;
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }
    }
}
