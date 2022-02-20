using System;
using System.Collections.Generic;
using System.Numerics;
using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents
{
    /// <summary>
    /// The event to launch a new sub. Create a new instance and add it to the time machine with `Game.timeMachine.add()`
    /// </summary>
    public class LaunchEvent : PlayerTriggeredEvent
    {
        /// <summary>
        /// A reference to the sub once it is launched
        /// </summary>
        private Sub _launchedSub;
        
        private List<GameEvent> combatEvents = new List<GameEvent>();
        public LaunchEvent(GameEventModel launchData) : base(launchData)
        {
        }

        public void SetLaunchedSub(Sub sub)
        {
            _launchedSub = sub;
        }

        /// <summary>
        /// Performs the backwards event
        /// </summary>
        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            if (this.EventSuccess)
            {
                state.GetEntity(GetEventData().SourceId).GetComponent<SubLauncher>().UndoLaunch(state, this);
                foreach(GameEvent e in combatEvents)
                {
                    timeMachine.RemoveEvent(e);
                }
                combatEvents.Clear();
            }

            return this.EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public override GameEventModel ToGameEventModel()
        {
            GameEventModel baseModel = GetBaseGameEventModel();
            baseModel.EventData = GetEventData().ToByteString();
            return baseModel;
        }

        public LaunchEventData GetEventData()
        {
            return LaunchEventData.Parser.ParseFrom(model.EventData);
        }

        /// <summary>
        /// Performs the forward event
        /// </summary>
        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            this._launchedSub = state.GetEntity(GetEventData().SourceId).GetComponent<SubLauncher>().LaunchSub(state, this);
            if (_launchedSub != null && _launchedSub is Sub && !_launchedSub.GetComponent<DrillerCarrier>().GetOwner().IsEliminated())
            {
                combatEvents.AddRange(CreateCombatEvents(_launchedSub as Sub, state));
                foreach(GameEvent e in combatEvents)
                {
                    timeMachine.AddEvent(e);
                }
                this.EventSuccess = true;
            } else
            {
                this.EventSuccess = false;
            }

            return this.EventSuccess;
        }

        /// <summary>
        /// Gets the sub instance of the launch
        /// </summary>
        /// <returns>The sub that was launched</returns>
        public Sub GetActiveSub()
        {
            return _launchedSub;
        }

        /// <summary>
        /// Creates any combat events that will result in the launch.
        /// </summary>
        private List<GameEvent> CreateCombatEvents(Sub launchedSub, GameState state)
        {
            List<GameEvent> _combatEvents = new List<GameEvent>();
            
            // Create the combat event for arrival
            CombatEvent arriveCombat = new CombatEvent(launchedSub, state.GetEntity(GetEventData().DestinationId), launchedSub.GetComponent<PositionManager>().GetExpectedArrival(state.CurrentTick));
            _combatEvents.Add(arriveCombat);

            // Determine any combat events that may exist along the way.
            // First determine if any subs are on the same path.
            // Subs will only be on the same path if it is outpost to outpost
            
            // TODO:
            /*
            if (launchedSub.GetComponent<PositionManager>().GetExpectedDestination() is Outpost && launchedSub.GetComponent<PositionManager>().GetSource() is Outpost)
            {
                foreach (Sub sub in state.getSubsOnPath((Outpost)launchedSub.GetComponent<PositionManager>().GetSource(), (Outpost)launchedSub.GetComponent<PositionManager>().GetDestination()))
                {
                    // Don't combat with yourself
                    if(sub == launchedSub)
                        continue;

                    // Determine if we combat it
                    if (sub.GetComponent<PositionManager>().GetDirection() == launchedSub.GetComponent<PositionManager>().GetDirection())
                    {
                        GameTick ourArrival = launchedSub.GetComponent<PositionManager>().GetExpectedArrival();
                        GameTick theirArrival = sub.GetComponent<PositionManager>().GetExpectedArrival();
                        if (ourArrival < theirArrival)
                        {
                            // We can catch it. Determine when and create a combat event.
                            float distanceBetween = (sub.GetComponent<PositionManager>().GetPositionAt(state.CurrentTick) - launchedSub.GetComponent<PositionManager>().GetPositionAt(state.GetCurrentTick())).ToVector2().Length();
                            float velocityDifference = launchedSub.GetComponent<SpeedManager>().GetSpeed() - sub.GetComponent<SpeedManager>().GetSpeed();
                            int catchInTicks = (int)Math.Ceiling(distanceBetween / velocityDifference);

                            CombatEvent catchCombat = new CombatEvent(launchedSub, sub, state.GetCurrentTick().Advance(catchInTicks));
                            _combatEvents.Add(arriveCombat);
                        }
                    }
                    else
                    {
                        // Sub is moving towards us.
                        if (sub.GetComponent<DrillerCarrier>().GetOwner() != launchedSub.GetComponent<DrillerCarrier>().GetOwner())
                        {
                            // Combat will occur
                            // Determine when and create a combat event.

                            // Determine the number of ticks between the incoming sub & the launched sub.
                            int ticksBetweenSubs = sub.GetComponent<PositionManager>().GetExpectedArrival() - launchedSub.GetComponent<PositionManager>().GetSpawnTick();

                            // Determine the speed ratio as a number between 0-0.5
                            double speedRatio = (sub.GetComponent<SpeedManager>().GetSpeed() / launchedSub.GetComponent<SpeedManager>().GetSpeed()) - 0.5;

                            int ticksUntilCombat = (int)Math.Floor(speedRatio * ticksBetweenSubs);

                            // Determine collision position:
                            RftVector combatPosition = new RftVector(RftVector.Map, launchedSub.GetComponent<PositionManager>().GetDirection().ToVector2() * ticksUntilCombat);

                            CombatEvent combatEvent = new CombatEvent(sub, launchedSub, state.CurrentTick.Advance(ticksUntilCombat));
                            _combatEvents.Add(combatEvent);
                        }
                    }
                }
            }
            */
            return _combatEvents;
        }
    }
}
