using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface IVisionEventPublisher
    {
        event EventHandler<OnVisionRangeChangeEventArgs> OnVisionRangeChange;
        event EventHandler<OnEntityEnterVisionRangeEventArgs> OnEntityEnterVisionRange;
        event EventHandler<OnEntityLeaveVisionRangeEventArgs> OnEntityLeaveVisionRange;
    }
    
    public class OnVisionRangeChangeEventArgs : DirectionalEventArgs
    {
        public float PreviousVisionRange { get; set; }
        public float NewVisionRange { get; set; }
        public VisionManager VisionManager { get; set; }
    }

    public class OnEntityEnterVisionRangeEventArgs : DirectionalEventArgs
    {
        public IEntity EntityInVision { get; set; }
        public VisionManager VisionManager { get; set; }
    }
    
    public class OnEntityLeaveVisionRangeEventArgs : DirectionalEventArgs
    {
        public IEntity EntityLeavingVision { get; set; }
        public VisionManager VisionManager { get; set; }
    }
}