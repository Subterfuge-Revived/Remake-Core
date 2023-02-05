using System;
using System.Collections.Generic;
using System.Linq;

namespace SubterfugeCore.Models.GameEvents
{

    public class MessageGroup
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public List<User> GroupMembers { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string RoomId { get; set; }
        public string GroupId { get; set; }
        public SimpleUser SentBy { get; set; }
        public string Message { get; set; }
    }

    public class CreateMessageGroupRequest
    {
        public List<string> UserIdsInGroup { get; set; }
    }
    
    public class CreateMessageGroupResponse : NetworkResponse
    { 
        public string GroupId { get; set; }   
    }

    public class SendMessageRequest
    {
        public string Message { get; set; }
    }
    
    public class SendMessageResponse : NetworkResponse { }

    public class GetMessageGroupsRequest
    {
    }

    public class GetMessageGroupsResponse : NetworkResponse
    {
        public List<MessageGroup> MessageGroups { get; set; }
    }

    public class GetGroupMessagesRequest
    {
        public int Pagination { get; set; } = 1;
    }

    public class GetGroupMessagesResponse : NetworkResponse
    {
        public List<ChatMessage> Messages { get; set; }
    }

    public class GetPlayerChatMessagesResponse : NetworkResponse
    {
        public List<ChatMessage> Messages { get; set; }
    }
}