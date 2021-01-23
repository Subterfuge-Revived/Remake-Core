using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using StackExchange.Redis;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeServerConsole.Connections.Models
{
    public class RedisRoomModel
    {

        public RoomModel RoomModel;

        public RedisRoomModel(RedisValue value)
        {
            RoomModel = RoomModel.Parser.ParseFrom(value);
        }

        public RedisRoomModel(CreateRoomRequest request, RedisUserModel creator)
        {
            Guid roomId = Guid.NewGuid();
            RoomModel = new RoomModel()
            {
                Anonymous = request.Anonymous,
                CreatorId = creator.UserModel.Id,
                Goal = request.Goal,
                MaxPlayers = request.MaxPlayers,
                RankedInformation = request.RankedInformation,
                RoomId = roomId.ToString(),
                RoomName = request.RoomName,
                RoomStatus = RoomStatus.Open,
                Seed = new Random().Next(),
                UnixTimeCreated =  DateTime.Now.ToFileTimeUtc(),
                UnixTimeStarted = 0,
            };
            RoomModel.AllowedSpecialists.AddRange(request.AllowedSpecialists);
        }

        public async Task<Boolean> JoinRoom(RedisUserModel userModel)
        {
            List<RedisUserModel> playersInRoom = await GetPlayersInGame();
            if (RoomModel.RoomStatus == RoomStatus.Open && playersInRoom.Count < RoomModel.MaxPlayers && playersInRoom.All(it => it.UserModel.Id != userModel.UserModel.Id))
            {
                await RedisConnector.Redis.SetAddAsync($"game:{RoomModel.RoomId}:players", userModel.UserModel.Id);
                await RedisConnector.Redis.SetAddAsync($"users:{userModel.UserModel.Id}:games", RoomModel.RoomId);
                return true;
            }

            return false;
        }
        
        public async Task<Boolean> LeaveRoom(RedisUserModel userModel)
        {
            if (RoomModel.RoomStatus == RoomStatus.Open)
            {
                await RedisConnector.Redis.SetRemoveAsync($"game:{RoomModel.RoomId}:players", userModel.UserModel.Id);
                await RedisConnector.Redis.SetRemoveAsync($"users:{userModel.UserModel.Id}:games", RoomModel.RoomId);
            }
            // TODO: Player left the game while ongoing.
            // Create a player leave game event and push to event list

            return false;
        }

        public async Task<List<RedisUserModel>> GetPlayersInGame()
        {
            List<RedisUserModel> players = new List<RedisUserModel>();
            RedisValue[] playerIds = await RedisConnector.Redis.SetMembersAsync($"game:{RoomModel.RoomId}:players");
            foreach(var playerId in playerIds)
            {
                players.Add(await RedisUserModel.GetUserFromGuid(playerId));
            }

            return players;
        }

        public async Task<List<GameEvent>> GetPlayerGameEvents(RedisUserModel player)
        {
            List<GameEvent> events = new List<GameEvent>();
            RedisValue[] eventIds = await RedisConnector.Redis.SetMembersAsync($"game:{RoomModel.RoomId}:user:{player.UserModel.Id}:events");
            foreach (var eventId in eventIds)
            {
                events.Add(await GetGameEventFromGuid(eventId));
            }
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            return events;
        }

        public async Task<SubmitGameEventResponse> AddPlayerGameEvent(RedisUserModel player, GameEvent gameEvent)
        {
            Guid eventId = Guid.NewGuid();
            HashEntry[] entries = new[]
            {
                new HashEntry(eventId.ToString(), gameEvent.ToByteArray()),
            };
            
            // Ensure player is the one issuing the command.
            if (player.UserModel.Id != gameEvent.IssuedBy.Id)
            {
                return new SubmitGameEventResponse()
                {
                    Success = false
                };
            }
            
            // Ensure the event happens after current time.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.Now);
            if (gameEvent.OccursAtTick <= currentTick.GetTick())
            {
                return new SubmitGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // TODO: validate event.
            
            await RedisConnector.Redis.HashSetAsync($"game:{RoomModel.RoomId}:events", entries);
            await RedisConnector.Redis.SetAddAsync($"game:{RoomModel.RoomId}:user:{player.UserModel.Id}:events", eventId.ToString());
            return new SubmitGameEventResponse()
            {
                Success = true,
                EventId = eventId.ToString(),
            };
        }
        
        public async Task<DeleteGameEventResponse> RemovePlayerGameEvent(RedisUserModel player, string eventId)
        {
            Guid parsedGuid;
            try
            {
                parsedGuid = Guid.Parse(eventId);
            }
            catch (FormatException e)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };
            }
            
            // Get the event to check some things...
            RedisValue eventData = await RedisConnector.Redis.HashGetAsync($"game:{RoomModel.RoomId}:events", parsedGuid.ToString());
            if (eventData.HasValue)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // Determine if the event has already passed.
            GameEvent gameEvent = GameEvent.Parser.ParseFrom(eventData);
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.Now);
            
            if (gameEvent.OccursAtTick <= currentTick.GetTick() || gameEvent.IssuedBy.Id != player.UserModel.Id)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // Determine if the event was issued by the player trying to delete the event.
            if (player.UserModel.Id != gameEvent.IssuedBy.Id)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // Remove game Event
            await RedisConnector.Redis.SetRemoveAsync($"game:{RoomModel.RoomId}:user:{player.UserModel.Id}:events", parsedGuid.ToString());
            await RedisConnector.Redis.HashDeleteAsync($"game:{RoomModel.RoomId}:events", parsedGuid.ToString());
            return new DeleteGameEventResponse()
            {
                Success = true
            };
        }

        public async Task<GameEvent> GetGameEventFromGuid(string eventId)
        {
            Guid parsedGuid;
            try
            {
                parsedGuid = Guid.Parse(eventId);
            }
            catch (FormatException e)
            {
                return null;
            }
            
            RedisValue eventData = await RedisConnector.Redis.HashGetAsync($"game:{RoomModel.RoomId}:events", parsedGuid.ToString());
            if (eventData.HasValue)
            {
                return GameEvent.Parser.ParseFrom(eventData);
            }

            return null;
        }

        public async Task<List<GameEvent>> GetAllGameEvents()
        {
            List<GameEvent> events = new List<GameEvent>();
            HashEntry[] eventHashes = await RedisConnector.Redis.HashGetAllAsync($"game:{RoomModel.RoomId}:events");
            foreach (var eventHash in eventHashes)
            {
                events.Add(GameEvent.Parser.ParseFrom(eventHash.Value));
            }
            return events;
        }
        
        public async Task<List<GameEvent>> GetAllPastGameEvents()
        {
            List<GameEvent> events = await GetAllGameEvents();
            
            // Get current game tick
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.Now);
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            // Filter
            return events.FindAll(it => it.OccursAtTick <= currentTick.GetTick());
        }
        
        public async Task<Boolean> StartGameEarly()
        {
            RoomModel.RoomStatus = RoomStatus.Ongoing;
            RoomModel.UnixTimeStarted = DateTime.Now.ToFileTimeUtc();
            SaveToDatabase();
            await RedisConnector.Redis.SetRemoveAsync("openlobbies", RoomModel.RoomId);
            return true;
        }

        public static async Task<List<RedisRoomModel>> GetOpenLobbies()
        {
            List<RedisRoomModel> rooms = new List<RedisRoomModel>();
            RedisValue[] roomIds = await RedisConnector.Redis.SetMembersAsync("openlobbies");
            foreach (var roomId in roomIds)
            {
                rooms.Add(await GetRoomFromGuid(roomId));
            }

            return rooms;
        }
        

        public static async Task<RedisRoomModel> GetRoomFromGuid(string roomGuid)
        {
            Guid parsedGuid;
            try
            {
                parsedGuid = Guid.Parse(roomGuid);
            }
            catch (FormatException e)
            {
                return null;
            }

            
            RedisValue roomData = await RedisConnector.Redis.HashGetAsync("games", parsedGuid.ToString());
            if (roomData.HasValue)
            {
                return new RedisRoomModel(roomData);
            }

            return null;
        }

        public async Task<Boolean> SaveToDatabase()
        {
            HashEntry[] roomRecord =
            {
                new HashEntry(RoomModel.RoomId, RoomModel.ToByteArray()),
            };
            await RedisConnector.Redis.HashSetAsync("games", roomRecord);
            
            if (RoomModel.RoomStatus == RoomStatus.Open)
            {
                await RedisConnector.Redis.SetAddAsync("openlobbies", RoomModel.RoomId);
            }
            
            return true;
        }

        public async Task<Room> asRoom()
        {
            Room room = new Room()
            {
                RoomId = RoomModel.RoomId,
                RoomStatus = RoomModel.RoomStatus,
                Creator = (await RedisUserModel.GetUserFromGuid(RoomModel.CreatorId)).asUser(),
                RankedInformation = RoomModel.RankedInformation,
                Anonymous = RoomModel.Anonymous, 
                RoomName = RoomModel.RoomName,
                Goal = RoomModel.Goal,
                Seed = RoomModel.Seed,
                UnixTimeCreated = RoomModel.UnixTimeCreated,
                UnixTimeStarted = RoomModel.UnixTimeStarted,
                MaxPlayers = RoomModel.MaxPlayers,
            };
            room.Players.AddRange((await GetPlayersInGame()).ConvertAll(it => it.asUser()));
            room.AllowedSpecialists.AddRange(RoomModel.AllowedSpecialists);
            return room;
        }
    }
}