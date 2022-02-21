using SubterfugeCore.Core.Components;

namespace SubterfugeCore.Core.EventArgs
{
    public class OnSpeedChangedEventArgs : System.EventArgs
    {
        public float PreviousSpeed { get; set; }
        public float NewSpeed { get; set; }
        public SpeedManager SpeedManager { get; set; }
    }
}