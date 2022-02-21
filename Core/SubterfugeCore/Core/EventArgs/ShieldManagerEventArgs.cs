using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.EventArgs
{
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
        public int PreviousCapacity { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
    
    public class OnShieldValueChangeEventArgs : System.EventArgs
    {
        public int PreviousValue { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
}