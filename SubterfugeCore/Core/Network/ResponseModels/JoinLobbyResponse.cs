using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class JoinLobbyResponse : BaseNetworkResponse
    {
        public GameRoom Room { get; set; }   
    }
}