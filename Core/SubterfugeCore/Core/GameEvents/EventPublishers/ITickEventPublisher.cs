using System;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ITickEventPublisher
    {
        event EventHandler<OnTickEventArgs> OnTick;
        event EventHandler<OnGameEventTriggeredEventArgs> OnGameEvent;
    }

    public class OnTickEventArgs
    {
        public GameTick CurrentTick { get; set; }
        public GameState CurrentState { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }

    public enum TimeMachineDirection
    {
        FORWARD = 1,
        REVERSE = 2,
    }

    public class OnGameEventTriggeredEventArgs
    {
        public GameEvent GameEvent { get; set; }
        public GameTick CurrentTick { get; set; }
        public GameState CurrentState { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }
}