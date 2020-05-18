using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    public class GroupMessageListResponse : BaseNetworkResponse
    {
        public List<NetworkMessage> array { get; set; }
    }
}