using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class JoinLobbyResponse : BaseNetworkResponse
    {
        public GameRoom room { get; set; }   
    }
}