using System;
using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class BlockPlayerResponse : BaseNetworkResponse
    {
        public List<BlockedPlayer> array { get; set; }
    }
}