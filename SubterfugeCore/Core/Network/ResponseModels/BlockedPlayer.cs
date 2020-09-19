using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class BlockedPlayer : BaseNetworkResponse
    {
        public int sender_player_id { get; set; }
        public int recipient_player_id { get; set; }
    }
}