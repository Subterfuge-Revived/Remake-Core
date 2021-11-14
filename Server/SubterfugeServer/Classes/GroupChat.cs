using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using MongoDB.Bson;
using MongoDB.Driver;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class GroupChat
    {
        public GroupModel MessageGroup;
        public Room Room;
        
        public GroupChat(Room room, GroupModel groupModel)
        {
            Room = room;
            MessageGroup = groupModel;
        }

        public async Task<ResponseStatus> SendChatMessage(DbUserModel dbUserModel, string message)
        {
            // Set the creation time.
            MessageModel model = new MessageModel()
            {
                Id = Guid.NewGuid().ToString(),
                RoomId = Room.GameConfiguration.Id,
                GroupId = MessageGroup.Id,
                SenderId = dbUserModel.UserModel.Id,
                Message = message,
                UnixTimeCreatedAt = DateTime.UtcNow.ToFileTimeUtc(),
            };
            
            await MongoConnector.GetMessagesCollection().InsertOneAsync(model);
            
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public async Task<List<MessageModel>> GetMessages(int pagination = 1)
        {
            // pagination fetches the last 50 messages in the chat.
            var start =  pagination <= 1 ? -1 : -50 * (pagination - 1);
            var end = -50 * pagination;
            List<MessageModel> parsedMessages = (await MongoConnector.GetMessagesCollection()
                .FindAsync(
                    message => message.GroupId == MessageGroup.Id && message.RoomId == Room.GameConfiguration.Id, 
                    new FindOptions<MessageModel>() 
                    {
                        Sort = Builders<MessageModel>.Sort.Descending(it => it.UnixTimeCreatedAt)
                        
                    }
                ))
                .ToList()
                .Skip((pagination - 1) * 50)
                .ToList()
                .Take(50)
                .ToList();

            return parsedMessages;
        }

        public Boolean IsPlayerInGroup(DbUserModel dbUserModel)
        {
            return MessageGroup.GroupMembers.Any(it => it == dbUserModel.UserModel.Id);
        }

        public async Task<MessageGroup> asMessageGroup(int messagesPagination = 1)
        {
            MessageGroup model = new MessageGroup()
            {
                GroupId = MessageGroup.Id,
            };
            
            // Convert UserIds to Users.
            foreach(string userId in MessageGroup.GroupMembers)
            {
                model.GroupMembers.Add((await DbUserModel.GetUserFromGuid(userId)).AsUser());
            }
            
            model.Messages.AddRange(await GetMessages(messagesPagination));
            return model;
        }
    }
}