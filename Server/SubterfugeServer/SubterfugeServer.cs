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
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            RoomDataResponse roomResponse = new RoomDataResponse();
            List<Room> rooms = (await RedisRoomModel.GetOpenLobbies()).ConvertAll(it => it.asRoom().Result);
            roomResponse.Rooms.AddRange(rooms);
            
            return roomResponse;
        }

        public override async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            RedisRoomModel roomModel = new RedisRoomModel(request, user);
            roomModel.SaveToDatabase();
                
               
            return new CreateRoomResponse()
            {
                CreatedRoom = await roomModel.asRoom()
            };
        }

        public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
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
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
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
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Room does not exist."));

            if (room.RoomModel.CreatorId == user.UserModel.Id)
            {
                await room.StartGameEarly();
                return new StartGameEarlyResponse()
                {
                    Success = true
                };
            }
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Only the host can start the game early."));
        }

        public override async Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request, ServerCallContext context)
        {
            
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Room does not exist."));
            
            List<RedisUserModel> playersInGame = new List<RedisUserModel>();
            if (playersInGame.All(it => it.UserModel.Id != user.UserModel.Id))
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Cannot view events for a game you are not in."));
            
            List<GameEvent> events = await room.GetAllGameEvents();
            GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
            response.GameEvents.AddRange(events);
            return response;
        }

        
        public override async Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            if(user.UserModel.Id != request.EventData.IssuedBy.Id)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "You cannot issues commands for another player."));

            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game room does not exist."));

            return await room.AddPlayerGameEvent(user, request.EventData);
        }

        public override async Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            if(user.UserModel.Id != request.EventData.IssuedBy.Id)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "You cannot issues commands for another player."));

            RedisRoomModel room = await RedisRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                throw new RpcException(new Status(StatusCode.NotFound, "Game room does not exist."));

            return await room.AddPlayerGameEvent(user, request.EventData);
        }

        public override async Task<DeleteGameEventResponse> DeleteGameEvent(DeleteGameEventRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            if(user.UserModel.Id != request.EventId)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "You cannot delete commands for another player."));

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
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                throw new RpcException(new Status(StatusCode.NotFound, "The user cannot be found."));
            
            
            // Check if player is blocked.
            List<RedisUserModel> blockedUsers = await user.GetBlockedUsers();
            if (blockedUsers.Any(it => it.UserModel.Id == friend.UserModel.Id)) 
                throw new RpcException(new Status(StatusCode.AlreadyExists, "You have already blocked this player."));

            await user.BlockUser(friend);
            return new BlockPlayerResponse();
        }

        public override async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if (user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));
            
            // Check if player is valid.
            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                throw new RpcException(new Status(StatusCode.NotFound, "The user cannot be found."));
            
            // Check if player is blocked.
            List<RedisUserModel> blockedUsers = await user.GetBlockedUsers();
            if (blockedUsers.All(it => it.UserModel.Id != friend.UserModel.Id)) 
                throw new RpcException(new Status(StatusCode.NotFound, "You do not have this player blocked."));

            await user.UnblockUser(friend);
            return new UnblockPlayerResponse();
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
                throw new RpcException(new Status(StatusCode.NotFound, "The player does not exist."));
            
            List<RedisUserModel> otherPlayerRequests = await friend.GetFriendRequests();
            if (otherPlayerRequests.Any(it => it.UserModel.Id == user.UserModel.Id))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "You have already sent a request to this player."));
            
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
                throw new RpcException(new Status(StatusCode.NotFound, "User does not exist."));
            
            List<RedisUserModel> friendRequests = await user.GetFriendRequests();
            
            if(friendRequests.All(it => it.UserModel.Id != friend.UserModel.Id)) 
                throw new RpcException(new Status(StatusCode.NotFound, "Friend request does not exist."));

            await user.AcceptFriendRequest(friend);

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
        
        public override async Task<DenyFriendRequestResponse> DenyFriendRequest(DenyFriendRequestRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User does not exist."));
            
            List<RedisUserModel> friendRequests = await user.GetFriendRequests();
            
            if(friendRequests.All(it => it.UserModel.Id != friend.UserModel.Id)) 
                throw new RpcException(new Status(StatusCode.NotFound, "Friend request does not exist."));

            await user.RemoveFriendRequest(friend);

            return new DenyFriendRequestResponse();
        }

        public override async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid Credentials."));

            RedisUserModel friend = await RedisUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                throw new RpcException(new Status(StatusCode.NotFound, "User does not exist."));
                
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