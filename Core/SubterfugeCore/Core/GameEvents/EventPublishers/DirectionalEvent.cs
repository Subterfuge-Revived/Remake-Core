using System;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public class DirectionalEventArgs : EventArgs
    {
        public TimeMachineDirection Direction { get; set; }
        public TimeMachine TimeMachine { get; set; }
    }
    
    public enum TimeMachineDirection
    {
        FORWARD = 1,
        REVERSE = 2,
    }
}