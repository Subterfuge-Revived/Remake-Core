using System;
using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class GameRoomResponse : BaseNetworkResponse
    {
        public List<GameRoom> array { get; set; }
    }
}