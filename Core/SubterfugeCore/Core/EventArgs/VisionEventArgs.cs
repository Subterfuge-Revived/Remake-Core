using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.EventArgs
{
    public class OnVisionRangeChangeEventArgs : System.EventArgs
    {
        public float PreviousVisionRange { get; set; }
        public float NewVisionRange { get; set; }
        public VisionManager VisionManager { get; set; }
    }

    public class OnEntityEnterVisionRangeEventArgs : System.EventArgs
    {
        public Entity EntityInVision { get; set; }
        public VisionManager VisionManager { get; set; }
    }
    
    public class OnEntityLeaveVisionRangeEventArgs : System.EventArgs
    {
        public Entity EntityLeavingVision { get; set; }
        public VisionManager VisionManager { get; set; }
    }
}