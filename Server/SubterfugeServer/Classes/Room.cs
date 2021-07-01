using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole.Connections.Models
{
    public class Room
    {

        public RoomModel RoomModel;

        public Room(RoomModel model)
        {
            RoomModel = model;
        }

        public Room(CreateRoomRequest request, SubterfugeRemakeService.User creator)
        {
            Guid roomId = Guid.NewGuid();
            RoomModel = new RoomModel()
            {
                Anonymous = request.Anonymous,
                CreatorId = creator.Id,
                Goal = request.Goal,
                MaxPlayers = request.MaxPlayers,
                RankedInformation = new RankedInformation()
                {
                    IsRanked = request.IsRanked,
                    MinRating = 500,
                    MaxRating = 1500, // TODO: +-100 of user rank
                },
                Id = roomId.ToString(),
                RoomName = request.RoomName,
                RoomStatus = RoomStatus.Open,
                Seed = new Random().Next(),
                UnixTimeCreated =  DateTime.UtcNow.ToFileTimeUtc(),
                UnixTimeStarted = 0,
                MinutesPerTick = request.MinutesPerTick,
                PlayersInGame = { creator.Id }
            };
            RoomModel.AllowedSpecialists.AddRange(request.AllowedSpecialists);
            GameTick.MINUTES_PER_TICK = RoomModel.MinutesPerTick;
        }

        public async Task<ResponseStatus> JoinRoom(DbUserModel dbUserModel)
        {
            if (IsRoomFull())
                return ResponseFactory.createResponse(ResponseType.ROOM_IS_FULL);
            
            if(IsPlayerInRoom(dbUserModel))
                return ResponseFactory.createResponse(ResponseType.DUPLICATE);
            
            if(RoomModel.RoomStatus != RoomStatus.Open)
                return ResponseFactory.createResponse(ResponseType.GAME_ALREADY_STARTED);
            
            List<DbUserModel> playersInRoom = new List<DbUserModel>();
            foreach (string userId in RoomModel.PlayersInGame)
            {
                playersInRoom.Add(await DbUserModel.GetUserFromGuid(userId));
            }
            
            // Check if any players in the room have the same device identifier
            if(playersInRoom.Any(it => it.UserModel.DeviceIdentifier == dbUserModel.UserModel.DeviceIdentifier))
                return ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED);

            RoomModel.PlayersInGame.Add(dbUserModel.UserModel.Id);
            MongoConnector.GetGameRoomCollection().ReplaceOne((it => it.Id == RoomModel.Id), new RoomModelMapper(RoomModel));
            
            // Check if the player joining the game is the last player.
            if (RoomModel.PlayersInGame.Count == RoomModel.MaxPlayers)
                return await StartGame();
            
            return ResponseFactory.createResponse(ResponseType.SUCCESS);;
        }

        public Boolean IsRoomFull()
        {
            return RoomModel.PlayersInGame.Count >= RoomModel.MaxPlayers;
        }
        
        public Boolean IsPlayerInRoom(DbUserModel player)
        {
            return RoomModel.PlayersInGame.Contains(player.UserModel.Id);
        }

        public async Task<ResponseStatus> LeaveRoom(DbUserModel dbUserModel)
        {
            if (RoomModel.RoomStatus == RoomStatus.Open)
            {
                // Check if the player leaving was the host.
                if (RoomModel.CreatorId == dbUserModel.UserModel.Id)
                {
                    // Delete the game
                    MongoConnector.GetGameRoomCollection().DeleteOne(it => it.Id == RoomModel.Id);
                    return ResponseFactory.createResponse(ResponseType.SUCCESS);
                }

                // Otherwise, just remove the player from the player list.
                RoomModel.PlayersInGame.Remove(dbUserModel.UserModel.Id);
                MongoConnector.GetGameRoomCollection().ReplaceOne((it => it.Id == RoomModel.Id), new RoomModelMapper(RoomModel));
                return ResponseFactory.createResponse(ResponseType.SUCCESS);
            }
            // TODO: Player left the game while ongoing.
            // Create a player leave game event and push to event list

            return ResponseFactory.createResponse(ResponseType.INVALID_REQUEST);
        }

        public async Task<List<GameEventModel>> GetPlayerGameEvents(DbUserModel player)
        {
            List<GameEventModel> events = MongoConnector.GetGameEventCollection()
                .Find(it => it.IssuedBy == player.UserModel.Id)
                .ToList()
                .Select(it => it.ToProto())
                .ToList();;
            events.Sort((a, b) => a.OccursAtTick.CompareTo(b.OccursAtTick));
            return events;
        }

        public async Task<SubmitGameEventResponse> UpdateGameEvent(DbUserModel player, UpdateGameEventRequest request)
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
            
            MongoConnector.GetGameEventCollection().ReplaceOne((it => it.RoomId == RoomModel.Id), new GameEventModelMapper(gameEvent));
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                EventId = gameEvent.Id,
            }; 
        }

        public async Task<SubmitGameEventResponse> AddPlayerGameEvent(DbUserModel player,  GameEventRequest request)
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
            
            MongoConnector.GetGameEventCollection().InsertOne(new GameEventModelMapper(eventModel));
            return new SubmitGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                EventId = eventModel.Id,
            };
        }
        
        public async Task<DeleteGameEventResponse> RemovePlayerGameEvent(DbUserModel player, string eventId)
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
            GameEventModelMapper gameEvent = MongoConnector.GetGameEventCollection().Find(it => it.Id == eventId).FirstOrDefault();
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
            MongoConnector.GetGameEventCollection().DeleteOne(it => it.Id == eventId);
            return new DeleteGameEventResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }
        
        public async Task<List<GroupChat>> GetAllGroupChats()
        {
            // Get all group chats
            List<GroupChat> groupChatList = MongoConnector.GetMessageGroupCollection()
                .Find(group => group.RoomId == RoomModel.Id)
                .ToList()
                .Select(group => new GroupChat(this, group.ToProto()))
                .ToList();
            return groupChatList;
        }
        
        public async Task<List<GroupChat>> GetPlayerGroupChats(DbUserModel dbUserModel)
        {
            // Get all group chats the player is in
            List<GroupChat> groupChatList = MongoConnector.GetMessageGroupCollection()
                .Find(group => group.RoomId == RoomModel.Id && group.GroupMembers.Contains(dbUserModel.UserModel.Id))
                .ToList()
                .Select(group => new GroupChat(this, group.ToProto()))
                .ToList();
            return groupChatList;
        }

        public async Task<GroupChat> GetGroupChatById(string groupChatId)
        {
            // Get all group chats the player is in
            GroupChat groupChat = MongoConnector.GetMessageGroupCollection()
                .Find(group => group.Id == groupChatId)
                .ToList()
                .Select(group => new GroupChat(this, group.ToProto()))
                .FirstOrDefault();
            return groupChat;
        }

        public async Task<CreateMessageGroupResponse> CreateMessageGroup(List<String> groupMembers)
        {
            // Ensure all members are in the room.
            if (!groupMembers.Except(RoomModel.PlayersInGame).Any())
            {
                // A player in the group is not in the game.
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST),
                };;
            }

            // Get all group chats for the room
            List<GroupChat> roomGroupChats = MongoConnector.GetMessageGroupCollection()
                .Find(group => group.RoomId == RoomModel.Id)
                .ToList()
                .Select(group => new GroupChat(this, group.ToProto()))
                .ToList();

            foreach (GroupChat groupChat in roomGroupChats)
            {
                if (groupChat.MessageGroup.GroupMembers.Count() == groupMembers.Count())
                {
                    // If the group chats are the same size and removing all the duplicates results in an empty list,
                    // We have a duplicate group
                    if (!groupChat.MessageGroup.GroupMembers.Except(groupMembers).Any())
                    {
                        return new CreateMessageGroupResponse()
                        {
                            Status = ResponseFactory.createResponse(ResponseType.DUPLICATE),
                        };
                    }
                }
            }
            
            // Otherwise, create the group
            GroupModel newGroup = new GroupModel();
            newGroup.Id = Guid.NewGuid().ToString();
            foreach (string s in groupMembers)
            {
                DbUserModel model = await DbUserModel.GetUserFromGuid(s);
                if (model != null)
                {
                    newGroup.GroupMembers.Add(model.UserModel.Id);                        
                }
            }
            
            MongoConnector.GetMessageGroupCollection().InsertOne(new GroupModelMapper(newGroup));
            
            return new CreateMessageGroupResponse()
            {
                GroupId = newGroup.Id,
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }

        public async Task<GameEventModel> GetGameEventFromGuid(string eventId)
        {
            GameEventModelMapper mapper = MongoConnector.GetGameEventCollection().Find(it => it.Id == eventId).FirstOrDefault();
            if (mapper == null)
            {
                return null;
            }
            return mapper.ToProto();
        }

        public async Task<List<GameEventModel>> GetAllGameEvents()
        {
            return MongoConnector.GetGameEventCollection()
                .Find(it => it.RoomId == RoomModel.Id)
                .ToList()
                .Select(it => it.ToProto())
                .ToList();;
        }
        
        public async Task<List<GameEventModel>> GetAllPastGameEvents()
        {

            List<GameEventModel> events = MongoConnector.GetGameEventCollection()
                .Find(it => it.RoomId == RoomModel.Id)
                .ToList()
                .Select(it => it.ToProto())
                .ToList();
            
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
            
            var update = Builders<RoomModelMapper>.Update
                .Set(it => it.RoomStatus, RoomStatus.Ongoing)
                .Set(it => it.UnixTimeStarted, DateTime.UtcNow.ToFileTimeUtc());
            MongoConnector.GetGameRoomCollection().UpdateOne((it => it.Id == RoomModel.Id), update);
            return ResponseFactory.createResponse(ResponseType.SUCCESS);
        }

        public static async Task<List<Room>> GetOpenLobbies()
        {
            List<Room> rooms = MongoConnector.GetGameRoomCollection()
                .Find(it => it.RoomStatus == RoomStatus.Open)
                .ToList()
                .Select(it => new Room(it.ToProto()))
                .ToList();
            return rooms;
        }
        

        public static async Task<Room> GetRoomFromGuid(string roomGuid)
        {
            RoomModelMapper room = MongoConnector.GetGameRoomCollection()
                .Find((it => it.Id == roomGuid))
                .FirstOrDefault();
            if (room != null)
            {
                return new Room(room.ToProto());
            }

            return null;
        }

        public async Task<Boolean> CreateInDatabase()
        {
            MongoConnector.GetGameRoomCollection().InsertOne(new RoomModelMapper(RoomModel));
            return true;
        }

        public async Task<SubterfugeRemakeService.Room> asRoom()
        {
            // TODO: If the room is anonymous, hide the creator and player ids.
            SubterfugeRemakeService.Room room = new SubterfugeRemakeService.Room()
            {
                RoomId = RoomModel.Id,
                RoomStatus = RoomModel.RoomStatus,
                Creator = (await DbUserModel.GetUserFromGuid(RoomModel.CreatorId)).asUser(),
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

            List<DbUserModel> playersInGame = new List<DbUserModel>();
            foreach (string playerId in RoomModel.PlayersInGame)
            {
                playersInGame.Add(await DbUserModel.GetUserFromGuid(playerId));
            }
            
            room.Players.AddRange(playersInGame.ConvertAll(it => it.asUser()));
            room.AllowedSpecialists.AddRange(RoomModel.AllowedSpecialists);
            return room;
        }

        private GameEventModel toGameEventModel(DbUserModel requestor, GameEventRequest request)
        {
            Guid eventId;
            eventId = Guid.NewGuid();
            
            return new GameEventModel()
            {
                Id = eventId.ToString(),
                UnixTimeIssued =  DateTime.UtcNow.ToFileTimeUtc(),
                IssuedBy = requestor.UserModel.Id,
                OccursAtTick = request.OccursAtTick,
                EventData = request.EventData,
                RoomId = RoomModel.Id,
            };
        }
    }
}