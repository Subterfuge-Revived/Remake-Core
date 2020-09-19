using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class LeaveLobbyResponse : BaseNetworkResponse
    {
        public GameRoom room { get; set; }
    }
}