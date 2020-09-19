using System;
using System.Collections.Generic;
using Jil;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

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
        private GameTick _launchTime;
        
        /// <summary>
        /// The position the sub is to be launched from
        /// </summary>
        private ILaunchable _source;
        
        /// <summary>
        /// The destination for the sub
        /// </summary>
        private ICombatable _destination;
        
        /// <summary>
        /// How many drillers should be launched.
        /// </summary>
        private int _drillerCount;

        /// <summary>
        /// A reference to the sub once it is launched
        /// </summary>
        private Sub _launchedSub;
        
        /// <summary>
        /// Any combat events that are generated because of the launch.
        /// </summary>
        List<CombatEvent> _combatEvents = new List<CombatEvent>();

        /// <summary>
        /// Constructor for a sub launch event.
        /// </summary>
        /// <param name="launchTime">The time of the launch</param>
        /// <param name="source">The source</param>
        /// <param name="drillerCount">The number of drillers to send</param>
        /// <param name="destination">The destination</param>
        public LaunchEvent(GameTick launchTime, ILaunchable source, int drillerCount, ICombatable destination)
        {
            this._launchTime = launchTime;
            this._source = source;
            this._drillerCount = drillerCount;
            this._destination = destination;
            this.EventName = "Launch Event";
        }

        /// <summary>
        /// Performs the backwards event
        /// </summary>
        public override bool BackwardAction()
        {
            if (this.EventSuccess)
            {
                this._source.UndoLaunch(this._launchedSub);
                this.RemoveCombatEvents();
            }

            return this.EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public override string ToJson()
        {
            return $"{{ 'source': '{this._source.GetId().ToString()}', " +
                   $"'destination': '{this._destination.GetId().ToString()}', " +
                   $"'game_tick': {this._launchTime.GetTick().ToString()}, " +
                   $"'drillers': {this._drillerCount.ToString()}," +
                   $"'specialists': []," +
                   $"'event_name': '{this.EventName}' }}";
        }

        [Serializable]
        class DeserializedLaunchEvent
        {
            public int Source { get; set; }
            public int Destination { get; set; }
            public int GameTick { get; set; }
            public int Drillers { get; set; }
            public List<string> Specialists { get; set; }
            public string EventName { get; set; }
        }

        public static LaunchEvent FromJson(string jsonString)
        {
            DeserializedLaunchEvent parsed = JSON.Deserialize<DeserializedLaunchEvent>(jsonString);

            GameTick currentTick = Game.TimeMachine.GetCurrentTick();
            GameTick eventTick = GameTick.FromTickNumber(parsed.GameTick);
            
            // Go to the time the event occurred.
            Game.TimeMachine.GoTo(eventTick);
            GameState state = Game.TimeMachine.GetState();

            ILaunchable source = state.GetOutpostById(parsed.Source);
            if (source == null)
            {
                source = state.GetSubById(parsed.Source);
            }
            
            ICombatable destination =  state.GetOutpostById(parsed.Destination);
            if (destination == null)
            {
                destination = state.GetSubById(parsed.Destination);
            }
            
            Game.TimeMachine.GoTo(currentTick);
            
            return new LaunchEvent(eventTick, source, parsed.Drillers, destination);
        }

        /// <summary>
        /// Performs the forward event
        /// </summary>
        public override bool ForwardAction()
        {
            this._launchedSub = _source.LaunchSub(_drillerCount, _destination);
            if (_launchedSub != null)
            {
                this.CreateCombatEvents();
                this.EventSuccess = true;
            } else
            {
                this.EventSuccess = false;
            }

            return this.EventSuccess;
        }

        /// <summary>
        /// Creates any combat events that will result in the launch.
        /// </summary>
        private void CreateCombatEvents()
        {
            // Create the combat event for arrival
            RftVector targetPosition = this._destination.GetTargetPosition(_source.GetCurrentPosition(), this._launchedSub.GetSpeed());
            GameTick arrival = this._launchTime.Advance((int)Math.Floor((targetPosition - _source.GetCurrentPosition()).Magnitude() / this._launchedSub.GetSpeed()));

            CombatEvent arriveCombat = new CombatEvent(this._launchedSub, this._destination, arrival, targetPosition);
            _combatEvents.Add(arriveCombat);
            Game.TimeMachine.AddEvent(arriveCombat);

            // Determine any combat events that may exist along the way.
            // First determine if any subs are on the same path.
            // Subs will only be on the same path if it is outpost to outpost
            if (this._destination.GetType().Equals(typeof(Outpost)) && this._source.GetType().Equals(typeof(Outpost)))
            {
                // Interpolate to launch time to determine combats!
                GameTick currentTick = Game.TimeMachine.GetCurrentTick();
                Game.TimeMachine.GoTo(this.GetTick());
                GameState interpolatedState = Game.TimeMachine.GetState();


                foreach (Sub sub in interpolatedState.getSubsOnPath((Outpost)this._source, (Outpost)this._destination))
                {
                    // Don't combat with yourself
                    if(sub == this.GetActiveSub())
                        continue;

                    // Determine if we combat it
                    if (sub.GetDirection() == this.GetActiveSub().GetDirection())
                    {
                        if (this.GetActiveSub().GetExpectedArrival() < sub.GetExpectedArrival())
                        {
                            // We can catch it. Determine when and create a combat event.
                        }
                    }
                    else
                    {
                        // Sub is moving towards us.
                        if (sub.GetOwner() != this.GetActiveSub().GetOwner())
                        {
                            // Combat will occur
                            // Determine when and create a combat event.

                            // Determine the number of ticks between the incoming sub & the launched sub.
                            int ticksBetweenSubs = sub.GetExpectedArrival() - this._launchTime;

                            // Determine the speed ratio as a number between 0-0.5
                            double speedRatio = (sub.GetSpeed() / this.GetActiveSub().GetSpeed()) - 0.5;

                            int ticksUntilCombat = (int)Math.Floor(speedRatio * ticksBetweenSubs);

                            // Determine collision position:
                            RftVector combatPosition = new RftVector(RftVector.Map, this.GetActiveSub().GetDirection() * ticksUntilCombat);

                            CombatEvent combatEvent = new CombatEvent(sub, this.GetActiveSub(), this._launchTime.Advance(ticksUntilCombat), combatPosition);
                            _combatEvents.Add(combatEvent);
                            Game.TimeMachine.AddEvent(combatEvent);
                        }
                    }
                }
                // Go back to the original point in time.
                Game.TimeMachine.GoTo(currentTick);
            }
        }

        /// <summary>
        /// Removes remaining combat events from the time machine to prevent ghost combats
        /// </summary>
        private void RemoveCombatEvents()
        {
            foreach(CombatEvent combatEvent in this._combatEvents){
                Game.TimeMachine.RemoveEvent(combatEvent);
            }
        }

        /// <summary>
        /// Gets the tick of the launch
        /// </summary>
        /// <returns>The time of the launch</returns>
        public override GameTick GetTick()
        {
            return this._launchTime;
        }

        /// <summary>
        /// Gets the sub instance of the launch
        /// </summary>
        /// <returns>The sub that was launched</returns>
        public Sub GetActiveSub()
        {
            return this._launchedSub;
        }
    }
}
