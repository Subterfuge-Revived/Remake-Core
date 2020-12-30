using System.Collections.Generic;
using SubterfugeCore.Core.GameEvents.Base;

namespace SubterfugeCore.Core.Network
{
    public class GameEventResponse : BaseNetworkResponse
    {
        public List<NetworkGameEvent> array { get; set; }
    }
}