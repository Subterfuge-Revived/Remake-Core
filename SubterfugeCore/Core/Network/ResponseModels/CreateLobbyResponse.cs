using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class CreateLobbyResponse : BaseNetworkResponse
    {
        public GameRoom created_room { get; set; }
    }
}