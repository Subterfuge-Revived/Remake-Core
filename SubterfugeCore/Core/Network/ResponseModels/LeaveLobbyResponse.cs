namespace SubterfugeCore.Core.Network
{
    public class LeaveLobbyResponse : BaseNetworkResponse
    {
        public GameRoom Room { get; set; }
    }
}