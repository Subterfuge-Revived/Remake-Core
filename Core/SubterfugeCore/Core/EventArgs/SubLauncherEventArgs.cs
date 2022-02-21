using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;

namespace SubterfugeCore.Core.EventArgs
{
    public class OnSubLaunchEventArgs : System.EventArgs
    {
        public LaunchEvent LaunchEvent { get; set; }
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Sub LaunchedSub { get; set; }
    }

    public class OnUndoSubLaunchEventArgs : System.EventArgs
    {
        public LaunchEvent LaunchEvent { get; set; }
        public Entity Source { get; set; }
        public Entity Destination { get; set; }
        public Sub LaunchedSub { get; set; }
    }
}