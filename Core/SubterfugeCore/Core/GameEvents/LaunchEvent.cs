using System;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeRemakeService;

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
        private string _sourceId;
        
        /// <summary>
        /// The destination for the sub
        /// </summary>
        private string _destinationId;
        
        /// <summary>
        /// How many drillers should be launched.
        /// </summary>
        private int _drillerCount;

        /// <summary>
        /// A reference to the sub once it is launched
        /// </summary>
        private ICombatable _launchedSub;

        /// <summary>
        /// Constructor for a sub launch event.
        /// </summary>
        /// <param name="launchTime">The time of the launch</param>
        /// <param name="sourceId">The source</param>
        /// <param name="drillerCount">The number of drillers to send</param>
        /// <param name="destinationId">The destination</param>
        public LaunchEvent(GameTick launchTime, string sourceId, int drillerCount, string destinationId)
        {
            this._launchTime = launchTime;
            this._sourceId = sourceId;
            this._drillerCount = drillerCount;
            this._destinationId = destinationId;
            this.EventName = "Launch Event";
        }

        /// <summary>
        /// Performs the backwards event
        /// </summary>
        public override bool BackwardAction(GameState state)
        {
            if (this.EventSuccess)
            {
                state.GetCombatableById(_sourceId).UndoLaunch(this._launchedSub);
                // this.RemoveCombatEvents();
            }

            return this.EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public override GameEventModel ToEventModel()
        {
            return new GameEventModel()
            {
                EventData = this.ToJson(),
                EventId = EventId,
                IssuedBy = this.IssuedBy.GetId(),
                OccursAtTick = this.UnixTimeIssued,
            };
        }

        public string ToJson()
        {
            return $"{{ 'source': '{this._sourceId.ToString()}', " +
                   $"'destination': '{this._destinationId.ToString()}', " +
                   $"'game_tick': {this._launchTime.GetTick().ToString()}, " +
                   $"'drillers': {this._drillerCount.ToString()}," +
                   $"'specialists': []," +
                   $"'event_name': '{this.EventName}' }}";
        }

        class DeserializedLaunchEvent
        {
            public string Source { get; set; }
            public string Destination { get; set; }
            public int GameTick { get; set; }
            public int Drillers { get; set; }
            public List<string> Specialists { get; set; }
            public string EventName { get; set; }
        }

        public static LaunchEvent FromJson(string jsonString)
        {
            DeserializedLaunchEvent parsed = JsonConvert.DeserializeObject<DeserializedLaunchEvent>(jsonString);
            return new LaunchEvent(new GameTick(parsed.GameTick), parsed.Source, parsed.Drillers, parsed.Destination);
        }

        /// <summary>
        /// Performs the forward event
        /// </summary>
        public override bool ForwardAction(GameState state)
        {
            
            this._launchedSub = state.GetCombatableById(_sourceId).LaunchSub(_drillerCount,state.GetCombatableById(_destinationId));
            if (_launchedSub != null)
            {
                // this.CreateCombatEvents();
                this.EventSuccess = true;
            } else
            {
                this.EventSuccess = false;
            }

            return this.EventSuccess;
        }

        // /// <summary>
        // /// Creates any combat events that will result in the launch.
        // /// </summary>
        // private void CreateCombatEvents()
        // {
        //     // Create the combat event for arrival
        //     RftVector targetPosition = this._destinationId.GetInterceptionPosition(_sourceId.GetCurrentPosition(), this._launchedSub.GetSpeed());
        //     GameTick arrival = this._launchTime.Advance((int)Math.Floor((targetPosition - _sourceId.GetCurrentPosition()).Magnitude() / this._launchedSub.GetSpeed()));
        //
        //     CombatEvent arriveCombat = new CombatEvent(this._launchedSub, this._destinationId, arrival, targetPosition);
        //     _combatEvents.Add(arriveCombat);
        //     Game.TimeMachine.AddEvent(arriveCombat);
        //
        //     // Determine any combat events that may exist along the way.
        //     // First determine if any subs are on the same path.
        //     // Subs will only be on the same path if it is outpost to outpost
        //     if (this._destinationId.GetType().Equals(typeof(Outpost)) && this._sourceId.GetType().Equals(typeof(Outpost)))
        //     {
        //         // Interpolate to launch time to determine combats!
        //         GameTick currentTick = Game.TimeMachine.GetCurrentTick();
        //         Game.TimeMachine.GoTo(this.GetTick());
        //         GameState interpolatedState = Game.TimeMachine.GetState();
        //
        //
        //         foreach (Sub sub in interpolatedState.getSubsOnPath((Outpost)this._sourceId, (Outpost)this._destinationId))
        //         {
        //             // Don't combat with yourself
        //             if(sub == this.GetActiveSub())
        //                 continue;
        //
        //             // Determine if we combat it
        //             if (sub.GetDirection() == this.GetActiveSub().GetDirection())
        //             {
        //                 if (this.GetActiveSub().GetExpectedArrival() < sub.GetExpectedArrival())
        //                 {
        //                     // We can catch it. Determine when and create a combat event.
        //                 }
        //             }
        //             else
        //             {
        //                 // Sub is moving towards us.
        //                 if (sub.GetOwner() != this.GetActiveSub().GetOwner())
        //                 {
        //                     // Combat will occur
        //                     // Determine when and create a combat event.
        //
        //                     // Determine the number of ticks between the incoming sub & the launched sub.
        //                     int ticksBetweenSubs = sub.GetExpectedArrival() - this._launchTime;
        //
        //                     // Determine the speed ratio as a number between 0-0.5
        //                     double speedRatio = (sub.GetSpeed() / this.GetActiveSub().GetSpeed()) - 0.5;
        //
        //                     int ticksUntilCombat = (int)Math.Floor(speedRatio * ticksBetweenSubs);
        //
        //                     // Determine collision position:
        //                     RftVector combatPosition = new RftVector(RftVector.Map, this.GetActiveSub().GetDirection() * ticksUntilCombat);
        //
        //                     CombatEvent combatEvent = new CombatEvent(sub, this.GetActiveSub(), this._launchTime.Advance(ticksUntilCombat), combatPosition);
        //                     _combatEvents.Add(combatEvent);
        //                     Game.TimeMachine.AddEvent(combatEvent);
        //                 }
        //             }
        //         }
        //         // Go back to the original point in time.
        //         Game.TimeMachine.GoTo(currentTick);
        //     }
        // }

        // /// <summary>
        // /// Removes remaining combat events from the time machine to prevent ghost combats
        // /// </summary>
        // private void RemoveCombatEvents()
        // {
        //     foreach(CombatEvent combatEvent in this._combatEvents){
        //         Game.TimeMachine.RemoveEvent(combatEvent);
        //     }
        // }

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
        public ICombatable GetActiveSub()
        {
            return this._launchedSub;
        }
    }
}
