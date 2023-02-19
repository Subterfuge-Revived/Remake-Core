using System;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ITickEventPublisher
    {
        event EventHandler<OnTickEventArgs> OnTick;
    }

    public class OnTickEventArgs
    {
        public GameTick CurrentTick { get; set; }
        public GameState.GameState CurrentState { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }

    public enum TimeMachineDirection
    {
        FORWARD = 1,
        REVERSE = 2,
    }
}