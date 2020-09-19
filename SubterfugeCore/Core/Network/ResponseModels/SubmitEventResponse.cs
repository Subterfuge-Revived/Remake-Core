using System;

namespace SubterfugeCore.Core.Network
{
    [Serializable]
    public class SubmitEventResponse : BaseNetworkResponse
    {
        public int room_id { get; set; }
    }
}