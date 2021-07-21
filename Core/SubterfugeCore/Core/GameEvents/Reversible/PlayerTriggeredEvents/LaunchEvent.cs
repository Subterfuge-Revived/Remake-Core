using System;
using System.Collections.Generic;
using System.Numerics;
using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.Reversible.PlayerTriggeredEvents;
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
        private LaunchEventData _launchEventData;
        
        /// <summary>
        /// A reference to the sub once it is launched
        /// </summary>
        private ICombatable _launchedSub;
        
        private List<GameEvent> combatEvents = new List<GameEvent>();
        public LaunchEvent(GameEventModel eventModel) : base(eventModel)
        {
        }

        public LaunchEvent(ICombatable launchedSub, ITargetable destination, GameTick occursAt) : base(
            Guid.NewGuid().ToString(),
            EventType.LaunchEvent,
            launchedSub.GetOwner(),
            occursAt,
            new LaunchEventData()
            {
                SourceId = launchedSub.GetId(),
                DestinationId = destination.GetOwner().GetId(),
                DrillerCount = launchedSub.GetDrillerCount(),
            }.ToByteString()
        ) {}

        /// <summary>
        /// Performs the backwards event
        /// </summary>
        public override void BackwardAction(TimeMachine timeMachine)
        {
            GameState state = timeMachine.GetState();
                state.GetCombatableById(_launchEventData.SourceId).UndoLaunch(state, this);
                foreach(GameEvent e in combatEvents)
                {
                    timeMachine.RemoveEvent(e);
                }
                combatEvents.Clear();
        }

        /// <summary>
        /// Gets the sub instance of the launch
        /// </summary>
        /// <returns>The sub that was launched</returns>
        public ICombatable GetActiveSub()
        {
            return this._launchedSub;
        }

        /// <summary>
        /// Creates any combat events that will result in the launch.
        /// </summary>
        private List<GameEvent> CreateCombatEvents(Sub launchedSub, GameState state)
        {
            List<GameEvent> _combatEvents = new List<GameEvent>();
            
            // Create the combat event for arrival
            CombatEvent arriveCombat = new CombatEvent(launchedSub, launchedSub.GetDestination(), launchedSub.GetExpectedArrival());
            _combatEvents.Add(arriveCombat);

            // Determine any combat events that may exist along the way.
            // First determine if any subs are on the same path.
            // Subs will only be on the same path if it is outpost to outpost
            if (launchedSub.GetDestination() is Outpost && launchedSub.GetSource() is Outpost)
            {
                foreach (Sub sub in state.getSubsOnPath((Outpost)launchedSub.GetSource(), (Outpost)launchedSub.GetDestination()))
                {
                    // Don't combat with yourself
                    if(sub == launchedSub)
                        continue;

                    // Determine if we combat it
                    if (sub.GetDirection() == launchedSub.GetDirection())
                    {
                        GameTick ourArrival = launchedSub.GetExpectedArrival();
                        GameTick theirArrival = sub.GetExpectedArrival();
                        if (ourArrival < theirArrival)
                        {
                            // We can catch it. Determine when and create a combat event.
                            float distanceBetween = (sub.GetCurrentPosition(state.CurrentTick) - launchedSub.GetCurrentPosition(state.GetCurrentTick())).ToVector2().Length();
                            float velocityDifference = launchedSub.GetSpeed() - sub.GetSpeed();
                            int catchInTicks = (int)Math.Ceiling(distanceBetween / velocityDifference);

                            CombatEvent catchCombat = new CombatEvent(launchedSub, sub, state.GetCurrentTick().Advance(catchInTicks));
                            _combatEvents.Add(arriveCombat);
                        }
                    }
                    else
                    {
                        // Sub is moving towards us.
                        if (sub.GetOwner() != launchedSub.GetOwner())
                        {
                            // Combat will occur
                            // Determine when and create a combat event.

                            // Determine the number of ticks between the incoming sub & the launched sub.
                            int ticksBetweenSubs = sub.GetExpectedArrival() - launchedSub.GetLaunchTick();

                            // Determine the speed ratio as a number between 0-0.5
                            double speedRatio = (sub.GetSpeed() / launchedSub.GetSpeed()) - 0.5;

                            int ticksUntilCombat = (int)Math.Floor(speedRatio * ticksBetweenSubs);

                            // Determine collision position:
                            RftVector combatPosition = new RftVector(RftVector.Map, launchedSub.GetDirection().ToVector2() * ticksUntilCombat);

                            CombatEvent combatEvent = new CombatEvent(sub, launchedSub, state.CurrentTick.Advance(ticksUntilCombat));
                            _combatEvents.Add(combatEvent);
                        }
                    }
                }
            }
            return _combatEvents;
        }

        public override void ForwardAction(TimeMachine timeMachine)
        {
            GameState state = timeMachine.GetState();
            this._launchedSub = state.GetCombatableById(_launchEventData.SourceId).LaunchSub(state, this);
            if (_launchedSub != null && _launchedSub is Sub && !_launchedSub.GetOwner().IsEliminated())
            {
                combatEvents.AddRange(CreateCombatEvents(_launchedSub as Sub, state));
                foreach(GameEvent e in combatEvents)
                {
                    timeMachine.AddEvent(e);
                }
            }
        }

        public override void parseGameEventModel(GameEventModel eventModel)
        {
            if (eventModel.EventType == EventType.LaunchEvent)
            {
                _launchEventData = LaunchEventData.Parser.ParseFrom(eventModel.EventData);
            }
        }
    }
}
