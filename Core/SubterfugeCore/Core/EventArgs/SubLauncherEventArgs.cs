using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;

namespace Subterfuge.Remake.Core.EventArgs
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