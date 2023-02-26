using System;
using Subterfuge.Remake.Core.Entities.Components;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface IShieldEventPublisher
    {
        event EventHandler<OnShieldEnableEventArgs> OnShieldEnable;
        event EventHandler<OnShieldDisableEventArgs> OnShieldDisable;
        event EventHandler<OnShieldCapacityChangeEventArgs> OnShieldCapacityChange;
        event EventHandler<OnShieldValueChangeEventArgs> OnShieldValueChange;
    }
    
    public class OnShieldEnableEventArgs : System.EventArgs
    {
        public ShieldManager ShieldManager { get; set; }
    }
    
    public class OnShieldDisableEventArgs : System.EventArgs
    {
        public ShieldManager ShieldManager { get; set; }
    }
    
    public class OnShieldCapacityChangeEventArgs : System.EventArgs
    {
        public int CapacityDelta { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
    
    public class OnShieldValueChangeEventArgs : System.EventArgs
    {
        public int ShieldDelta { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
}