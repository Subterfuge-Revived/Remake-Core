namespace SubterfugeCore.Core.Network
{
    public class BlockedPlayer : BaseNetworkResponse
    {
        public int SenderPlayerId { get; set; }
        public int RecipientPlayerId { get; set; }
    }
}