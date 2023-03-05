using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public class UnpauseGameEvent : PlayerTriggeredEvent
    {
        public UnpauseGameEvent(GameRoomEvent model) : base(model, Priority.UNPAUSE_EVENT)
        {
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            timeMachine.TogglePause(false);
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            timeMachine.TogglePause(true);
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
        
        
    }
}