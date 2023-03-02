using System;
using Subterfuge.Remake.Core.Entities.Components;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface IShieldEventPublisher
    {
        event EventHandler<OnToggleShieldEventArgs> OnToggleShield;
        event EventHandler<OnShieldCapacityChangeEventArgs> OnShieldCapacityChange;
        event EventHandler<OnShieldValueChangeEventArgs> OnShieldValueChange;
    }
    
    public class OnToggleShieldEventArgs : DirectionalEventArgs
    {
        public ShieldManager ShieldManager { get; set; }
        public bool isEnabled { get; set; }
    }
    
    public class OnShieldCapacityChangeEventArgs : DirectionalEventArgs
    {
        public int CapacityDelta { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
    
    public class OnShieldValueChangeEventArgs : DirectionalEventArgs
    {
        public int ShieldDelta { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
}