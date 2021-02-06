using System;
using SubterfugeCore.Core.GameEvents;

namespace SubterfugeCore.Core.Interfaces.EventHandlers
{
    public class LaunchArguments : EventArgs
    {
        public int DrillerCount { get; set; }
        public string DestinationId { get; set; }
    }
}