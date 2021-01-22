using System;
using System.Threading.Tasks;
using Google.Protobuf;
using StackExchange.Redis;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections.Models
{
    public class RedisRoomModel
    {
        
        public static async Task<Room> getRoom(Guid roomId)
        {
            RedisValue roomData = await RedisConnector.Redis.HashGetAsync($"game:{roomId.ToString()}", new RedisValue("roomData"));
            if (roomData.HasValue)
            {
                Room room = Room.Parser.ParseFrom(ByteString.FromBase64(roomData.ToString()));
                
                // Get players in the room.
                RedisValue[] playersInRoom = await RedisConnector.Redis.HashKeysAsync($"game:{roomId.ToString()}:players");
                foreach (RedisValue playerId in playersInRoom)
                {
                    RedisUserModel user = await RedisUserModel.GetUserFromUsername(playerId);
                    if (user != null)
                    {
                        room.Players.Add(user.asUser());
                    }
                }
                return room;
            }

            return null;
        }
    }
}