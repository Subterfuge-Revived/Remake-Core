using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class LeaveLobbyResponse : BaseNetworkResponse
    {
        public GameRoom Room { get; set; }
    }
}