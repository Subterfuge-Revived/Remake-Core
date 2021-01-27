using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
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
            GameTick.MINUTES_PER_TICK = RoomModel.MinutesPerTick;
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
                RankedInformation = new RankedInformation()
                {
                    IsRanked = request.IsRanked,
                    MinRating = 500,
                    MaxRating = 1500, // TODO: +-100 of user rank
                },
                RoomId = roomId.ToString(),
                RoomName = request.RoomName,
                RoomStatus = RoomStatus.Open,
                Seed = new Random().Next(),
                UnixTimeCreated =  DateTime.UtcNow.ToFileTimeUtc(),
                UnixTimeStarted = 0,
                MinutesPerTick = request.MinutesPerTick,
            };
            RoomModel.AllowedSpecialists.AddRange(request.AllowedSpecialists);
            GameTick.MINUTES_PER_TICK = RoomModel.MinutesPerTick;
        }

        public async Task<Boolean> JoinRoom(RedisUserModel userModel)
        {
            List<RedisUserModel> playersInRoom = await GetPlayersInGame();
            if(playersInRoom.Count >= RoomModel.MaxPlayers)
                throw new RpcException(new Status(StatusCode.ResourceExhausted, "Room is full."));
            
            if(playersInRoom.Any(it => it.UserModel.Id == userModel.UserModel.Id))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Player is already in this game."));
            
            if(RoomModel.RoomStatus != RoomStatus.Open)
                throw new RpcException(new Status(StatusCode.Unavailable, "Cannot join a game that has already started."));
            
            // Check if any players in the room have the same device identifier
            if(playersInRoom.Any(it => it.UserModel.DeviceIdentifier == userModel.UserModel.DeviceIdentifier))
                throw new RpcException(new Status(StatusCode.PermissionDenied, "Cannot join a game with someone using the same device."));

            await RedisConnector.Redis.SetAddAsync(GameRoomPlayerListKey(), userModel.UserModel.Id);
            await RedisConnector.Redis.SetAddAsync(UserGameListKey(userModel), RoomModel.RoomId);
            
            // Check if the player joining the game is the last player.
            if (playersInRoom.Count + 1 == RoomModel.MaxPlayers)
                await StartGame();
            
            return true;
        }

        public async Task<Boolean> IsRoomFull()
        {
            return (await GetPlayersInGame()).Count >= RoomModel.MaxPlayers;
        }
        
        public async Task<Boolean> IsPlayerInRoom(RedisUserModel player)
        {
            return (await GetPlayersInGame()).Any(it => it.UserModel.Id == player.UserModel.Id);
        }
        
        public async Task<Boolean> LeaveRoom(RedisUserModel userModel)
        {
            if (RoomModel.RoomStatus == RoomStatus.Open)
            {
                // Check if the player leaving was the host.
                if (RoomModel.CreatorId == userModel.UserModel.Id)
                {
                    // Remove all players from the game.
                    foreach (var player in await GetPlayersInGame())
                    {
                        // Make sure we don't infinitely loop.
                        if(player.UserModel.Id != RoomModel.CreatorId)
                            await LeaveRoom(player);
                    }
                    
                    // Destroy room.
                    await RedisConnector.Redis.HashDeleteAsync(GameRoomsHashKey(), RoomModel.RoomId);
                    await RedisConnector.Redis.SetRemoveAsync(OpenLobbiesKey(), RoomModel.RoomId);
                    
                    // Finally, remove creator from game
                    await RedisConnector.Redis.SetRemoveAsync(GameRoomPlayerListKey(), userModel.UserModel.Id);
                    await RedisConnector.Redis.SetRemoveAsync(UserGameListKey(userModel), RoomModel.RoomId);
                    return true;
                }
                
                await RedisConnector.Redis.SetRemoveAsync(GameRoomPlayerListKey(), userModel.UserModel.Id);
                await RedisConnector.Redis.SetRemoveAsync(UserGameListKey(userModel), RoomModel.RoomId);
                return true;
            }
            // TODO: Player left the game while ongoing.
            // Create a player leave game event and push to event list

            return false;
        }

        public async Task<List<RedisUserModel>> GetPlayersInGame()
        {
            List<RedisUserModel> players = new List<RedisUserModel>();
            RedisValue[] playerIds = await RedisConnector.Redis.SetMembersAsync(GameRoomPlayerListKey());
            foreach(var playerId in playerIds)
            {
                players.Add(await RedisUserModel.GetUserFromGuid(playerId));
            }

            return players;
        }

        public async Task<List<GameEventModel>> GetPlayerGameEvents(RedisUserModel player)
        {
            List<GameEventModel> events = new List<GameEventModel>();
            RedisValue[] eventIds = await RedisConnector.Redis.SetMembersAsync(UserGameEventsKey(player));
            foreach (var eventId in eventIds)
            {
                events.Add(await GetGameEventFromGuid(eventId));
            }
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            return events;
        }

        public async Task<SubmitGameEventResponse> UpdateGameEvent(RedisUserModel player, UpdateGameEventRequest request)
        {
            GameEventModel gameEvent = await GetGameEventFromGuid(request.EventId);
            if (gameEvent == null || gameEvent.IssuedBy != player.UserModel.Id)
            {
                return new SubmitGameEventResponse()
                {
                    Success = false
                };;
            }

            // Ensure the event happens after current time.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            if (request.EventData.OccursAtTick <= currentTick.GetTick())
            {
                return new SubmitGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // TODO: validate event.
            gameEvent.EventData = request.EventData.EventData;
            gameEvent.OccursAtTick = request.EventData.OccursAtTick;

            HashEntry[] entries = new[]
            {
                new HashEntry(gameEvent.EventId, gameEvent.ToByteArray()),
            };
            
            await RedisConnector.Redis.HashSetAsync(GameEventsKey(), entries);
            await RedisConnector.Redis.SetAddAsync(UserGameEventsKey(player), gameEvent.EventId);
            return new SubmitGameEventResponse()
            {
                Success = true,
                EventId = gameEvent.EventId,
            }; 
        }

        public async Task<SubmitGameEventResponse> AddPlayerGameEvent(RedisUserModel player,  GameEventRequest request)
        {
            GameEventModel eventModel = toGameEventModel(player, request);

            // Ensure the event happens after current time.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            if (eventModel.OccursAtTick <= currentTick.GetTick())
            {
                return new SubmitGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // TODO: validate event.
            
            HashEntry[] entries = new[]
            {
                new HashEntry(eventModel.EventId, eventModel.ToByteArray()),
            };
            
            await RedisConnector.Redis.HashSetAsync(GameEventsKey(), entries);
            await RedisConnector.Redis.SetAddAsync(UserGameEventsKey(player), eventModel.EventId);
            return new SubmitGameEventResponse()
            {
                Success = true,
                EventId = eventModel.EventId,
            };
        }
        
        public async Task<DeleteGameEventResponse> RemovePlayerGameEvent(RedisUserModel player, string eventId)
        {
            Guid parsedGuid;
            try
            {
                parsedGuid = Guid.Parse(eventId);
            }
            catch (FormatException)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };
            }
            
            // Get the event to check some things...
            RedisValue eventData = await RedisConnector.Redis.HashGetAsync(GameEventsKey(), parsedGuid.ToString());
            if (!eventData.HasValue)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // Determine if the event has already passed.
            GameEventModel gameEvent = GameEventModel.Parser.ParseFrom(eventData);
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            
            if (gameEvent.OccursAtTick <= currentTick.GetTick() || gameEvent.IssuedBy != player.UserModel.Id)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // Determine if the event was issued by the player trying to delete the event.
            if (player.UserModel.Id != gameEvent.IssuedBy)
            {
                return new DeleteGameEventResponse()
                {
                    Success = false
                };;
            }
            
            // Remove game Event
            await RedisConnector.Redis.SetRemoveAsync(UserGameEventsKey(player), parsedGuid.ToString());
            await RedisConnector.Redis.HashDeleteAsync(GameEventsKey(), parsedGuid.ToString());
            return new DeleteGameEventResponse()
            {
                Success = true
            };
        }
        
        public async Task<List<GroupChatModel>> GetAllGroupChats()
        {
            // Get all group chats
            List<GroupChatModel> groupChatList = new List<GroupChatModel>();
            HashEntry[] groupChats = await RedisConnector.Redis.HashGetAllAsync(GameRoomChatListKey());
            foreach (var groupChat in groupChats)
            {
                groupChatList.Add(new GroupChatModel(this, groupChat.Value));
            }
            return groupChatList;
        }
        
        public async Task<List<GroupChatModel>> GetPlayerGroupChats(RedisUserModel user)
        {
            // Get all group chats
            List<GroupChatModel> groupChatList = new List<GroupChatModel>();
            HashEntry[] groupChats = await RedisConnector.Redis.HashGetAllAsync(GameRoomChatListKey());
            foreach (var groupChat in groupChats)
            {
                GroupChatModel model = new GroupChatModel(this, groupChat.Value);
                
                // Admins can see all group chats.
                if (user.HasClaim(UserClaim.Admin))
                {
                    groupChatList.Add(model);
                } else if(model.IsPlayerInGroup(user))
                {
                    groupChatList.Add(model);
                }
            }

            return groupChatList;
        }

        public async Task<GroupChatModel> GetGroupChatById(string groupChatId)
        {
            RedisValue groupChats = await RedisConnector.Redis.HashGetAsync(GameRoomChatListKey(), groupChatId);
            if (groupChats.HasValue)
            {
                return new GroupChatModel(this, groupChats);
            }

            return null;
        }

        public async Task<CreateMessageGroupResponse> CreateMessageGroup(List<String> groupMembers)
        {
            // Sort member list by id
            groupMembers.Sort((a, b) => String.Compare(a, b, StringComparison.Ordinal));

            // Get group hash
            string hash = "";
            foreach (var userId in groupMembers)
            {
                RedisUserModel user = await RedisUserModel.GetUserFromGuid(userId);
                if(user == null)
                    throw new RpcException(new Status(StatusCode.NotFound, "Player does not exist."));
                // Ensure the user is in the game room.
                if (await IsPlayerInRoom(user))
                {
                    hash += user.UserModel.Username;
                }
                else
                {
                    return new CreateMessageGroupResponse()
                    {
                        Success = false,
                    };
                }
            }
            
            // Determine if the Id exists.
            if (await RedisConnector.Redis.HashExistsAsync(GameRoomChatListKey(), hash))
            {
                return new CreateMessageGroupResponse()
                {
                    Success = false,
                };;
            }
            // Create the group
            GroupModel newGroup = new GroupModel();
            newGroup.GroupId = hash;
            foreach (string s in groupMembers)
            {
                RedisUserModel model = await RedisUserModel.GetUserFromGuid(s);
                if (model != null)
                {
                    newGroup.GroupMembers.Add(model.asUser());                        
                }
            }
            await RedisConnector.Redis.HashSetAsync(GameRoomChatListKey(), hash, newGroup.ToByteArray());
            return new CreateMessageGroupResponse()
            {
                GroupId = hash,
                Success = true,
            };
        }

        public async Task<GameEventModel> GetGameEventFromGuid(string eventId)
        {
            Guid parsedGuid;
            try
            {
                parsedGuid = Guid.Parse(eventId);
            }
            catch (FormatException)
            {
                return null;
            }
            
            RedisValue eventData = await RedisConnector.Redis.HashGetAsync(GameEventsKey(), parsedGuid.ToString());
            if (eventData.HasValue)
            {
                return GameEventModel.Parser.ParseFrom(eventData);
            }

            return null;
        }

        public async Task<List<GameEventModel>> GetAllGameEvents()
        {
            List<GameEventModel> events = new List<GameEventModel>();
            HashEntry[] eventHashes = await RedisConnector.Redis.HashGetAllAsync(GameEventsKey());
            foreach (var eventHash in eventHashes)
            {
                events.Add(GameEventModel.Parser.ParseFrom(eventHash.Value));
            }
            return events;
        }
        
        public async Task<List<GameEventModel>> GetAllPastGameEvents()
        {
            List<GameEventModel> events = await GetAllGameEvents();
            
            // Get current game tick
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            // Filter
            return events.FindAll(it => it.OccursAtTick <= currentTick.GetTick());
        }
        
        public async Task<Boolean> StartGame()
        {
            // Check that there is enough players to start early.
            if ((await GetPlayersInGame()).Count < 2)
                return false;
            
            RoomModel.RoomStatus = RoomStatus.Ongoing;
            RoomModel.UnixTimeStarted = DateTime.UtcNow.ToFileTimeUtc();
            await UpdateDatabase();
            await RedisConnector.Redis.SetRemoveAsync(OpenLobbiesKey(), RoomModel.RoomId);
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
            catch (FormatException)
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
        
        public async Task<Boolean> UpdateDatabase()
        {
            HashEntry[] roomRecord =
            {
                new HashEntry(RoomModel.RoomId, RoomModel.ToByteArray()),
            };
            await RedisConnector.Redis.HashSetAsync(GameRoomsHashKey(), roomRecord);

            if (RoomModel.RoomStatus == RoomStatus.Open)
            {
                await RedisConnector.Redis.SetAddAsync(OpenLobbiesKey(), RoomModel.RoomId);
            }

            return true;
        }

        public async Task<Boolean> CreateInDatabase()
        {
            HashEntry[] roomRecord =
            {
                new HashEntry(RoomModel.RoomId, RoomModel.ToByteArray()),
            };
            await RedisConnector.Redis.HashSetAsync(GameRoomsHashKey(), roomRecord);
            
            if (RoomModel.RoomStatus == RoomStatus.Open)
            {
                await RedisConnector.Redis.SetAddAsync(OpenLobbiesKey(), RoomModel.RoomId);
            }
            
            // Add the creator as a player in the game
            await JoinRoom(await RedisUserModel.GetUserFromGuid(RoomModel.CreatorId));
            
            return true;
        }

        public async Task<Room> asRoom()
        {
            // TODO: If the room is anonymous, hide the creator and player ids.
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
                MinutesPerTick = RoomModel.MinutesPerTick,
            };
            List<RedisUserModel> playersInGame = await GetPlayersInGame();
            room.Players.AddRange(playersInGame.ConvertAll(it => it.asUser()));
            room.AllowedSpecialists.AddRange(RoomModel.AllowedSpecialists);
            return room;
        }

        private string OpenLobbiesKey()
        {
            return "openlobbies";
        }

        private string GameRoomsHashKey()
        {
            return "games";
        }
        
        private string GameEventsKey()
        {
            return $"game:{RoomModel.RoomId}:events";
        }

        private string UserGameEventsKey(RedisUserModel user)
        {
            return $"game:{RoomModel.RoomId}:user:{user.UserModel.Id}:events";
        }

        private string GameRoomPlayerListKey()
        {
            return $"game:{RoomModel.RoomId}:players";
        }

        private string GameRoomChatListKey()
        {
            return $"game:{RoomModel.RoomId}:chats";
        }

        private string UserGameListKey(RedisUserModel user)
        {
            return $"users:{user.UserModel.Id}:games";
        }

        private GameEventModel toGameEventModel(RedisUserModel requestor, GameEventRequest request)
        {
            Guid eventId;
            eventId = Guid.NewGuid();
            
            return new GameEventModel()
            {
                EventId = eventId.ToString(),
                UnixTimeIssued =  DateTime.UtcNow.ToFileTimeUtc(),
                IssuedBy = requestor.UserModel.Id,
                OccursAtTick = request.OccursAtTick,
                EventData = request.EventData,
            };
        }
    }
}