using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ISubLaunchEventPublisher
    {
        event EventHandler<OnSubLaunchEventArgs> OnSubLaunched;
    }
    
    public class OnSubLaunchEventArgs : DirectionalEventArgs
    {
        public LaunchEvent LaunchEvent { get; set; }
        public IEntity Source { get; set; }
        public IEntity Destination { get; set; }
        public Sub LaunchedSub { get; set; }
    }
}