using SubterfugeCore.Core.Components;

namespace SubterfugeCore.Core.EventArgs
{
    public class OnSpeedChangedEventArgs : System.EventArgs
    {
        public float previousSpeed { get; set; }
        public float newSpeed { get; set; }
        public SpeedManager SpeedManager { get; set; }
    }
}