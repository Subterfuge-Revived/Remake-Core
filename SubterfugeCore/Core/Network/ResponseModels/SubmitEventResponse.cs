using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class SubmitEventResponse : BaseNetworkResponse
    {
        public int RoomId { get; set; }
    }
}