using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents
{
    /// <summary>
    /// Sub launch event
    /// </summary>
    public class LaunchEvent : GameEvent
    {
        private GameTick launchTime;
        private ILaunchable source;
        private ICombatable destination;
        private int drillerCount;

        private Sub launchedSub;
        List<CombatEvent> combatEvents = new List<CombatEvent>();

        /// <summary>
        /// Constructor for a sub launch event.
        /// </summary>
        /// <param name="launchTime">The time of the launch</param>
        /// <param name="source">The source</param>
        /// <param name="drillerCount">The number of drillers to send</param>
        /// <param name="destination">The destination</param>
        public LaunchEvent(GameTick launchTime, ILaunchable source, int drillerCount, ICombatable destination)
        {
            this.launchTime = launchTime;
            this.source = source;
            this.drillerCount = drillerCount;
            this.destination = destination;
            this.eventName = "Launch Event";
        }

        /// <summary>
        /// Performs the backwards event
        /// </summary>
        public override void eventBackwardAction()
        {
            if (this.eventSuccess)
            {
                this.source.undoLaunch(this.launchedSub);
                this.removeCombatEvents();
            }
        }

        /// <summary>
        /// Performs the forward event
        /// </summary>
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

        /// <summary>
        /// Creates any combat events that will result in the launch.
        /// </summary>
        public void createCombatEvents()
        {
            // Create the combat event for arrival
            Vector2 targetLocation = this.destination.getTargetLocation(source.getCurrentLocation(), this.launchedSub.getSpeed());
            GameTick arrival = this.launchTime.advance((int)Math.Floor((targetLocation - source.getCurrentLocation()).Length() / this.launchedSub.getSpeed()));

            CombatEvent arriveCombat = new CombatEvent(this.launchedSub, this.destination, arrival, targetLocation);
            combatEvents.Add(arriveCombat);
            Game.timeMachine.addEvent(arriveCombat);

            // Determine any combat events that may exist along the way.
            // First determine if any subs are on the same path.
            // Subs will only be on the same path if it is outpost to outpost
            if (this.destination.GetType().Equals(typeof(Outpost)) && this.source.GetType().Equals(typeof(Outpost)))
            {
                // Interpolate to launch time to determine combats!
                GameTick currentTick = Game.timeMachine.getCurrentTick();
                Game.timeMachine.goTo(this.getTick());
                GameState interpolatedState = Game.timeMachine.getState();


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
                            combatEvents.Add(combatEvent);
                            Game.timeMachine.addEvent(combatEvent);
                        }
                    }
                }
                // Go back to the original point in time.
                Game.timeMachine.goTo(currentTick);
            }
        }

        /// <summary>
        /// Removes remaining combat events from the time machine to prevent ghost combats
        /// </summary>
        public void removeCombatEvents()
        {
            foreach(CombatEvent combatEvent in this.combatEvents){
                Game.timeMachine.removeEvent(combatEvent);
            }
        }

        /// <summary>
        /// Gets the tick of the launch
        /// </summary>
        /// <returns>The time of the launch</returns>
        public override GameTick getTick()
        {
            return this.launchTime;
        }

        /// <summary>
        /// Gets the sub instance of the launch
        /// </summary>
        /// <returns>The sub that was launched</returns>
        public Sub getActiveSub()
        {
            return this.launchedSub;
        }
    }
}
