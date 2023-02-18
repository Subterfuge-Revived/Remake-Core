using System;
using System.Collections.Generic;

namespace Subterfuge.Remake.Api.Network
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
    
    public class CreateMessageGroupResponse
    { 
        public string GroupId { get; set; }   
    }

    public class SendMessageRequest
    {
        public string Message { get; set; }
    }
    
    public class SendMessageResponse { }

    public class GetMessageGroupsRequest
    {
    }

    public class GetMessageGroupsResponse
    {
        public List<MessageGroup> MessageGroups { get; set; }
    }

    public class GetGroupMessagesRequest
    {
        public int Pagination { get; set; } = 1;
    }

    public class GetGroupMessagesResponse
    {
        public List<ChatMessage> Messages { get; set; }
    }

    public class GetPlayerChatMessagesResponse
    {
        public List<ChatMessage> Messages { get; set; }
    }
}