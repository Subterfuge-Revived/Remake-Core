using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
{
    public class PauseGameEvent : PlayerTriggeredEvent
    {
        public PauseGameEvent(GameRoomEvent model) : base(model)
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