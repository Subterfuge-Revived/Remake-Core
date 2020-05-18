using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    public class GameRoomResponse : BaseNetworkResponse
    {
        public List<GameRoom> array { get; set; }
    }
}