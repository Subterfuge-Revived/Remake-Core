using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class CreateLobbyResponse : BaseNetworkResponse
    {
        public GameRoom CreatedRoom { get; set; }
    }
}