using System;

namespace SubterfugeCore.Core.Network {

    /// <summary>
    /// Represents a chat message sent to a group
    /// </summary>
    [Serializable]
    public class NetworkMessage
    {
        /// <summary>
        /// The id of the message
        /// </summary>
        public int id { get; set; }
        
        /// <summary>
        /// The content of the message
        /// </summary>
        public string message { get; set; }
        
        /// <summary>
        /// The day the message was created
        /// </summary>
        public DateTime created_at { get; set; }
        
        /// <summary>
        /// The player id of the player who sent the message
        /// </summary>
        public DateTime updated_at { get; set; }
        
        /// <summary>
        /// The player id of the player who sent the message
        /// </summary>
        public int sender_player_id { get; set; }
        
        /// <summary>
        /// The group id the message was sent to
        /// </summary>
        public int message_group_id { get; set; }
    }
}