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

        private RedisConnector redis;

        public SubterfugeServer(RedisConnector redis)
        {
            redis = redis;
        }
        
        public override Task<RoomDataResponse> GetRoomData(RoomDataRequest request, ServerCallContext context)
        {
            return base.GetRoomData(request, context);
        }

        public override Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request, ServerCallContext context)
        {
            return base.CreateNewRoom(request, context);
        }

        public override Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            return base.JoinRoom(request, context);
        }

        public override Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
        {
            return base.LeaveRoom(request, context);
        }

        public override Task<StartGameEarlyResponse> StartGameEarly(StartGameEarlyRequest request, ServerCallContext context)
        {
            return base.StartGameEarly(request, context);
        }

        public override async Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
                // Get list of event ids.
                RedisValue[] values = await RedisConnector.redis.ListRangeAsync($"game:{request.RoomId}:events:");
                foreach (RedisValue eventId in values)
                {
                    // Get the event.
                    RedisValue roomData = await RedisConnector.redis.StringGetAsync($"game:{request.RoomId}:events:{eventId.ToString()}");
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
                Guid eventId = Guid.NewGuid();
                request.EventData.EventId = eventId.ToString();
                // Remove game event from the game room.
                await RedisConnector.redis.StringSetAsync($"game:{request.RoomId}:events:{eventId}", request.EventData.ToByteArray());
                
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
                // Remove game event from the game room.
                await RedisConnector.redis.StringSetAsync($"game:{request.RoomId}:events:{request.EventData.EventId}", request.EventData.ToByteArray());
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
                await RedisConnector.redis.ListRemoveAsync($"game:{request.RoomId}:events", request.EventId);
                await RedisConnector.redis.KeyDeleteAsync($"game:{request.RoomId}:events{request.EventId}");
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
                await RedisConnector.redis.ListRightPushAsync($"game:{request.RoomId}:groups", group.ToByteArray());
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
                await RedisConnector.redis.ListRightPushAsync($"game:{request.RoomId}:groups:{request.RoomId}:messages", request.Message.ToByteArray());
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
                RedisValue[] roomValues = await RedisConnector.redis.ListRangeAsync($"game:{request.RoomId}:groups");
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
                await RedisConnector.redis.ListRightPushAsync($"user:{user.GetUserId()}:blocks", request.UserIdToBlock);
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
                await RedisConnector.redis.ListRemoveAsync($"user:{user.GetUserId()}:blocks", request.UserIdToBlock);
                
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
                // Add request to the other player.
                await RedisConnector.redis.ListRightPushAsync($"user:{request.FriendId}:friendRequests", user.GetUserId());
                return new SendFriendRequestResponse();
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
        }

        public override async Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user != null)
            {
                // TODO: Check user is in player's friend request list
                
                // Add both players as friends to each other.
                ITransaction transaction = RedisConnector.redis.CreateTransaction();
                // remove from friend requests
                transaction.ListRemoveAsync($"user:{user.GetUserId()}:friendRequests", request.FriendId);
                transaction.ListRightPushAsync($"user:{request.FriendId}:friends", user.GetUserId());
                transaction.ListRightPushAsync($"user:{user.GetUserId()}:friends", request.FriendId);
                await transaction.ExecuteAsync();
                return new AcceptFriendRequestResponse();
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
                ITransaction transaction = RedisConnector.redis.CreateTransaction();
                transaction.ListRemoveAsync($"user:{user.GetUserId()}:friends", request.FriendId);
                // Remove yourself from friend list
                transaction.ListRemoveAsync($"user:{request.FriendId}:friends", user.GetUserId());
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