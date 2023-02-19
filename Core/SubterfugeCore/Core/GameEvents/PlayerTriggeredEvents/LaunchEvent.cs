using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
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
        
        private readonly List<GameEvent> _combatEvents = new List<GameEvent>();
        public LaunchEvent(GameRoomEvent launch) : base(launch)
        {
        }

        public void SetLaunchedSub(Sub sub)
        {
            _launchedSub = sub;
        }

        /// <summary>
        /// Performs the backwards event
        /// </summary>
        public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (this.EventSuccess)
            {
                state.GetEntity(GetEventData().SourceId).GetComponent<SubLauncher>().UndoLaunch(state, this);
            }
            return this.EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return this.EventSuccess;
        }

        public LaunchEventData GetEventData()
        {
            return JsonConvert.DeserializeObject<LaunchEventData>(Model.GameEventData.SerializedEventData);
        }

        /// <summary>
        /// Performs the forward event
        /// </summary>
        public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            this._launchedSub = state.GetEntity(GetEventData().SourceId).GetComponent<SubLauncher>().LaunchSub(timeMachine, this);
            
            
            if (_launchedSub != null && !_launchedSub.GetComponent<DrillerCarrier>().GetOwner().IsEliminated())
            {
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
    }
}
