using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    public class BlockPlayerResponse : BaseNetworkResponse
    {
        public List<BlockedPlayer> blockedPlayers { get; set; }
    }
}