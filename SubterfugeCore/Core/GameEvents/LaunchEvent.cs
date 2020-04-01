using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents
{
    /// <summary>
    /// The event to launch a new sub. Create a new instance and add it to the time machine with `Game.timeMachine.add()`
    /// </summary>
    public class LaunchEvent : GameEvent
    {
        /// <summary>
        /// The time the event will occur
        /// </summary>
        private GameTick launchTime;
        
        /// <summary>
        /// The location the sub is to be launched from
        /// </summary>
        private ILaunchable source;
        
        /// <summary>
        /// The destination for the sub
        /// </summary>
        private ICombatable destination;
        
        /// <summary>
        /// How many drillers should be launched.
        /// </summary>
        private int drillerCount;

        /// <summary>
        /// A reference to the sub once it is launched
        /// </summary>
        private Sub launchedSub;
        
        /// <summary>
        /// Any combat events that are generated because of the launch.
        /// </summary>
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
        public override bool backwardAction()
        {
            if (this.eventSuccess)
            {
                this.source.undoLaunch(this.launchedSub);
                this.removeCombatEvents();
            }

            return this.eventSuccess;
        }

        public override bool wasEventSuccessful()
        {
            return this.eventSuccess;
        }

        public override string toJSON()
        {
            return $"{{ 'source': '{this.source.getId().ToString()}', " +
                   $"'destination': '{this.destination.getId().ToString()}', " +
                   $"'game_tick': {this.launchTime.getTick().ToString()}, " +
                   $"'drillers': {this.drillerCount.ToString()}," +
                   $"'specialists': []," +
                   $"'event_name': '{this.eventName}' }}";
        }

        class DeserializedLaunchEvent
        {
            public int source { get; set; }
            public int destination { get; set; }
            public int game_tick { get; set; }
            public int drillers { get; set; }
            public List<string> specialists { get; set; }
            public string event_name { get; set; }
        }

        public static LaunchEvent fromJSON(string jsonString)
        {
            DeserializedLaunchEvent parsed = JsonConvert.DeserializeObject<DeserializedLaunchEvent>(jsonString);

            GameTick currentTick = Game.timeMachine.getCurrentTick();
            GameTick eventTick = GameTick.fromTickNumber(parsed.game_tick);
            
            // Go to the time the event occurred.
            Game.timeMachine.goTo(eventTick);
            GameState state = Game.timeMachine.getState();

            ILaunchable source = state.getOutpostById(parsed.source);
            if (source == null)
            {
                source = state.getSubById(parsed.source);
            }
            
            ICombatable destination =  state.getOutpostById(parsed.destination);
            if (destination == null)
            {
                destination = state.getSubById(parsed.destination);
            }
            
            Game.timeMachine.goTo(currentTick);
            
            return new LaunchEvent(eventTick, source, parsed.drillers, destination);
        }

        /// <summary>
        /// Performs the forward event
        /// </summary>
        public override bool forwardAction()
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

            return this.eventSuccess;
        }

        /// <summary>
        /// Creates any combat events that will result in the launch.
        /// </summary>
        private void createCombatEvents()
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
        private void removeCombatEvents()
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
