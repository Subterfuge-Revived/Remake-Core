using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;

namespace SubterfugeServerConsole
{
    public class SubterfugeServer : subterfugeService.subterfugeServiceBase
    {
        public override async Task<OpenLobbiesResponse> GetOpenLobbies(OpenLobbiesRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new OpenLobbiesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            OpenLobbiesResponse roomResponse = new OpenLobbiesResponse();
            List<Room> rooms = (await DatabaseRoomModel.GetOpenLobbies()).ConvertAll(it => it.asRoom().Result);
            roomResponse.Rooms.AddRange(rooms);
            roomResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            
            return roomResponse;
        }

        public override async Task<PlayerCurrentGamesResponse> GetPlayerCurrentGames(PlayerCurrentGamesRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new PlayerCurrentGamesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            PlayerCurrentGamesResponse currentGameResponse = new PlayerCurrentGamesResponse();
            List<Room> rooms = (await user.GetActiveRooms()).ConvertAll(it => it.asRoom().Result);
            currentGameResponse.Games.AddRange(rooms);
            currentGameResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return currentGameResponse;
        }

        public override async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new CreateRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Ensure max players is over 1
            if(request.MaxPlayers < 2)
                return new CreateRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
                
            
            DatabaseRoomModel roomModel = new DatabaseRoomModel(request, user);
            await roomModel.CreateInDatabase();
                
               
            return new CreateRoomResponse()
            {
                CreatedRoom = await roomModel.asRoom(),
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }

        public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new JoinRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new JoinRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            return new JoinRoomResponse()
            {
                Status = await room.JoinRoom(user)
            };
        }

        public override async Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new LeaveRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new LeaveRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            return new LeaveRoomResponse()
            {
                Status = await room.LeaveRoom(user)
            };
        }

        public override async Task<StartGameEarlyResponse> StartGameEarly(StartGameEarlyRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new StartGameEarlyResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new StartGameEarlyResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            if (room.RoomModel.CreatorId == user.UserModel.Id)
            {
                return new StartGameEarlyResponse()
                {
                    Status = await room.StartGame(),
                };
            }
            return new StartGameEarlyResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED),
            };
        }

        public override async Task<GetGameRoomEventsResponse> GetGameRoomEvents(GetGameRoomEventsRequest request, ServerCallContext context)
        {
            
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new GetGameRoomEventsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new GetGameRoomEventsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            List<DatabaseUserModel> playersInGame = await room.GetPlayersInGame();
            if (playersInGame.All(it => it.UserModel.Id != user.UserModel.Id) && !user.HasClaim(UserClaim.Admin))
                return new GetGameRoomEventsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };
            
            List<GameEventModel> events = await room.GetAllGameEvents();
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
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }

        
        public override async Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            if(!await room.IsPlayerInRoom(user))
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };

            return await room.AddPlayerGameEvent(user, request.EventData);
        }

        public override async Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            // GameEventToUpdate.
            return await room.UpdateGameEvent(user, request);
        }

        public override async Task<DeleteGameEventResponse> DeleteGameEvent(DeleteGameEventRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            return await room.RemovePlayerGameEvent(user, request.EventId);
        }

        public override async Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            if(room.RoomModel.RoomStatus != RoomStatus.Ongoing)
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED),
                };
            
            if(!request.UserIdsInGroup.Contains(user.UserModel.Id))
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };

            return await room.CreateMessageGroup(request.UserIdsInGroup.ToList());
        }

        public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            GroupChatModel groupChat = await room.GetGroupChatById(request.GroupId);
            if(groupChat == null)
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST)
                };
            
            if(!groupChat.IsPlayerInGroup(user))
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };

            return new SendMessageResponse()
            {
                Status = await groupChat.SendChatMessage(user, request.Message)
            };
        }

        public override async Task<GetMessageGroupsResponse> GetMessageGroups(GetMessageGroupsRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new GetMessageGroupsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new GetMessageGroupsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            List<GroupChatModel> groupChats = await room.GetPlayerGroupChats(user);
            GetMessageGroupsResponse response = new GetMessageGroupsResponse();
            foreach (var groupModel in groupChats)
            {
                response.MessageGroups.Add(await groupModel.asMessageGroup());
            }

            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }

        public override async Task<GetGroupMessagesResponse> GetGroupMessages(GetGroupMessagesRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new GetGroupMessagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            DatabaseRoomModel room = await DatabaseRoomModel.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new GetGroupMessagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            GroupChatModel groupChat = await room.GetGroupChatById(request.GroupId);
            if(groupChat == null)
                return new GetGroupMessagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST)
                };
            
            if(!groupChat.IsPlayerInGroup(user))
                return new GetGroupMessagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };

            return new GetGroupMessagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                Group = await groupChat.asMessageGroup(request.Pagination)
            };
        }

        public override async Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new BlockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseUserModel friend = await DatabaseUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                return new BlockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(await user.IsBlocked(friend))
                return new BlockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
                };

            await user.BlockUser(friend);
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }

        public override async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new UnblockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Check if player is valid.
            DatabaseUserModel friend = await DatabaseUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                return new UnblockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            // Check if player is blocked.
            if(!await user.IsBlocked(friend))
                return new UnblockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };

            await user.UnblockUser(friend);
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }

        public override async Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(ViewBlockedPlayersRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new ViewBlockedPlayersResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            ViewBlockedPlayersResponse response = new ViewBlockedPlayersResponse();
            response.BlockedUsers.AddRange((await user.GetBlockedUsers()).ConvertAll(it => it.asUser()));
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }

        public override async Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseUserModel friend = await DatabaseUserModel.GetUserFromGuid(request.FriendId);
            if (friend == null)
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(await friend.HasFriendRequestFrom(user))
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
                };
            
            if(await friend.IsBlocked(user) || await user.IsBlocked(friend))
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_IS_BLOCKED)
                };
            
            // Add request to the other player.
            await friend.AddFriendRequestFrom(user);
            return new SendFriendRequestResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }

        public override async Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new AcceptFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseUserModel friend = await DatabaseUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                return new AcceptFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };

            if(!await user.HasFriendRequestFrom(friend))
                return new AcceptFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.FRIEND_REQUEST_DOES_NOT_EXIST)
                };

            return new AcceptFriendRequestResponse()
            {
                Status = await user.AcceptFriendRequestFrom(friend),
            };
        }

        public override async Task<ViewFriendRequestsResponse> ViewFriendRequests(ViewFriendRequestsRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new ViewFriendRequestsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            ViewFriendRequestsResponse response = new ViewFriendRequestsResponse();
            List<User> users = (await user.GetFriendRequests()).ConvertAll(input => input.asUser());
            response.IncomingFriends.AddRange(users);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }
        
        public override async Task<DenyFriendRequestResponse> DenyFriendRequest(DenyFriendRequestRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new DenyFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseUserModel friend = await DatabaseUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                return new DenyFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(!await user.HasFriendRequestFrom(friend))
                return new DenyFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.FRIEND_REQUEST_DOES_NOT_EXIST)
                };

            return new DenyFriendRequestResponse()
            {
                Status = await user.RemoveFriendRequestFrom(friend),
            };
        }

        public override async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new RemoveFriendResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DatabaseUserModel friend = await DatabaseUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                return new RemoveFriendResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(!await user.IsFriend(friend))
                return new RemoveFriendResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
                
            await user.RemoveFriend(friend);
            return new RemoveFriendResponse();
        }

        public override async Task<ViewFriendsResponse> ViewFriends(ViewFriendsRequest request, ServerCallContext context)
        {
            DatabaseUserModel user = context.UserState["user"] as DatabaseUserModel;
            if(user == null)
                return new ViewFriendsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            ViewFriendsResponse response = new ViewFriendsResponse();
            response.Friends.AddRange((await user.GetFriends()).ConvertAll(input => input.asUser()));
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }

        public override Task<HealthCheckResponse> HealthCheck(HealthCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HealthCheckResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            });
        }

        public override Task<AuthorizedHealthCheckResponse> AuthorizedHealthCheck(AuthorizedHealthCheckRequest request, ServerCallContext context)
        {
            return Task.FromResult(new AuthorizedHealthCheckResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            });
        }

        public override async Task<AuthorizationResponse> Login(AuthorizationRequest request, ServerCallContext context)
        {
            // Try to get a user
            DatabaseUserModel user = await DatabaseUserModel.GetUserFromUsername(request.Username);
            
            if (user == null || !JwtManager.VerifyPasswordHash(request.Password, user.UserModel.PasswordHash))
                return new AuthorizationResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_CREDENTIALS)
                };

            string token = JwtManager.GenerateToken(user.UserModel.Id);
            context.ResponseTrailers.Add("Authorization", token);
            return new AuthorizationResponse
            {
                Token = token,
                User = user.asUser(),
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }

        public override async Task<AuthorizationResponse> LoginWithToken(AuthorizedTokenRequest request, ServerCallContext context)
        {
            if(string.IsNullOrEmpty(request.Token))
                return new AuthorizationResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_CREDENTIALS)
                };

            if (JwtManager.ValidateToken(request.Token, out var uuid))
            {
                // Validate user exists.
                DatabaseUserModel user = await DatabaseUserModel.GetUserFromGuid(uuid);
                if (user != null)
                {
                    context.UserState["user"] = user;
                    return new AuthorizationResponse()
                    {
                        Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                        Token = request.Token,
                        User = user.asUser(),
                    };
                }
            }
            return new AuthorizationResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.INVALID_CREDENTIALS)
            };
        }

        public override async Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest request,
            ServerCallContext context)
        {
            DatabaseUserModel user = await DatabaseUserModel.GetUserFromUsername(request.Username);
            if(user != null)
                return new AccountRegistrationResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
                };
            
            // Create a new user model
            DatabaseUserModel model = new DatabaseUserModel(request);
            await model.SaveToDatabase();
            string token = JwtManager.GenerateToken(model.UserModel.Id);
            context.ResponseTrailers.Add("Authorization", token);
            return new AccountRegistrationResponse
            {
                Token = token,
                User = model.asUser(),
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };    
        }

        public override async Task<SubmitCustomSpecialistResponse> SubmitCustomSpecialist(SubmitCustomSpecialistRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                return new SubmitCustomSpecialistResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            // Set author
            request.Configuration.Creator = user.asUser();
            SpecialistConfigurationModel configModel = new SpecialistConfigurationModel(request.Configuration);
            await configModel.saveToRedis();

            // Get the generated specialist ID
            string specialistId = configModel.SpecialistConfig.SpecialistId;

            return new SubmitCustomSpecialistResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                SpecialistConfigurationId = specialistId,
            };
        }

        public override async Task<GetCustomSpecialistsResponse> GetCustomSpecialists(GetCustomSpecialistsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                return new GetCustomSpecialistsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Search through all specialists for the search term.
            List<SpecialistConfigurationModel> results = (await SpecialistConfigurationModel.search(request.SearchTerm)).Skip((int)request.PageNumber * 50).Take(50).ToList();

            GetCustomSpecialistsResponse response = new GetCustomSpecialistsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
            
            foreach (SpecialistConfigurationModel model in results)
            {
                response.CustomSpecialists.Add(model.SpecialistConfig);   
            }

            return response;
        }

        public override async Task<GetPlayerCustomSpecialistsResponse> GetPlayerCustomSpecialists(GetPlayerCustomSpecialistsRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                return new GetPlayerCustomSpecialistsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Get the requested user from their id
            RedisUserModel player = await RedisUserModel.GetUserFromGuid(request.PlayerId);
            if (player == null)
            {
                return new GetPlayerCustomSpecialistsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            }
            List<SpecialistConfigurationModel> results = await player.GetSpecialistConfigurations();

            GetPlayerCustomSpecialistsResponse response = new GetPlayerCustomSpecialistsResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
            
            foreach (SpecialistConfigurationModel model in results)
            {
                response.PlayerSpecialists.Add(model.SpecialistConfig);   
            }

            return response;
        }

        public override async Task<CreateSpecialistPackageResponse> CreateSpecialistPackage(CreateSpecialistPackageRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                return new CreateSpecialistPackageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            // Set author
            request.SpecialistPackage.Creator = user.asUser();
            SpecialistPackageModel packageModel = new SpecialistPackageModel(request.SpecialistPackage);
            await packageModel.saveToRedis();

            // Get the generated specialist ID
            string packageId = packageModel.SpecialistPackage.SpecialistPackageId;

            return new CreateSpecialistPackageResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                SpecialistPackageId = packageId,
            };
        }

        public override async Task<GetSpecialistPackagesResponse> GetSpecialistPackages(GetSpecialistPackagesRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                return new GetSpecialistPackagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Search through all specialists for the search term.
            List<SpecialistPackageModel> results = (await SpecialistPackageModel.search(request.SearchTerm)).Skip((int)request.PageNumber * 50).Take(50).ToList();

            GetSpecialistPackagesResponse response = new GetSpecialistPackagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
            
            foreach (SpecialistPackageModel model in results)
            {
                response.SpecialistPackages.Add(model.SpecialistPackage);   
            }

            return response;
        }

        public override async Task<GetPlayerSpecialistPackagesResponse> GetPlayerSpecialistPackages(GetPlayerSpecialistPackagesRequest request, ServerCallContext context)
        {
            RedisUserModel user = context.UserState["user"] as RedisUserModel;
            if(user == null)
                return new GetPlayerSpecialistPackagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Get the requested user from their id
            RedisUserModel player = await RedisUserModel.GetUserFromGuid(request.PlayerId);
            if (player == null)
            {
                return new GetPlayerSpecialistPackagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            }
            List<SpecialistPackageModel> results = await player.GetSpecialistPackages();

            GetPlayerSpecialistPackagesResponse response = new GetPlayerSpecialistPackagesResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
            
            foreach (SpecialistPackageModel model in results)
            {
                response.PlayerPackages.Add(model.SpecialistPackage);   
            }

            return response;
        }

    }
}