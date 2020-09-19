using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class StartLobbyEarlyResponse : BaseNetworkResponse
    {
        public int Room { get; set; }
    }
}