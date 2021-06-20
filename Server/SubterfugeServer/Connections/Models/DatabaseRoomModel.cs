using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using MongoDB.Driver;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class DatabaseRoomModel
    {

        public RoomModel RoomModel;

        public DatabaseRoomModel(RoomModel model)
        {
            RoomModel = model;
        }

        public DatabaseRoomModel(CreateRoomRequest request, DatabaseUserModel creator)
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

        public async Task<ResponseStatus> JoinRoom(DatabaseUserModel userModel)
        {
            if (IsRoomFull())
                return ResponseFactory.createResponse(ResponseType.ROOM_IS_FULL);
            
            if(IsPlayerInRoom(userModel))
                return ResponseFactory.createResponse(ResponseType.DUPLICATE);
            
            if(RoomModel.RoomStatus != RoomStatus.Open)
                return ResponseFactory.createResponse(ResponseType.GAME_ALREADY_STARTED);
            
            List<DatabaseUserModel> playersInRoom = new List<DatabaseUserModel>();
            foreach (string userId in RoomModel.PlayersInGame)
            {
                playersInRoom.Add(await DatabaseUserModel.GetUserFromGuid(userId));
            }
            
            // Check if any players in the room have the same device identifier
            if(playersInRoom.Any(it => it.UserModel.DeviceIdentifier == userModel.UserModel.DeviceIdentifier))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);

            RoomModel.PlayersInGame.Add(userModel.UserModel.Id);
            MongoConnector.getGameRoomCollection().ReplaceOne((it => it.RoomId == RoomModel.RoomId), RoomModel);
            
            // Check if the player joining the game is the last player.
            if (RoomModel.PlayersInGame.Count == RoomModel.MaxPlayers)
                return await StartGame();
            
            return ResponseFactory.createResponse(ResponseType.SUCCESS);;
        }

        public Boolean IsRoomFull()
        {
            return RoomModel.PlayersInGame.Count >= RoomModel.MaxPlayers;
        }
        
        public Boolean IsPlayerInRoom(DatabaseUserModel player)
        {
            return RoomModel.PlayersInGame.Contains(player.UserModel.Id);
        }

        public async Task<ResponseStatus> LeaveRoom(DatabaseUserModel userModel)
        {
            if (RoomModel.RoomStatus == RoomStatus.Open)
            {
                // Check if the player leaving was the host.
                if (RoomModel.CreatorId == userModel.UserModel.Id)
                {
                    // Delete the game
                    MongoConnector.getGameRoomCollection().DeleteOne(it => it.RoomId == RoomModel.RoomId);
                    return ResponseFactory.createResponse(ResponseType.SUCCESS);
                }

                // Otherwise, just remove the player from the player list.
                RoomModel.PlayersInGame.Remove(userModel.UserModel.Id);
                MongoConnector.getGameRoomCollection().ReplaceOne((it => it.RoomId == RoomModel.RoomId), RoomModel);
                return ResponseFactory.createResponse(ResponseType.SUCCESS);
            }
            // TODO: Player left the game while ongoing.
            // Create a player leave game event and push to event list

            return ResponseFactory.createResponse(ResponseType.INVALID_REQUEST);
        }

        public async Task<List<GameEventModel>> GetPlayerGameEvents(DatabaseUserModel player)
        {
            List<GameEventModel> events = MongoConnector.getGameEventCollection()
                .Find(it => it.IssuedBy == player.UserModel.Id)
                .ToList();
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            return events;
        }

        public async Task<SubmitGameEventResponse> UpdateGameEvent(DatabaseUserModel player, UpdateGameEventRequest request)
        {
            GameEventModel gameEvent = await GetGameEventFromGuid(request.EventId);
            if (gameEvent == null)
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST)
                };
            }
            
            if (gameEvent.IssuedBy != player.UserModel.Id)
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };
            }

            // Ensure the event happens after current time.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            if (request.EventData.OccursAtTick <= currentTick.GetTick())
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
            }
            
            // TODO: validate event.
            
            // Overwrite data with request data.
            gameEvent.EventData = request.EventData.EventData;
            gameEvent.OccursAtTick = request.EventData.OccursAtTick;
            
            MongoConnector.getGameEventCollection().ReplaceOne((it => it.RoomId == RoomModel.RoomId), gameEvent);
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                EventId = gameEvent.EventId,
            }; 
        }

        public async Task<SubmitGameEventResponse> AddPlayerGameEvent(DatabaseUserModel player,  GameEventRequest request)
        {
            GameEventModel eventModel = toGameEventModel(player, request);

            // Ensure the event happens after current time.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            if (eventModel.OccursAtTick <= currentTick.GetTick())
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST),
                };;
            }
            
            // TODO: validate event.
            
            MongoConnector.getGameEventCollection().InsertOne(eventModel);
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                EventId = eventModel.EventId,
            };
        }
        
        public async Task<DeleteGameEventResponse> RemovePlayerGameEvent(DatabaseUserModel player, string eventId)
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
                    Status = ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST)
                };
            }

            // Get the event to check some things...
            GameEventModel gameEvent = MongoConnector.getGameEventCollection().Find(it => it.EventId == eventId).FirstOrDefault();
            if (gameEvent == null)
            {
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST)
                };;
            }
            
            // Determine if the event has already passed.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            
            if (gameEvent.OccursAtTick <= currentTick.GetTick())
            {
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
            }

            if (gameEvent.IssuedBy != player.UserModel.Id)
            {
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };
            }

            // Remove game Event
            MongoConnector.getGameEventCollection().DeleteOne(it => it.EventId == eventId);
            return new DeleteGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
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
        
        public async Task<List<GroupChatModel>> GetPlayerGroupChats(DatabaseUserModel user)
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
                DatabaseUserModel user = await DatabaseUserModel.GetUserFromGuid(userId);
                if(user == null)
                    return new CreateMessageGroupResponse()
                    {
                        Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST),
                    };
                // Ensure the user is in the game room.
                if (await IsPlayerInRoom(user))
                {
                    hash += user.UserModel.Username;
                }
                else
                {
                    return new CreateMessageGroupResponse()
                    {
                        Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED),
                    };
                }
            }
            
            // Determine if the Id exists.
            if (await RedisConnector.Redis.HashExistsAsync(GameRoomChatListKey(), hash))
            {
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.DUPLICATE),
                };;
            }
            // Create the group
            GroupModel newGroup = new GroupModel();
            newGroup.GroupId = hash;
            foreach (string s in groupMembers)
            {
                DatabaseUserModel model = await DatabaseUserModel.GetUserFromGuid(s);
                if (model != null)
                {
                    newGroup.GroupMembers.Add(model.asUser());                        
                }
            }
            await RedisConnector.Redis.HashSetAsync(GameRoomChatListKey(), hash, newGroup.ToByteArray());
            return new CreateMessageGroupResponse()
            {
                GroupId = hash,
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }

        public async Task<GameEventModel> GetGameEventFromGuid(string eventId)
        {
            return MongoConnector.getGameEventCollection().Find(it => it.EventId == eventId).FirstOrDefault();
        }

        public async Task<List<GameEventModel>> GetAllGameEvents()
        {
            return MongoConnector.getGameEventCollection().Find(it => it.RoomId == RoomModel.RoomId).ToList();
        }
        
        public async Task<List<GameEventModel>> GetAllPastGameEvents()
        {

            List<GameEventModel> events = MongoConnector.getGameEventCollection().Find(it => it.RoomId == RoomModel.RoomId).ToList();
            
            // Get current game tick
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(RoomModel.UnixTimeStarted), DateTime.UtcNow);
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            return events.FindAll(it => it.OccursAtTick <= currentTick.GetTick());
        }
        
        public async Task<ResponseStatus> StartGame()
        {
            // Check that there is enough players to start early.
            if (RoomModel.PlayersInGame.Count < 2)
                return ResponseFactory.createResponse(ResponseType.INVALID_REQUEST);
            
            var update = Builders<RoomModel>.Update.Set(it => it.RoomStatus, RoomStatus.Ongoing);
            MongoConnector.getGameRoomCollection().UpdateOne((it => it.RoomId == RoomModel.RoomId), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public static async Task<List<DatabaseRoomModel>> GetOpenLobbies()
        {
            List<DatabaseRoomModel> rooms = MongoConnector.getGameRoomCollection()
                .Find(it => it.RoomStatus == RoomStatus.Open)
                .ToList()
                .Select(it => new DatabaseRoomModel(it))
                .ToList();
            return rooms;
        }
        

        public static async Task<DatabaseRoomModel> GetRoomFromGuid(string roomGuid)
        {
            RoomModel room = MongoConnector.getGameRoomCollection()
                .Find((it => it.RoomId == roomGuid))
                .FirstOrDefault();
            if (room != null)
            {
                return new DatabaseRoomModel(room);
            }

            return null;
        }
        
        public async Task<Boolean> UpdateDatabase()
        {
            // TODO

            return true;
        }

        public async Task<Boolean> CreateInDatabase()
        {
            MongoConnector.getGameRoomCollection().InsertOne(RoomModel);
            return true;
        }

        public async Task<Room> asRoom()
        {
            // TODO: If the room is anonymous, hide the creator and player ids.
            Room room = new Room()
            {
                RoomId = RoomModel.RoomId,
                RoomStatus = RoomModel.RoomStatus,
                Creator = (await DatabaseUserModel.GetUserFromGuid(RoomModel.CreatorId)).asUser(),
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

            List<DatabaseUserModel> playersInGame = new List<DatabaseUserModel>();
            foreach (string playerId in RoomModel.PlayersInGame)
            {
                playersInGame.Add(await DatabaseUserModel.GetUserFromGuid(playerId));
            }
            
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

        private string UserGameEventsKey(DatabaseUserModel user)
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

        private string UserGameListKey(DatabaseUserModel user)
        {
            return $"users:{user.UserModel.Id}:games";
        }

        private GameEventModel toGameEventModel(DatabaseUserModel requestor, GameEventRequest request)
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
                RoomId = RoomModel.RoomId,
            };
        }
    }
}