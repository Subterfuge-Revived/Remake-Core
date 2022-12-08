using System.Collections.Generic;
using System.Linq;

namespace SubterfugeCore.Models.GameEvents
{

    public interface INetworkChatManager
    {
        CreateMessageGroupResponse CreateMessageGroup(CreateMessageGroupRequest createMessageGroupRequest);

        SendMessageResponse SendMessage(SendMessageRequest sendMessageRequest);

        GetMessageGroupsResponse GetMessageGroups(GetMessageGroupsRequest getMessageGroupsRequest);

        GetGroupMessagesResponse GetGroupMessages(GetGroupMessagesRequest getGroupMessagesRequest);
    }

    public class MessageGroupDatabaseModel
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public List<string> MemberIds { get; set; }
    }
    
    public class MessageGroup
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public List<User> GroupMembers { get; set; }
        public List<ChatMessage> Messages { get; set; }

        public MessageGroupDatabaseModel ToDatabaseModel()
        {
            return new MessageGroupDatabaseModel()
            {
                Id = Id,
                RoomId = RoomId,
                MemberIds = GroupMembers.Select(it => it.Id).ToList()
            };
        }
    }

    public class ChatMessage
    {
        public string RoomId { get; set; }
        public string GroupId { get; set; }
        public string Id { get; set; }
        public long UnixTimeCreatedAt { get; set; }
        public User SentBy { get; set; }
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
        public int Pagination { get; set; }
    }

    public class GetGroupMessagesResponse : NetworkResponse
    {
        public MessageGroup Group { get; set; }
    }
}