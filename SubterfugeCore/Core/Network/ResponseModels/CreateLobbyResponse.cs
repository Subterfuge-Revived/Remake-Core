namespace SubterfugeCore.Core.Network
{
    public class CreateLobbyResponse : BaseNetworkResponse
    {
        public GameRoom CreatedRoom { get; set; }
    }
}