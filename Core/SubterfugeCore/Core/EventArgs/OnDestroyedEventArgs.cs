using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.EventArgs
{
    public class OnDestroyedEventArgs : System.EventArgs
    {
        public DrillerCarrier DrillerCarrier { get; set; }
        public GameTick tickDestroyed { get; set; }
    }
}