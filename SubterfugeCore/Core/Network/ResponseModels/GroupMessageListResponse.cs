using System;
using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class GroupMessageListResponse : BaseNetworkResponse
    {
        public List<NetworkMessage> array { get; set; }
    }
}