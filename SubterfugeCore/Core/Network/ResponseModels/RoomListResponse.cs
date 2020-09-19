using System;
using System.Collections.Generic;

namespace SubterfugeCore.Core.Network
{
    /// <summary>
    /// Class is parsed in Unity by using JsonConvert.Deserialize() on the RoomList Network response.
    /// For some reason including the JSON library in the dll to do it here throws errors.
    /// </summary>
    [Serializable]
    public class RoomListResponse : BaseNetworkResponse
    {
        /// <summary>
        /// A list of the game rooms returned from the network.
        /// </summary>
        public List<GameRoom> Rooms { get; set; }
    }
}