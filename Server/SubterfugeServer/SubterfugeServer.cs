using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using StackExchange.Redis;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole
{
    public class SubterfugeServer : subterfugeService.subterfugeServiceBase
    {
        public override async Task<OpenLobbiesResponse> GetOpenLobbies(OpenLobbiesRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            OpenLobbiesResponse roomResponse = new OpenLobbiesResponse();
            List<Room> rooms = (await RedisRoomModel.GetOpenLobbies()).ConvertAll(it => it.asRoom().Result);
            roomResponse.Rooms.AddRange(rooms);
            
            return roomResponse;
        }

        public override async Task<PlayerCurrentGamesResponse> GetPlayerCurrentGames(PlayerCurrentGamesRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            PlayerCurrentGamesResponse currentGameResponse = new PlayerCurrentGamesResponse();
            List<Room> rooms = (await user.GetActiveRooms()).ConvertAll(it => it.asRoom().Result);
            currentGameResponse.Games.AddRange(rooms);
            
            return currentGameResponse;
        }

        public override async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            // Ensure max players is over 1
            if(request.MaxPlayers < 2)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Cannot create a room with less than 2 players."));
                
            
            RedisRoomModel roomModel = new RedisRoomModel(request, user);
            roomModel.CreateInDatabase();
                
               
            return new CreateRoomResponse()
            {
                CreatedRoom = await roomModel.asRoom()
            };
        }

        public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Room does not exist."));

            if (await room.JoinRoom(user))
            {
                return new JoinRoomResponse()
                {
                    Success = true
                };
            }
            throw new RpcException(new Status(StatusCode.Unavailable, "The room is full."));
        }

        public override async Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Room does not exist."));

            if (await room.LeaveRoom(user))
            {
                return new LeaveRoomResponse()
                {
                    Success = true
                };
            }
            throw new RpcException(new Status(StatusCode.Internal, "Unknown Error."));
        }

        public override async Task<StartGameEarlyResponse> StartGameEarly(StartGameEarlyRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Room does not exist."));

            if (room.RoomModel.CreatorId == user.UserModel.Id)
            {
                if (await room.StartGame())
                {
                    return new StartGameEarlyResponse()
                    {
                        Success = true
                    };
                }
                return new StartGameEarlyResponse()
                {
                    Success = false
                };
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Only the host can start the game early."));
        }

        public override async Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request, ServerCallContext context)
        {
            
            RedisUserModel user = GetUserContext(context);
            
            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Room does not exist."));

            List<RedisUserModel> playersInGame = await room.GetPlayersInGame();
            if (playersInGame.All(it => it.UserModel.Id != user.UserModel.Id) && !user.HasClaim(UserClaim.Admin))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Cannot view events for a game you are not in."));
            
            List<GameEvent> events = await room.GetAllGameEvents();
            // Filter out only the player's events and events that have occurred in the past.
            // Get current tick to determine events in the past.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(room.RoomModel.UnixTimeStarted), DateTime.UtcNow);
            
            // Admins see all events :)
            if (!user.HasClaim(UserClaim.Admin))
            {
                events = events.FindAll(it =>
                    it.OccursAtTick <= currentTick.GetTick() || it.IssuedBy == user.UserModel.Id);
            }

            GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
            response.GameEvents.AddRange(events);
            return response;
        }

        
        public override async Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            if(user.UserModel.Id != request.EventData.IssuedBy)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "You cannot issues commands for another player."));

            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game room does not exist."));
            
            if(!await room.IsPlayerInRoom(user))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "You cannot submit events to a game you are not a member of."));

            return await room.AddPlayerGameEvent(user, request.EventData);
        }

        public override async Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            if(user.UserModel.Id != request.EventData.IssuedBy)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "You cannot issues commands for another player."));

            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game room does not exist."));

            return await room.AddPlayerGameEvent(user, request.EventData);
        }

        public override async Task<DeleteGameEventResponse> DeleteGameEvent(DeleteGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);

            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game room does not exist."));

            return await room.RemovePlayerGameEvent(user, request.EventId);
        }

        public override async Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // Add player to your block list
                MessageGroup group = new MessageGroup();
                group.GroupId = Guid.NewGuid().ToString();
                foreach (string s in request.UserIdsInGroup)
                {
                    RedisUserModel model = await RedisUserModel.GetUserFromGuid(s);
                    if (model != null)
                    {
                        group.GroupMembers.Add(model.asUser());                        
                    }
                }
                await RedisConnector.Redis.ListRightPushAsync($"game:{request.RoomId}:groups", group.ToByteArray());
                return new CreateMessageGroupResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // Add player to your block list
                await RedisConnector.Redis.ListRightPushAsync($"game:{request.RoomId}:groups:{request.RoomId}:messages", request.Message.ToByteArray());
                return new SendMessageResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<GetMessageGroupsResponse> GetMessageGroups(GetMessageGroupsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            GetMessageGroupsResponse response = new GetMessageGroupsResponse();
            if (user != null)
            {
                // Add player to your block list
                RedisValue[] roomValues = await RedisConnector.Redis.ListRangeAsync($"game:{request.RoomId}:groups");
                foreach (RedisValue roomValue in roomValues)
                {
                    MessageGroup group = MessageGroup.Parser.ParseFrom(roomValue);
                    if (group != null && group.GroupMembers.Any(it => it.Id == user.UserModel.Id))
                    {
                        // player is in the group.
                        response.MessageGroups.Add(group);
                    }
                }
                return response;
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                throw new RpcException(new Status(StatusCode.NotFound, "The user cannot be found."));
            
            if(await user.IsBlocked(friend))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "You have already blocked this player."));

            await user.BlockUser(friend);
            return new BlockPlayerResponse();
        }

        public override async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            // Check if player is valid.
            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                throw new RpcException(new Status(StatusCode.NotFound, "The user cannot be found."));
            
            // Check if player is blocked.
            if(!await user.IsBlocked(friend))
                throw new RpcException(new Status(StatusCode.NotFound, "You do not have this player blocked."));

            await user.UnblockUser(friend);
            return new UnblockPlayerResponse();
        }

        public override async Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(ViewBlockedPlayersRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            ViewBlockedPlayersResponse response = new ViewBlockedPlayersResponse();
            response.BlockedUsers.AddRange((await user.GetBlockedUsers()).ConvertAll(it => it.asUser()));
            return response;
        }

        public override async Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if (friend == null)
                throw new RpcException(new Status(StatusCode.NotFound, "The player does not exist."));
            
            if(await friend.HasFriendRequestFrom(user))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "You have already sent a request to this player."));
            
            if(await friend.IsBlocked(user) || await user.IsBlocked(friend))
                throw new RpcException(new Status(StatusCode.Unavailable, "Player is blocked."));
            
            // Add request to the other player.
            await friend.AddFriendRequestFrom(user);
            return new SendFriendRequestResponse();
        }

        public override async Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User does not exist."));
            
            if(!await user.HasFriendRequestFrom(friend))
                throw new RpcException(new Status(StatusCode.NotFound, "Friend request does not exist."));

            await user.AcceptFriendRequestFrom(friend);

            return new AcceptFriendRequestResponse();
        }

        public override async Task<ViewFriendRequestsResponse> ViewFriendRequests(ViewFriendRequestsRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);
            
            ViewFriendRequestsResponse response = new ViewFriendRequestsResponse();
            List<User> users = (await user.GetFriendRequests()).ConvertAll(input => input.asUser());
            response.IncomingFriends.AddRange(users);
            return response;
        }
        
        public override async Task<DenyFriendRequestResponse> DenyFriendRequest(DenyFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User does not exist."));
            
            if(!await user.HasFriendRequestFrom(friend))
                throw new RpcException(new Status(StatusCode.NotFound, "Friend request does not exist."));

            await user.RemoveFriendRequestFrom(friend);

            return new DenyFriendRequestResponse();
        }

        public override async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
        {
            RedisUserModel user = GetUserContext(context);

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User does not exist."));
            
            if(!await user.IsFriend(friend))
                throw new RpcException(new Status(StatusCode.NotFound, "You are not friends with this player."));
                
            await user.RemoveFriend(friend);
            return new RemoveFriendResponse();
        }

        public override async Task<ViewFriendsResponse> ViewFriends(ViewFriendsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            ViewFriendsResponse response = new ViewFriendsResponse();
            response.Friends.AddRange((await user.GetFriends()).ConvertAll(input => input.asUser()));
            
            return response;
        }

        public override Task<HealthCheckResponse> HealthCheck(HealthCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HealthCheckResponse());
        }

        public override Task<AuthorizedHealthCheckResponse> AuthorizedHealthCheck(AuthorizedHealthCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AuthorizedHealthCheckResponse());
        }

        public override async Task<AuthorizationResponse> Login(AuthorizationRequest request, ServerCallContext context)
        {
            // Try to get a user
            RedisUserModel user = await RedisUserModel.GetUserFromUsername(request.Username);
            
            if (user == null || !JwtManager.VerifyPasswordHash(request.Password, user.UserModel.PasswordHash))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));

            string token = JwtManager.GenerateToken(user.UserModel.Id);
            context.ResponseTrailers.Add("Authorization", token);
            return new AuthorizationResponse { Token = token, User = user.asUser() };
        }

        public override async Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest request,
            ServerCallContext context)
        {
            RedisUserModel user = await RedisUserModel.GetUserFromUsername(request.Username);
            if(user != null)
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Username already exists."));
            
            // Create a new user model
            RedisUserModel model = new RedisUserModel(request);
            await model.SaveToDatabase();
            string token = JwtManager.GenerateToken(model.UserModel.Id);
            context.ResponseTrailers.Add("Authorization", token);
            return new AccountRegistrationResponse { Token = token, User = model.asUser() };    
        }

        private RedisUserModel GetUserContext(ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            return user;
        }
    }
}