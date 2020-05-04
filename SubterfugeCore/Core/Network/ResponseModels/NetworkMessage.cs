using System;

namespace SubterfugeCore.Core.Network {

    /// <summary>
    /// Represents a chat message sent to a group
    /// </summary>
    public class NetworkMessage
    {
        /// <summary>
        /// The content of the message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// The day the message was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// The player id of the player who sent the message
        /// </summary>
        public int SenderPlayerId { get; set; }
    }
}