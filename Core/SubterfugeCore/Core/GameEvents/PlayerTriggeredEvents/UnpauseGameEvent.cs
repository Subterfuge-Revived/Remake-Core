using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public class UnpauseGameEvent : PlayerTriggeredEvent
    {
        public UnpauseGameEvent(GameRoomEvent model) : base(model)
        {
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool WasEventSuccessful()
        {
            throw new System.NotImplementedException();
        }
    }
}