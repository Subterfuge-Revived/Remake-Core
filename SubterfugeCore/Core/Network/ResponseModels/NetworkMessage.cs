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
        public int Id { get; set; }
        
        /// <summary>
        /// The content of the message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// The day the message was created
        /// </summary>
        public DateTime Created_At { get; set; }
        
        /// <summary>
        /// The player id of the player who sent the message
        /// </summary>
        public DateTime Updated_At { get; set; }
        
        /// <summary>
        /// The player id of the player who sent the message
        /// </summary>
        public int Sender_Player_Id { get; set; }
        
        /// <summary>
        /// The group id the message was sent to
        /// </summary>
        public int Message_Group_Id { get; set; }
    }
}