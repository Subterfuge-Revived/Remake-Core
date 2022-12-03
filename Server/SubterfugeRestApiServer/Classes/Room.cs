using MongoDB.Driver;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class Room
    {

        public GameConfiguration GameConfiguration;

        public Room(GameConfiguration model)
        {
            GameConfiguration = model;
        }

        public Room(CreateRoomRequest request, User creator)
        {
            Guid roomId = Guid.NewGuid();
            GameConfiguration = new GameConfiguration()
            {
                Id = roomId.ToString(),
                RoomStatus = request.IsPrivate ? RoomStatus.Private : RoomStatus.Open,
                RoomName = request.RoomName,
                UnixTimeCreated =  DateTime.UtcNow.ToFileTimeUtc(),
                UnixTimeStarted = 0,
                Creator = creator,

                GameSettings = request.GameSettings,
                MapConfiguration = request.MapConfiguration,
            };
            GameConfiguration.PlayersInLobby.Add(creator);
            GameTick.MinutesPerTick = request.GameSettings.MinutesPerTick;
        }

        public async Task<ResponseStatus> JoinRoom(DbUserModel dbUserModel)
        {
            if (IsRoomFull())
                return ResponseFactory.createResponse(ResponseType.ROOM_IS_FULL);
            
            if(IsPlayerInRoom(dbUserModel))
                return ResponseFactory.createResponse(ResponseType.DUPLICATE);
            
            if(GameConfiguration.RoomStatus != RoomStatus.Open)
                return ResponseFactory.createResponse(ResponseType.GAME_ALREADY_STARTED);
            
            List<DbUserModel?> playersInRoom = new List<DbUserModel?>();
            foreach (User user in GameConfiguration.PlayersInLobby)
            {
                playersInRoom.Add(await DbUserModel.GetUserFromGuid(user.Id));
            }
            
            // Check if any players in the room have the same device identifier
            if(playersInRoom.Any(it => it.UserModel.DeviceIdentifier == dbUserModel.UserModel.DeviceIdentifier))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);

            GameConfiguration.PlayersInLobby.Add(dbUserModel.AsUser());
            MongoConnector.GetGameRoomCollection().ReplaceOne((it => it.Id == GameConfiguration.Id), GameConfiguration);
            
            // Check if the player joining the game is the last player.
            if (GameConfiguration.PlayersInLobby.Count == GameConfiguration.GameSettings.MaxPlayers)
                return await StartGame();
            
            return ResponseFactory.createResponse(ResponseType.SUCCESS);;
        }

        public Boolean IsRoomFull()
        {
            return GameConfiguration.PlayersInLobby.Count >= GameConfiguration.GameSettings.MaxPlayers;
        }
        
        public Boolean IsPlayerInRoom(DbUserModel player)
        {
            return GameConfiguration.PlayersInLobby.Contains(player.AsUser());
        }

        public async Task<ResponseStatus> LeaveRoom(DbUserModel dbUserModel)
        {
            if (GameConfiguration.RoomStatus == RoomStatus.Open)
            {
                // Check if the player leaving was the host.
                if (GameConfiguration.Creator.Id == dbUserModel.UserModel.Id)
                {
                    // Delete the game
                    await MongoConnector.GetGameRoomCollection().DeleteOneAsync(it => it.Id == GameConfiguration.Id);
                    return ResponseFactory.createResponse(ResponseType.SUCCESS);
                }

                // Otherwise, just remove the player from the player list.
                GameConfiguration.PlayersInLobby.Remove(dbUserModel.AsUser());
                await MongoConnector.GetGameRoomCollection().ReplaceOneAsync((it => it.Id == GameConfiguration.Id), GameConfiguration);
                return ResponseFactory.createResponse(ResponseType.SUCCESS);
            }
            // TODO: Player left the game while ongoing.
            // Create a player leave game event and push to event list

            return ResponseFactory.createResponse(ResponseType.INVALID_REQUEST);
        }

        public async Task<List<GameEventData>> GetPlayerGameEvents(DbUserModel player)
        {
            List<GameEventData> events = (await MongoConnector.GetGameEventCollection()
                .FindAsync(it => it.IssuedBy.Id == player.UserModel.Id))
                .ToList();
            
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            return events;
        }

        public async Task<SubmitGameEventResponse> UpdateGameEvent(DbUserModel player, UpdateGameEventRequest request)
        {
            GameEventData? gameEvent = await GetGameEventFromGuid(request.EventId);
            if (gameEvent == null)
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST)
                };
            }
            
            if (gameEvent.IssuedBy.Id != player.UserModel.Id)
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };
            }

            // Ensure the event happens after current time.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(GameConfiguration.UnixTimeStarted), DateTime.UtcNow);
            if (request.GameEventRequest.OccursAtTick <= currentTick.GetTick())
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
            }
            
            // TODO: validate event.
            
            // Overwrite data with request data.
            gameEvent.EventData = request.GameEventRequest.EventData;
            gameEvent.OccursAtTick = request.GameEventRequest.OccursAtTick;
            
            MongoConnector.GetGameEventCollection().ReplaceOne((it => it.RoomId == GameConfiguration.Id), gameEvent);
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                EventId = gameEvent.Id,
            }; 
        }

        public async Task<SubmitGameEventResponse> AddPlayerGameEvent(DbUserModel player,  GameEventRequest request)
        {
            GameEventData eventModel = toGameEventModel(player, request);

            // Ensure the event happens after current time.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(GameConfiguration.UnixTimeStarted), DateTime.UtcNow);
            if (eventModel.OccursAtTick <= currentTick.GetTick())
            {
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST),
                };;
            }
            
            // TODO: validate event.
            
            await MongoConnector.GetGameEventCollection().InsertOneAsync(new GameEventData());
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                EventId = eventModel.Id,
            };
        }
        
        public async Task<DeleteGameEventResponse> RemovePlayerGameEvent(DbUserModel player, string eventId)
        {
            try
            {
                Guid.Parse(eventId);
            }
            catch (FormatException)
            {
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST)
                };
            }

            // Get the event to check some things...
            GameEventData gameEvent = (await MongoConnector.GetGameEventCollection()
                .FindAsync(it => it.Id == eventId))
                .ToList()
                .FirstOrDefault();
            
            if (gameEvent == null)
            {
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.GAME_EVENT_DOES_NOT_EXIST)
                };;
            }
            
            // Determine if the event has already passed.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(GameConfiguration.UnixTimeStarted), DateTime.UtcNow);
            
            if (gameEvent.OccursAtTick <= currentTick.GetTick())
            {
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
            }

            if (gameEvent.IssuedBy.Id != player.UserModel.Id)
            {
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };
            }

            // Remove game Event
            MongoConnector.GetGameEventCollection().DeleteOne(it => it.Id == eventId);
            return new DeleteGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }
        
        public async Task<List<GroupChat>> GetAllGroupChats()
        {
            // Get all group chats
            List<GroupChat> groupChatList = (await MongoConnector.GetMessageGroupCollection()
                .FindAsync(group => group.RoomId == GameConfiguration.Id))
                .ToList()
                .Select(group => new GroupChat(this, group))
                .ToList();
            return groupChatList;
        }
        
        public async Task<List<GroupChat>> GetPlayerGroupChats(DbUserModel dbUserModel)
        {
            // Get all group chats the player is in
            List<GroupChat> groupChatList = (await MongoConnector.GetMessageGroupCollection()
                .FindAsync(group => 
                    group.RoomId == GameConfiguration.Id &&
                    group.MemberIds.Contains(dbUserModel.UserModel.Id)))
                .ToList()
                .Select(group => new GroupChat(this, group))
                .ToList();
            
            return groupChatList;
        }

        public async Task<GroupChat> GetGroupChatById(string groupChatId)
        {
            // Get all group chats the player is in
            return (await MongoConnector.GetMessageGroupCollection()
                .FindAsync(group => group.Id == groupChatId))
                .ToList()
                .Select(group => new GroupChat(this, group))
                .FirstOrDefault();
        }

        public async Task<CreateMessageGroupResponse> CreateMessageGroup(List<String> groupMembers)
        {
            // Ensure all members are in the room.
            if (groupMembers.Except(GameConfiguration.PlayersInLobby.Select(it => it.Id)).Any())
            {
                // A player in the group is not in the game.
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST),
                };;
            }

            // Get all group chats for the room
            List<GroupChat> roomGroupChats = (await MongoConnector.GetMessageGroupCollection()
                .FindAsync(group => group.RoomId == GameConfiguration.Id))
                .ToList()
                .Select(group => new GroupChat(this, group))
                .ToList();

            foreach (GroupChat groupChat in roomGroupChats)
            {
                if (groupChat.MessageGroup.MemberIds.Count() == groupMembers.Count())
                {
                    // If the group chats are the same size and removing all the duplicates results in an empty list,
                    // We have a duplicate group
                    if (!groupChat.MessageGroup.MemberIds.Except(groupMembers).Any())
                    {
                        return new CreateMessageGroupResponse()
                        {
                            Status = ResponseFactory.createResponse(ResponseType.DUPLICATE),
                        };
                    }
                }
            }
            
            // Otherwise, create the group
            MessageGroup newGroup = new MessageGroup();
            newGroup.Id = Guid.NewGuid().ToString();
            newGroup.RoomId = GameConfiguration.Id;
            foreach (string s in groupMembers)
            {
                DbUserModel? model = await DbUserModel.GetUserFromGuid(s);
                if (model != null)
                {
                    newGroup.GroupMembers.Add(model.UserModel.ToUser());                        
                }
            }
            
            await MongoConnector.GetMessageGroupCollection().InsertOneAsync(newGroup.ToDatabaseModel());
            
            return new CreateMessageGroupResponse()
            {
                GroupId = newGroup.Id,
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }

        public async Task<GameEventData?> GetGameEventFromGuid(string eventId)
        {
            var mapper = (await MongoConnector.GetGameEventCollection()
                .FindAsync(it => it.Id == eventId))
                .ToList()
                .FirstOrDefault();
            
            return mapper == null ? null : mapper;
        }

        public async Task<List<GameEventData>> GetAllGameEvents()
        {
            return (await MongoConnector.GetGameEventCollection()
                .FindAsync(it => it.RoomId == GameConfiguration.Id))
                .ToList();
        }
        
        public async Task<List<GameEventData>> GetAllPastGameEvents()
        {

            var events = (await MongoConnector.GetGameEventCollection()
                .FindAsync(it => it.RoomId == GameConfiguration.Id))
                .ToList();
            
            // Get current game tick
            var currentTick = new GameTick(DateTime.FromFileTimeUtc(GameConfiguration.UnixTimeStarted), DateTime.UtcNow);
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            return events.FindAll(it => it.OccursAtTick <= currentTick.GetTick());
        }
        
        public async Task<ResponseStatus> StartGame()
        {
            // Check that there is enough players to start early.
            if (GameConfiguration.PlayersInLobby.Count < 2)
                return ResponseFactory.createResponse(ResponseType.INVALID_REQUEST);

            var update = Builders<GameConfiguration>.Update
                .Set(it => it.RoomStatus, RoomStatus.Ongoing)
                .Set(it => it.UnixTimeStarted, DateTime.UtcNow.ToFileTimeUtc())
                .Set(it => it.GameSettings.MaxPlayers, GameConfiguration.PlayersInLobby.Count);
            await MongoConnector.GetGameRoomCollection().UpdateOneAsync((it => it.Id == GameConfiguration.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public static async Task<List<Room>> GetOpenLobbies()
        {
            return (await MongoConnector.GetGameRoomCollection()
                .FindAsync(it => it.RoomStatus == RoomStatus.Open))
                .ToList()
                .Select(it => new Room(it))
                .ToList();
        }
        

        public static async Task<Room> GetRoomFromGuid(string roomGuid)
        {
            var room = (await MongoConnector.GetGameRoomCollection()
                .FindAsync(it => it.Id == roomGuid))
                .FirstOrDefault();
            return room != null ? new Room(room) : null;
        }

        public async Task<Boolean> CreateInDatabase()
        {
            await MongoConnector.GetGameRoomCollection().InsertOneAsync(GameConfiguration);
            return true;
        }
        
        private GameEventData toGameEventModel(DbUserModel requestor, GameEventRequest request)
        {
            Guid eventId;
            eventId = Guid.NewGuid();
            
            return new GameEventData()
            {
                Id = eventId.ToString(),
                UnixTimeIssued =  DateTime.UtcNow.ToFileTimeUtc(),
                IssuedBy = requestor.UserModel.ToUser(),
                OccursAtTick = request.OccursAtTick,
                EventData = request.EventData,
                RoomId = GameConfiguration.Id,
            };
        }
    }
}