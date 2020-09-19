using System;
using System.Collections.Generic;
using SubterfugeCore.Core.GameEvents.Base;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class GameEventResponse : BaseNetworkResponse
    {
        public List<NetworkGameEvent> array { get; set; }
    }
}