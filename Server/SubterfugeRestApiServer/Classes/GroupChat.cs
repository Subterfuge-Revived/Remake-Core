using MongoDB.Driver;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class GroupChat
    {
        public MessageGroupDatabaseModel MessageGroup;
        public Room Room;
        
        public GroupChat(Room room, MessageGroupDatabaseModel groupModel)
        {
            Room = room;
            MessageGroup = groupModel;
        }

        public async Task<ResponseStatus> SendChatMessage(DbUserModel dbUserModel, string message)
        {
            // Set the creation time.
            ChatMessage model = new ChatMessage()
            {
                Id = Guid.NewGuid().ToString(),
                RoomId = Room.GameConfiguration.Id,
                GroupId = MessageGroup.Id,
                SentBy = dbUserModel.UserModel.ToUser(),
                Message = message,
                UnixTimeCreatedAt = DateTime.UtcNow.ToFileTimeUtc(),
            };
            
            await MongoConnector.GetCollection<ChatMessage>().InsertOneAsync(model);
            
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public async Task<List<ChatMessage>> GetMessages(int pagination = 1)
        {
            // pagination fetches the last 50 messages in the chat.
            var start =  pagination <= 1 ? -1 : -50 * (pagination - 1);
            var end = -50 * pagination;
            List<ChatMessage> parsedMessages = (await MongoConnector.GetCollection<ChatMessage>()
                .FindAsync(
                    message => message.GroupId == MessageGroup.Id && message.RoomId == Room.GameConfiguration.Id, 
                    new FindOptions<ChatMessage>() 
                    {
                        Sort = Builders<ChatMessage>.Sort.Descending(it => it.UnixTimeCreatedAt)
                        
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
            return MessageGroup.MemberIds.Any(it => it == dbUserModel.UserModel.Id);
        }

        public async Task<MessageGroup> asMessageGroup(int messagesPagination = 1)
        {
            MessageGroup model = new MessageGroup()
            {
                Id = MessageGroup.Id,
            };
            
            // Convert UserIds to Users.
            foreach(string? memberId in MessageGroup.MemberIds)
            {
                model.GroupMembers.Add((await DbUserModel.GetUserFromGuid(memberId)).AsUser());
            }
            
            model.Messages.AddRange(await GetMessages(messagesPagination));
            return model;
        }
    }
}