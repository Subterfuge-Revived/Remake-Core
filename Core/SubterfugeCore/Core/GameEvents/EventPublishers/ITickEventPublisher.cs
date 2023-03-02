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

    public class OnTickEventArgs: DirectionalEventArgs
    {
        public GameTick CurrentTick { get; set; }
        public GameState CurrentState { get; set; }
    }

    public class OnGameEventTriggeredEventArgs: DirectionalEventArgs
    {
        public GameEvent GameEvent { get; set; }
        public GameTick CurrentTick { get; set; }
        public GameState CurrentState { get; set; }
    }
}