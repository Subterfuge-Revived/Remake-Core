using System;
using Subterfuge.Remake.Core.Entities.Components;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ISpeedEventPublisher
    {
        event EventHandler<OnSpeedChangedEventArgs> OnSpeedChanged;
    }
    
    public class OnSpeedChangedEventArgs : System.EventArgs
    {
        public float PreviousSpeed { get; set; }
        public float NewSpeed { get; set; }
        public SpeedManager SpeedManager { get; set; }
    }
}