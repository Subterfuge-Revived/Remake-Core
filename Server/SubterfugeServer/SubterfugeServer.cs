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
                        Creator = new User() { Username = user.GetUsername(), Id = user.GetUserId() },
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
                        await RedisConnector.Redis.HashSetAsync($"room:{request.RoomId}:players",
                            new[] {new HashEntry(user.GetUserId(), user.GetUserId()),});
                        
                        // Add game to player game lobbies
                        await RedisConnector.Redis.HashSetAsync($"user:{user.GetUserId()}:games",
                            new[] {new HashEntry(request.RoomId, request.RoomId),});
                        
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
                    await RedisConnector.Redis.HashDeleteAsync($"room:{request.RoomId}:players", user.GetUserId());
                    
                    // Remove game to palyer game lobbies
                    await RedisConnector.Redis.HashDeleteAsync($"user:{user.GetUserId()}:games", request.RoomId);
                    
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
                    RedisUserModel model = await RedisUserModel.getUser(Guid.Parse(s));
                    if (model != null)
                    {
                        group.GroupMembers.Add(new User
                        {
                            Id = model.GetUserId(),
                            Username = model.GetUsername()
                        });                        
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
                    if (group != null && group.GroupMembers.Any(it => it.Id == user.GetUserId()))
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
                await RedisConnector.Redis.SetAddAsync($"user:{user.GetUserId()}:blocks", request.UserIdToBlock);
                return new BlockPlayerResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // Remove player from your block list
                await RedisConnector.Redis.SetRemoveAsync($"user:{user.GetUserId()}:blocks", request.UserIdToBlock);
                
                return new UnblockPlayerResponse();
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(ViewBlockedPlayersRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            ViewBlockedPlayersResponse response = new ViewBlockedPlayersResponse();
            if (user != null)
            {
                foreach (RedisUserModel blockedRedisUser in await user.GetBlockedUsers())
                {
                    User blockedUser = new User()
                    {
                        Id = blockedRedisUser.GetUserId(),
                        Username = blockedRedisUser.GetUsername()
                    };
                    response.BlockedUsers.Add(blockedUser);
                }
                return response;
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // Check the requested user is valid.
                try
                {
                    Guid friendId = Guid.Parse(request.FriendId);
                }
                catch (FormatException e)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid player Id."));
                }
                
                RedisUserModel friend = await RedisUserModel.getUser(Guid.Parse(request.FriendId));
                if (friend != null)
                {
                    // Check if the user already sent a friend request.
                    if (!await RedisConnector.Redis.SetContainsAsync($"user:{request.FriendId}:friendRequests",
                        user.GetUserId()))
                    {
                        // Add request to the other player.
                        await RedisConnector.Redis.SetAddAsync($"user:{request.FriendId}:friendRequests",
                            user.GetUserId());
                        return new SendFriendRequestResponse();
                    }
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "You have already sent a request to this player."));
                }
                throw new RpcException(new Status(StatusCode.Unavailable, "The player does not exist."));
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                if (await RedisConnector.Redis.SetContainsAsync($"user:{user.GetUserId()}:friendRequests", request.FriendId))
                {
                    if (await RedisConnector.Redis.SetRemoveAsync($"user:{user.GetUserId()}:friendRequests",
                        request.FriendId))
                    {
                        // Add both players as friends to each other.
                        ITransaction transaction = RedisConnector.Redis.CreateTransaction();
                        // remove from friend requests
                        transaction.SetRemoveAsync($"user:{user.GetUserId()}:friendRequests", request.FriendId);
                        transaction.SetAddAsync($"user:{request.FriendId}:friends", user.GetUserId());
                        transaction.SetAddAsync($"user:{user.GetUserId()}:friends", request.FriendId);

                        await transaction.ExecuteAsync();
                        return new AcceptFriendRequestResponse();
                    }
                }
                throw new RpcException(new Status(StatusCode.OutOfRange, "Friend request does not exist."));
            }

            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<ViewFriendRequestsResponse> ViewFriendRequests(ViewFriendRequestsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            ViewFriendRequestsResponse response = new ViewFriendRequestsResponse();
            if (user != null)
            {
                foreach (RedisUserModel friendRequest in await user.GetFriendRequests())
                {
                    User rpcUser = new User
                    {
                        Id = friendRequest.GetUserId(),
                        Username = friendRequest.GetUsername(),
                    };
                    response.IncomingFriends.Add(rpcUser);
                }
                return response;
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // Remove friend from your list
                ITransaction transaction = RedisConnector.Redis.CreateTransaction();
                transaction.HashDeleteAsync($"user:{user.GetUserId()}:friends", request.FriendId);
                // Remove yourself from friend list
                transaction.HashDeleteAsync($"user:{request.FriendId}:friends", user.GetUserId());
                await transaction.ExecuteAsync();
                return new RemoveFriendResponse();
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<ViewFriendsResponse> ViewFriends(ViewFriendsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            ViewFriendsResponse response = new ViewFriendsResponse();
            if (user != null)
            {
                foreach (RedisUserModel userFriend in await user.GetFriends())
                {
                    User rpcUser = new User
                    {
                        Id = userFriend.GetUserId(),
                        Username = userFriend.GetUsername(),
                    };
                    response.Friends.Add(rpcUser);
                }

                return response;
            }
            
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
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
            RedisUserModel user = await RedisUserModel.getUser(request.Username);
            
            if (user == null || user.GetPassword() != request.Password)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            }

            string token = JwtManager.GenerateToken(user.GetUserId());
            context.ResponseTrailers.Add("Authorization", token);
            return new AuthorizationResponse {Token = token, User = new User {Id = user.GetUserId(), Username = user.GetUsername()}};
        }

        public override async Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest request,
            ServerCallContext context)
        {
            RedisUserModel user = await RedisUserModel.getUser(request.Username);
            if (user == null)
            {
                // Create a new user model
                RedisUserModel model = RedisUserModel.newBuilder()
                    .setEmail(request.Email)
                    .setPassword(request.Password)
                    .setUsername(request.Username)
                    .Build();

                // Save the new user
                await model.saveUser();
                string token = JwtManager.GenerateToken(model.GetUserId());
                context.ResponseTrailers.Add("Authorization", token);
                return new AccountRegistrationResponse {Token = token, User = new User {Id = model.GetUserId(), Username = model.GetUsername()}};    
            }
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Username already exists."));
        }
    }
}