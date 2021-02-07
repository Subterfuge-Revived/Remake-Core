using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;
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
        private ICombatable _launchedSub;

        /// <summary>
        /// Constructor for a sub launch event.
        /// </summary>
        /// <param name="launchTime">The time of the launch</param>
        /// <param name="sourceId">The source</param>
        /// <param name="drillerCount">The number of drillers to send</param>
        /// <param name="destinationId">The destination</param>
        public LaunchEvent(GameEventModel launchData) : base(launchData)
        {
        }

        /// <summary>
        /// Performs the backwards event
        /// </summary>
        public override bool BackwardAction(GameState state)
        {
            if (this.EventSuccess)
            {
                state.GetCombatableById(GetEventData().SourceId).UndoLaunch(state, this);
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
        public override bool ForwardAction(GameState state)
        {
            this._launchedSub = state.GetCombatableById(GetEventData().SourceId).LaunchSub(state, this);
            if (_launchedSub != null)
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
        public ICombatable GetActiveSub()
        {
            return this._launchedSub;
        }
    }
}
