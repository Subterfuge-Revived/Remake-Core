using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using MongoDB.Driver;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class GroupChatModel
    {
        public GroupModel MessageGroup;
        public DatabaseRoomModel RoomModel;
        
        public GroupChatModel(DatabaseRoomModel roomModel, GroupModel groupModel)
        {
            RoomModel = roomModel;
            MessageGroup = groupModel;
        }

        public async Task<ResponseStatus> SendChatMessage(DatabaseUserModel user, string message)
        {
            // Set the creation time.
            MessageModel model = new MessageModel()
            {
                RoomId = RoomModel.RoomModel.RoomId,
                GroupId = MessageGroup.GroupId,
                SenderId = user.UserModel.Id,
                Message = message,
                UnixTimeCreatedAt = DateTime.UtcNow.ToFileTimeUtc(),
            };
            
            MongoConnector.GetMessagesCollection().InsertOne(model);
            
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public async Task<List<MessageModel>> GetMessages(int pagination = 1)
        {
            // pagination fetches the last 50 messages in the chat.
            var start =  pagination <= 1 ? -1 : -50 * (pagination - 1);
            var end = -50 * pagination;
            List<MessageModel> parsedMessages = MongoConnector.GetMessagesCollection()
                .Find(message => message.GroupId == MessageGroup.GroupId && message.RoomId == RoomModel.RoomModel.RoomId)
                .SortBy(message => message.UnixTimeCreatedAt)
                .Skip(pagination * 50)
                .ToList()
                .Take(50)
                .ToList();

            return parsedMessages;
        }

        public Boolean IsPlayerInGroup(DatabaseUserModel user)
        {
            return MessageGroup.GroupMembers.Any(it => it.Id == user.UserModel.Id);
        }

        public async Task<MessageGroup> asMessageGroup(int messagesPagination = 1)
        {
            MessageGroup model = new MessageGroup()
            {
                GroupId = MessageGroup.GroupId,
            };
            model.GroupMembers.AddRange(MessageGroup.GroupMembers);
            model.Messages.AddRange(await GetMessages(messagesPagination));
            return model;
        }
    }
}