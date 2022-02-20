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
        public int previousCapacity { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
    
    public class OnShieldValueChangeEventArgs : System.EventArgs
    {
        public int previousValue { get; set; }
        public ShieldManager ShieldManager { get; set; }
    }
}