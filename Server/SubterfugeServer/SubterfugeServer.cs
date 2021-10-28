using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;
using Room = SubterfugeServerConsole.Connections.Models.Room;

namespace SubterfugeServerConsole
{
    public class SubterfugeServer : subterfugeService.subterfugeServiceBase
    {
        
        public override Task<GetRolesResponse> GetRoles(GetRolesRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return Task.FromResult(new GetRolesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                });
            var response = new GetRolesResponse();
            response.Claims.AddRange(dbUserModel.UserModel.Claims);
            return Task.FromResult(response);
        }

        public override async Task<OpenLobbiesResponse> GetOpenLobbies(OpenLobbiesRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new OpenLobbiesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            OpenLobbiesResponse roomResponse = new OpenLobbiesResponse();
            List<SubterfugeRemakeService.GameConfiguration> rooms = (await Room.GetOpenLobbies()).Select(it => it.GameConfiguration).ToList();
            roomResponse.Rooms.AddRange(rooms);
            roomResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            
            return roomResponse;
        }

        public override async Task<PlayerCurrentGamesResponse> GetPlayerCurrentGames(PlayerCurrentGamesRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new PlayerCurrentGamesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            PlayerCurrentGamesResponse currentGameResponse = new PlayerCurrentGamesResponse();
            List<GameConfiguration> rooms = (await dbUserModel.GetActiveRooms()).Select(it => it.GameConfiguration).ToList();
            currentGameResponse.Games.AddRange(rooms);
            currentGameResponse.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return currentGameResponse;
        }

        public override async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new CreateRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Ensure max players is over 1
            if(request.GameSettings.MaxPlayers < 2)
                return new CreateRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
                
            
            Room room = new Room(request, dbUserModel.asUser());
            await room.CreateInDatabase();
                
               
            return new CreateRoomResponse()
            {
                CreatedRoom = room.GameConfiguration,
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
            };
        }

        public override async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new JoinRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new JoinRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            return new JoinRoomResponse()
            {
                Status = await room.JoinRoom(dbUserModel)
            };
        }

        public override async Task<LeaveRoomResponse> LeaveRoom(LeaveRoomRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new LeaveRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new LeaveRoomResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            return new LeaveRoomResponse()
            {
                Status = await room.LeaveRoom(dbUserModel)
            };
        }

        public override async Task<StartGameEarlyResponse> StartGameEarly(StartGameEarlyRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new StartGameEarlyResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new StartGameEarlyResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            if (room.GameConfiguration.Creator.Id == dbUserModel.UserModel.Id)
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
            
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new GetGameRoomEventsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new GetGameRoomEventsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            if (room.GameConfiguration.Players.All(it => it.Id != dbUserModel.UserModel.Id) && !dbUserModel.HasClaim(UserClaim.Admin))
                return new GetGameRoomEventsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };
            
            List<GameEventModel> events = await room.GetAllGameEvents();
            // Filter out only the player's events and events that have occurred in the past.
            // Get current tick to determine events in the past.
            GameTick currentTick = new GameTick(DateTime.FromFileTimeUtc(room.GameConfiguration.UnixTimeStarted), DateTime.UtcNow);
            
            // Admins see all events :)
            if (!dbUserModel.HasClaim(UserClaim.Admin))
            {
                events = events.FindAll(it =>
                    it.OccursAtTick <= currentTick.GetTick() || it.IssuedBy == dbUserModel.UserModel.Id);
            }

            GetGameRoomEventsResponse response = new GetGameRoomEventsResponse();
            response.GameEvents.AddRange(events);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }

        
        public override async Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            if(!room.IsPlayerInRoom(dbUserModel))
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };

            return await room.AddPlayerGameEvent(dbUserModel, request.EventData);
        }

        public override async Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new SubmitGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            // GameEventToUpdate.
            return await room.UpdateGameEvent(dbUserModel, request);
        }

        public override async Task<DeleteGameEventResponse> DeleteGameEvent(DeleteGameEventRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new DeleteGameEventResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            return await room.RemovePlayerGameEvent(dbUserModel, request.EventId);
        }

        public override async Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            if(room.GameConfiguration.RoomStatus != RoomStatus.Ongoing)
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED),
                };
            
            if(!request.UserIdsInGroup.Contains(dbUserModel.UserModel.Id))
                return new CreateMessageGroupResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };

            return await room.CreateMessageGroup(request.UserIdsInGroup.ToList());
        }

        public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            GroupChat groupChat = await room.GetGroupChatById(request.GroupId);
            if(groupChat == null)
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST)
                };
            
            if(!groupChat.IsPlayerInGroup(dbUserModel))
                return new SendMessageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PERMISSION_DENIED)
                };

            return new SendMessageResponse()
            {
                Status = await groupChat.SendChatMessage(dbUserModel, request.Message)
            };
        }

        public override async Task<GetMessageGroupsResponse> GetMessageGroups(GetMessageGroupsRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new GetMessageGroupsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new GetMessageGroupsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };

            List<GroupChat> groupChats = await room.GetPlayerGroupChats(dbUserModel);
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
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new GetGroupMessagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            Room room = await Room.GetRoomFromGuid(request.RoomId);
            if(room == null)
                return new GetGroupMessagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.ROOM_DOES_NOT_EXIST)
                };
            
            GroupChat groupChat = await room.GetGroupChatById(request.GroupId);
            if(groupChat == null)
                return new GetGroupMessagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.CHAT_GROUP_DOES_NOT_EXIST)
                };
            
            if(!groupChat.IsPlayerInGroup(dbUserModel))
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
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new BlockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DbUserModel friend = await DbUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                return new BlockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(await dbUserModel.IsBlocked(friend))
                return new BlockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
                };

            await dbUserModel.BlockUser(friend);
            return new BlockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }

        public override async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new UnblockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Check if player is valid.
            DbUserModel friend = await DbUserModel.GetUserFromGuid(request.UserIdToBlock);
            if (friend == null) 
                return new UnblockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            // Check if player is blocked.
            if(!await dbUserModel.IsBlocked(friend))
                return new UnblockPlayerResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };

            await dbUserModel.UnblockUser(friend);
            return new UnblockPlayerResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }

        public override async Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(ViewBlockedPlayersRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new ViewBlockedPlayersResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            ViewBlockedPlayersResponse response = new ViewBlockedPlayersResponse();

            var blockedUsers = Task.WhenAll(
                dbUserModel.GetBlockedUsers()
                    .Result
                    .Select(async it => (await DbUserModel.GetUserFromGuid(it.FriendId)).asUser())
            ).Result.ToList();
                
            
            response.BlockedUsers.AddRange(blockedUsers);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }

        public override async Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DbUserModel friend = await DbUserModel.GetUserFromGuid(request.FriendId);
            if (friend == null)
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(await friend.HasFriendRequestFrom(dbUserModel))
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
                };
            
            if(await friend.IsBlocked(dbUserModel) || await dbUserModel.IsBlocked(friend))
                return new SendFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_IS_BLOCKED)
                };
            
            // Add request to the other player.
            await friend.AddFriendRequestFrom(dbUserModel);
            return new SendFriendRequestResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS)
            };
        }

        public override async Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new AcceptFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DbUserModel friend = await DbUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                return new AcceptFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };

            if(!await dbUserModel.HasFriendRequestFrom(friend))
                return new AcceptFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.FRIEND_REQUEST_DOES_NOT_EXIST)
                };

            return new AcceptFriendRequestResponse()
            {
                Status = await dbUserModel.AcceptFriendRequestFrom(friend),
            };
        }

        public override async Task<ViewFriendRequestsResponse> ViewFriendRequests(ViewFriendRequestsRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new ViewFriendRequestsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            ViewFriendRequestsResponse response = new ViewFriendRequestsResponse();
            List<SubterfugeRemakeService.User> friendRequests = Task.WhenAll(
                dbUserModel.GetFriendRequests()
                    .Result
                    .Select(async it => (await DbUserModel.GetUserFromGuid(it.FriendId)).asUser())
            ).Result.ToList();
            
            response.IncomingFriends.AddRange(friendRequests);
            response.Status = ResponseFactory.createResponse(ResponseType.SUCCESS);
            return response;
        }
        
        public override async Task<DenyFriendRequestResponse> DenyFriendRequest(DenyFriendRequestRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new DenyFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DbUserModel friend = await DbUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                return new DenyFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(!await dbUserModel.HasFriendRequestFrom(friend))
                return new DenyFriendRequestResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.FRIEND_REQUEST_DOES_NOT_EXIST)
                };

            return new DenyFriendRequestResponse()
            {
                Status = await dbUserModel.RemoveFriendRequestFrom(friend),
            };
        }

        public override async Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new RemoveFriendResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            DbUserModel friend = await DbUserModel.GetUserFromGuid(request.FriendId);
            if(friend == null)
                return new RemoveFriendResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.PLAYER_DOES_NOT_EXIST)
                };
            
            if(!await dbUserModel.IsFriend(friend))
                return new RemoveFriendResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_REQUEST)
                };
                
            await dbUserModel.RemoveFriend(friend);
            return new RemoveFriendResponse();
        }

        public override async Task<ViewFriendsResponse> ViewFriends(ViewFriendsRequest request, ServerCallContext context)
        {
            DbUserModel dbUserModel = context.UserState["user"] as DbUserModel;
            if(dbUserModel == null)
                return new ViewFriendsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            ViewFriendsResponse response = new ViewFriendsResponse();
            List<SubterfugeRemakeService.User> friends = Task.WhenAll(
                dbUserModel.GetFriends()
                    .Result
                    .Select(async it =>
                        {
                            if (it.PlayerId == dbUserModel.UserModel.Id)
                            {
                                return (await DbUserModel.GetUserFromGuid(it.FriendId)).asUser();
                            }
                            else
                            {
                                return (await DbUserModel.GetUserFromGuid(it.PlayerId)).asUser();
                            }
                        }
                    )).Result.ToList();
            response.Friends.AddRange(friends);
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
            DbUserModel dbUserModel = await DbUserModel.GetUserFromUsername(request.Username);
            
            if (dbUserModel == null || !JwtManager.VerifyPasswordHash(request.Password, dbUserModel.UserModel.PasswordHash))
                return new AuthorizationResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.INVALID_CREDENTIALS)
                };

            string token = JwtManager.GenerateToken(dbUserModel.UserModel.Id);
            context.ResponseTrailers.Add("Authorization", token);
            return new AuthorizationResponse
            {
                Token = token,
                User = dbUserModel.asUser(),
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
                DbUserModel dbUserModel = await DbUserModel.GetUserFromGuid(uuid);
                if (dbUserModel != null)
                {
                    context.UserState["user"] = dbUserModel;
                    return new AuthorizationResponse()
                    {
                        Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                        Token = request.Token,
                        User = dbUserModel.asUser(),
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
            DbUserModel dbUserModel = await DbUserModel.GetUserFromUsername(request.Username);
            if(dbUserModel != null)
                return new AccountRegistrationResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.DUPLICATE)
                };
            
            // Create a new user model
            DbUserModel model = new DbUserModel(request);
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
            DbUserModel user = context.UserState["user"] as DbUserModel;
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
            string specialistId = configModel.SpecialistConfig.Id;

            return new SubmitCustomSpecialistResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                SpecialistConfigurationId = specialistId,
            };
        }

        public override async Task<GetCustomSpecialistsResponse> GetCustomSpecialists(GetCustomSpecialistsRequest request, ServerCallContext context)
        {
            DbUserModel user = context.UserState["user"] as DbUserModel;
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
            DbUserModel user = context.UserState["user"] as DbUserModel;
            if(user == null)
                return new GetPlayerCustomSpecialistsResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Get the requested user from their id
            DbUserModel player = await DbUserModel.GetUserFromGuid(request.PlayerId);
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
            DbUserModel user = context.UserState["user"] as DbUserModel;
            if(user == null)
                return new CreateSpecialistPackageResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };

            // Set author
            request.SpecialistPackage.Creator = user.asUser();
            SpecialistPackageModel packageModel = new SpecialistPackageModel(request.SpecialistPackage);
            await packageModel.SaveToDatabase();

            // Get the generated specialist ID
            string packageId = packageModel.SpecialistPackage.Id;

            return new CreateSpecialistPackageResponse()
            {
                Status = ResponseFactory.createResponse(ResponseType.SUCCESS),
                SpecialistPackageId = packageId,
            };
        }

        public override async Task<GetSpecialistPackagesResponse> GetSpecialistPackages(GetSpecialistPackagesRequest request, ServerCallContext context)
        {
            DbUserModel user = context.UserState["user"] as DbUserModel;
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
            DbUserModel user = context.UserState["user"] as DbUserModel;
            if(user == null)
                return new GetPlayerSpecialistPackagesResponse()
                {
                    Status = ResponseFactory.createResponse(ResponseType.UNAUTHORIZED)
                };
            
            // Get the requested user from their id
            DbUserModel player = await DbUserModel.GetUserFromGuid(request.PlayerId);
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