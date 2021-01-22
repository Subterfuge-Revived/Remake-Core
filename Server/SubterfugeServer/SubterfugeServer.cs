using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using StackExchange.Redis;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;

namespace SubterfugeServerConsole
{
    public class SubterfugeServer : subterfugeService.subterfugeServiceBase
    {

        public override async Task<RoomDataResponse> GetRoomData(RoomDataRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                RoomDataResponse roomResponse = new RoomDataResponse();
                RedisValue[] roomIds = await RedisConnector.Redis.HashKeysAsync($"openlobbies");
                foreach(RedisValue roomId in roomIds)
                {
                    roomResponse.Rooms.Add(await RedisRoomModel.getRoom(Guid.Parse(roomId.ToString())));
                }
                return roomResponse;
            }
            
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                Guid roomId = Guid.NewGuid();
                
                // Push new room to redis.
                HashEntry[] roomData = new[]
                {
                    new HashEntry(roomId.ToString(), request.ToByteString().ToBase64()),
                };

                await RedisConnector.Redis.HashSetAsync($"openlobbies", roomData);
                await RedisConnector.Redis.HashSetAsync($"game:{roomId.ToString()}", roomData);
                return new CreateRoomResponse()
                {
                    CreatedRoom = new Room()
                    {
                        Creator = user.asUser(),
                        RoomId = roomId.ToString(),
                        RoomStatus = RoomStatus.Open,
                        MaxPlayers = request.MaxPlayers,
                    },
                };
            }
            
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                Room room = await RedisRoomModel.getRoom(Guid.Parse(request.RoomId));
                if (room != null)
                {
                    if (room.Players.Count < room.MaxPlayers)
                    {
                        // Check if this is the last player required to join.
                        if (room.Players.Count + 1 == room.MaxPlayers)
                        {
                            // Remove the game from the open lobby list.
                            await RedisConnector.Redis.HashDeleteAsync($"openlobbies", room.RoomId);
                            
                            // TODO: Start the game and update the room's started on date.
                        }
                        
                        // Add player to game player list
                        await RedisConnector.Redis.SetAddAsync($"room:{request.RoomId}:players", user.UserModel.Id);
                        
                        // Add game to player game lobbies
                        await RedisConnector.Redis.SetAddAsync($"user:{user.UserModel.Id}:games", request.RoomId);
                        
                        return new JoinRoomResponse()
                        {
                            Success = true
                        };
                    }
                    throw new RpcException(new Status(StatusCode.Unavailable, "The room is full."));
                }
                throw new RpcException(new Status(StatusCode.Unavailable, "Room does not exist."));
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                Room room = await RedisRoomModel.getRoom(Guid.Parse(request.RoomId));
                if (room != null)
                {
                    // Remove player from game player list
                    await RedisConnector.Redis.SetRemoveAsync($"room:{request.RoomId}:players", user.UserModel.Id);
                    
                    // Remove game to palyer game lobbies
                    await RedisConnector.Redis.SetRemoveAsync($"user:{user.UserModel.Id}:games", request.RoomId);
                    
                    return new LeaveRoomResponse()
                    {
                        Success = true
                    };
                }
                throw new RpcException(new Status(StatusCode.Unavailable, "Room does not exist."));
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<StartGameEarlyResponse> StartGameEarly(StartGameEarlyRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                Room room = await RedisRoomModel.getRoom(Guid.Parse(request.RoomId));
                if (room != null)
                {
                    // Remove lobby from open lobbies.
                    // From now on all players know the room id as it is in their game list.
                    await RedisConnector.Redis.HashDeleteAsync($"openlobbies", room.RoomId);
                    
                    // TODO: Set the start date and save.
                }
            }
            
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
                // Get list of event ids.
                RedisValue[] values = await RedisConnector.Redis.ListRangeAsync($"game:{request.RoomId}:events");
                foreach (RedisValue eventId in values)
                {
                    // Get the event.
                    RedisValue roomData = await RedisConnector.Redis.StringGetAsync($"game:{request.RoomId}:events:{eventId.ToString()}");
                    GameEvent gameEvent = GameEvent.Parser.ParseFrom(roomData);
                    response.GameEvents.Add(gameEvent);
                }

                return response;
            }
            
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // TODO: Validate event
                Guid eventId = Guid.NewGuid();
                request.EventData.EventId = eventId.ToString();
                // Remove game event from the game room.
                await RedisConnector.Redis.StringSetAsync($"game:{request.RoomId}:events:{eventId}", request.EventData.ToByteArray());
                await RedisConnector.Redis.ListRightPushAsync($"game:{request.RoomId}:events", eventId.ToString());
                
                return new SubmitGameEventResponse()
                {
                    Success = true,
                    EventId = eventId.ToString(),
                };
            }
            
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<UpdateGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // TODO: Validate event
                await RedisConnector.Redis.StringSetAsync($"game:{request.RoomId}:events:{request.EventData.EventId}", request.EventData.ToByteArray());
                return new UpdateGameEventResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<DeleteGameEventResponse> DeleteGameEvent(DeleteGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // Remove game event from the game room.
                await RedisConnector.Redis.ListRemoveAsync($"game:{request.RoomId}:events", request.EventId);
                await RedisConnector.Redis.KeyDeleteAsync($"game:{request.RoomId}:events:{request.EventId}");
                return new DeleteGameEventResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
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
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // Add player to your block list
                await RedisConnector.Redis.SetAddAsync($"user:{user.UserModel.Id}:blocks", request.UserIdToBlock);
                return new BlockPlayerResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            // Check if player is valid.
            
            if (user != null)
            {
                // Remove player from your block list
                await RedisConnector.Redis.SetRemoveAsync($"user:{user.UserModel.Id}:blocks", request.UserIdToBlock);
                
                return new UnblockPlayerResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(ViewBlockedPlayersRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            ViewBlockedPlayersResponse response = new ViewBlockedPlayersResponse();
            response.BlockedUsers.AddRange((await user.GetBlockedUsers()).ConvertAll(it => it.asUser()));
            return response;
        }

        public override async Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if (friend == null)
            {
                throw new RpcException(new Status(StatusCode.Unavailable, "The player does not exist."));
            }
            
            List<RedisUserModel> otherPlayerRequests = await friend.GetFriendRequests();
            if (otherPlayerRequests.Contains(user))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "You have already sent a request to this player."));
            }
            
            // Add request to the other player.
            await friend.AddFriendRequest(user);
            return new SendFriendRequestResponse();
        }

        public override async Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                throw new RpcException(new Status(StatusCode.Unavailable, "User does not exist."));
            
            List<RedisUserModel> friendRequests = await user.GetFriendRequests();
            
            if(!friendRequests.Contains(friend)) 
                throw new RpcException(new Status(StatusCode.OutOfRange, "Friend request does not exist."));

            await user.AddFriend(friend);
            await friend.AddFriend(user);
            await user.RemoveFriendRequest(friend);

            return new AcceptFriendRequestResponse();
        }

        public override async Task<ViewFriendRequestsResponse> ViewFriendRequests(ViewFriendRequestsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            ViewFriendRequestsResponse response = new ViewFriendRequestsResponse();
            List<User> users = (await user.GetFriendRequests()).ConvertAll(input => input.asUser());
            response.IncomingFriends.AddRange(users);
            return response;
        }

        public override async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                throw new RpcException(new Status(StatusCode.Unavailable, "User does not exist."));
                
            await user.RemoveFriend(friend);
            await friend.RemoveFriend(user);
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
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            }

            string token = JwtManager.GenerateToken(user.UserModel.Id);
            context.ResponseTrailers.Add("Authorization", token);
            return new AuthorizationResponse { Token = token, User = user.asUser() };
        }

        public override async Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest request,
            ServerCallContext context)
        {
            RedisUserModel user = await RedisUserModel.GetUserFromUsername(request.Username);
            if (user == null)
            {
                // Create a new user model
                RedisUserModel model = new RedisUserModel(request);
                await model.SaveToDatabase();
                string token = JwtManager.GenerateToken(model.UserModel.Id);
                context.ResponseTrailers.Add("Authorization", token);
                return new AccountRegistrationResponse { Token = token, User = model.asUser() };    
            }
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Username already exists."));
        }
    }
}