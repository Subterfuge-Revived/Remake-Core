using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class BlockedPlayer : BaseNetworkResponse
    {
        public int SenderPlayerId { get; set; }
        public int RecipientPlayerId { get; set; }
    }
}