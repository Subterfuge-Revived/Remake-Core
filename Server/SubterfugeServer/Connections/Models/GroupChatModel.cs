using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using StackExchange.Redis;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class GroupChatModel
    {
        public GroupModel MessageGroup;
        public RedisRoomModel RoomModel;
        
        public GroupChatModel(RedisRoomModel roomModel, RedisValue groupChat)
        {
            RoomModel = roomModel;
            MessageGroup = GroupModel.Parser.ParseFrom(groupChat);
        }

        public async Task<ResponseStatus> SendChatMessage(RedisUserModel user, string message)
        {
            // Set the creation time.
            MessageModel model = new MessageModel()
            {
                Message = message,
                SenderId = user.UserModel.Id,
                UnixTimeCreatedAt = DateTime.UtcNow.ToFileTimeUtc(),
            };
            
            await RedisConnector.Redis.ListRightPushAsync(MessageListKey(), model.ToByteArray());
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public async Task<List<MessageModel>> GetMessages(int pagination = 1)
        {
            // pagination fetches the last 50 messages in the chat.
            var start =  pagination <= 1 ? -1 : -50 * (pagination - 1);
            var end = -50 * pagination;
            RedisValue[] messages = await RedisConnector.Redis.ListRangeAsync(MessageListKey(), end, start);
            List<MessageModel> parsedMessages = new List<MessageModel>();
            foreach(var messageBytes in messages.Reverse())
            {
                parsedMessages.Add(MessageModel.Parser.ParseFrom(messageBytes));
            }

            return parsedMessages;
        }

        public Boolean IsPlayerInGroup(RedisUserModel user)
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

        private string MessageListKey()
        {
            return $"game:{RoomModel.RoomModel.RoomId}:groups:{MessageGroup.GroupId}:messages";
        }
    }
}