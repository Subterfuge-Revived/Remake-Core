#nullable enable
using System;
using System.Threading.Tasks;

namespace SubterfugeCore.Models.GameEvents.Api
{
    /// <summary>
    /// Perform account operations
    /// </summary>
    public interface ISubterfugeAccountApi
    {
        /// <summary>
        /// Login to your account.
        /// </summary>
        /// <param name="request">The login request</param>
        /// <returns>A login response</returns>
        Task<SubterfugeResponse<AuthorizationResponse>> Login(AuthorizationRequest request);
        
        /// <summary>
        /// Register a new account
        /// </summary>
        /// <param name="registrationRequeset">Request params to register a new account</param>
        /// <returns>A registration response</returns>
        Task<SubterfugeResponse<AccountRegistrationResponse>> RegisterAccount(AccountRegistrationRequest registrationRequeset);

        /// <summary>
        /// Verifies the user's account with their verification code.
        /// </summary>
        /// <param name="validationRequest">The user's verification code</param>
        /// <returns>If the verification was successful or not</returns>
        Task<SubterfugeResponse<AccountVadliationResponse>> VerifyPhone(AccountValidationRequest validationRequest);
        
        /// <summary>
        /// Gets an individual user account
        /// </summary>
        /// <param name="userId">The userID to query</param>
        /// <returns>Userdata about the queried user</returns>
        Task<SubterfugeResponse<GetUserResponse>> GetUser(string userId);
        
        /// <summary>
        /// Queries all created accounts based on the set query parameters
        ///
        /// This endpoint can only be used by Administrators.
        /// </summary>
        /// <param name="request">Query and filter parameters to narrow the search</param>
        /// <returns>A list of users matching the query</returns>
        Task<SubterfugeResponse<GetDetailedUsersResponse>> GetUsers(GetUserRequest request);
        
        /// <summary>
        /// Gets all of a player's chat messages with no context.
        /// This is useful to let admins get a glimpse of messages being sent by the player.
        ///
        /// This endpoint can only be used by Administrators.
        /// </summary>
        /// <param name="playerId">The playerID to get chat messages for</param>
        /// <param name="pagination">Page number</param>
        /// <returns>A list of the player's chat messages ordered by most recently sent</returns>
        Task<SubterfugeResponse<GetPlayerChatMessagesResponse>> GetPlayerChatMessages(string playerId, int pagination = 1);
    }

    public interface ISubterfugeUserRoleApi
    {
        /// <summary>
        /// Gets all of the player's roles
        /// </summary>
        /// <param name="userId">The userID to get the roles of</param>
        /// <returns>A list of the user's roles</returns>
        Task<SubterfugeResponse<GetRolesResponse>> GetRoles(string userId);
        
        /// <summary>
        /// Updates a user's roles
        /// </summary>
        /// <param name="userId">The userID to update</param>
        /// <param name="request">An updated list of the user's roles.</param>
        /// <returns>If the update was successful or not</returns>
        Task<SubterfugeResponse<GetRolesResponse>> SetRoles(string userId, UpdateRolesRequest request);
    }

    public interface ISubterfugeAdminApi
    {
        /// <summary>
        /// Returns a list of all player actions.
        ///
        /// May only be used by Administrators.
        /// </summary>
        /// <param name="request">Filter and query params</param>
        /// <returns>A list of server actions matching the query params</returns>
        Task<SubterfugeResponse<ServerActionLogResponse>> GetActionLog(ServerActionLogRequeset request);
        
        /// <summary>
        /// Gets a list of all server exceptions that have been thrown.
        ///
        /// May only be used by administrators
        /// </summary>
        /// <param name="request">Query and filter params</param>
        /// <returns>A list of server exceptions</returns>
        Task<SubterfugeResponse<ServerExceptionLogResponse>> GetServerExceptions(ServerExceptionLogRequest request);
        
        /// <summary>
        /// Bans a player.
        ///
        /// May only be used by administrators
        /// </summary>
        /// <param name="userId">The userID to ban</param>
        /// <param name="until">The date to ban the user until</param>
        /// <param name="reason">The reason for the ban</param>
        /// <param name="adminNotes">Extra admin-only notes</param>
        /// <returns>If the ban was successful or not</returns>
        Task<SubterfugeResponse<BanPlayerResponse>> BanPlayer(BanPlayerRequest banRequest);
        
        /// <summary>
        /// Bans an IP or range of IP addresses
        ///
        /// May only be used by administrators
        /// </summary>
        /// <param name="directIpOrRegex">A regex string that matches a particular pattern of IP addresses.</param>
        /// <param name="until">The date to ban the IPs until.</param>
        /// <param name="adminNotes">Extra admin-only notes</param>
        /// <returns>If the ban was successful or not</returns>
        Task<SubterfugeResponse<BanIpResponse>> BanIp(BanIpRequest banIpRequest);
        
        /// <summary>
        /// Get a list of current IP bans
        ///
        /// May only be used by administrators
        /// </summary>
        /// <param name="pagination">Page number</param>
        /// <returns>A list of current IP bans</returns>
        Task<SubterfugeResponse<GetIpBansResponse>> GetIpBans(int pagination);
        
        /// <summary>
        /// Gets a list of currently banned players
        /// </summary>
        /// <param name="pagination">Page Number</param>
        /// <returns>A list of the currently banned players</returns>
        Task<SubterfugeResponse<GetBannedPlayerResponse>> GetBannedPlayers(int pagination);

        Task<SubterfugeResponse<Echo>> EchoRequest(Echo request);
    }
    
    public interface ISubterfugeGameLobbyApi
    {
        /// <summary>
        /// Gets a list of lobbies based on the filters provided.
        /// Regular players can see open lobbies from everyone, but can only see private lobbies that they have created or joined.
        /// 
        /// Administrators can see all lobbies.
        /// </summary>
        /// <param name="lobbyRequest">Filters to narrow the search</param>
        /// <returns>A list of visible lobbies matching the supplied filters</returns>
        Task<SubterfugeResponse<GetLobbyResponse>> GetLobbies(GetLobbyRequest lobbyRequest);
        
        /// <summary>
        /// Creates a new game lobby. If successful, the creator of the lobby is a member of the lobby.
        /// </summary>
        /// <param name="createRoomRequest">Parameters for the game settings</param>
        /// <returns>If the lobby was created, and the ID of the lobby if it was.</returns>
        Task<SubterfugeResponse<CreateRoomResponse>> CreateNewRoom(CreateRoomRequest createRoomRequest);
        
        /// <summary>
        /// Join a lobby.
        /// </summary>
        /// <param name="request">TODO: Delete this class.</param>
        /// <param name="guid">The ID of the lobby to join</param>
        /// <returns>If the player successfully joined the lobby</returns>
        Task<SubterfugeResponse<JoinRoomResponse>> JoinRoom(JoinRoomRequest request, string guid);
        
        /// <summary>
        /// Leave a lobby.
        /// </summary>
        /// <param name="guid">The ID of the lobby to leave</param>
        /// <returns>If the player successfully left the lobby</returns>
        Task<SubterfugeResponse<LeaveRoomResponse>> LeaveRoom(string guid);
        
        /// <summary>
        /// Starts a game early if the maximum number of players has not been reached.
        /// Games can only be started early by the lobby's creator.
        /// </summary>
        /// <param name="guid">The ID of the lobby to start</param>
        /// <returns>If the lobby was successfully started</returns>
        Task<SubterfugeResponse<StartGameEarlyResponse>> StartGameEarly(string guid);
    }
    
    public interface ISubterfugeGameEventApi
    {
        /// <summary>
        /// Gets a list of game events currently visible to the player
        ///
        /// Administrators can see every event in a lobby.
        /// </summary>
        /// <param name="roomId">The ID of the room to get events for</param>
        /// <returns>A list of visible game events</returns>
        Task<SubterfugeResponse<GetGameRoomEventsResponse>> GetGameRoomEvents(string roomId);
        
        /// <summary>
        /// Submits a new game event to the room
        /// </summary>
        /// <param name="request">Details about the event to submit</param>
        /// <param name="roomId">The room id to submit the event to. The game must be ongoing and the player must be a member of the room.</param>
        /// <returns>If the event was successfully submitted to the room</returns>
        Task<SubterfugeResponse<SubmitGameEventResponse>> SubmitGameEvent(SubmitGameEventRequest request, string roomId);
        
        /// <summary>
        /// Updates an event that was previously submitted to the room.
        /// </summary>
        /// <param name="request">The updated event details</param>
        /// <param name="roomId">The ID of the room</param>
        /// <param name="eventGuid">The ID of the event to update</param>
        /// <returns>If the event was updated successfully</returns>
        Task<SubterfugeResponse<SubmitGameEventResponse>> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid);
        
        /// <summary>
        /// Deletes a game event you have previously submitted
        /// </summary>
        /// <param name="roomId">The room ID the event was submitted to</param>
        /// <param name="eventGuid">The ID of the event to delete. Must have been an event submitted by the player</param>
        /// <returns>If the event was deleted</returns>
        Task<SubterfugeResponse<DeleteGameEventResponse>> DeleteGameEvent(string roomId, string eventGuid);
    }
    
    public interface ISubterfugeHealthApi
    {
        /// <summary>
        /// Generic server health check.
        /// </summary>
        /// <returns>A server response</returns>
        Task<SubterfugeResponse<PingResponse>> Ping();
        
        /// <summary>
        /// A server health check that requires the user to be logged in to access
        /// </summary>
        /// <returns>A server response</returns>
        Task<SubterfugeResponse<AuthorizedPingResponse>> AuthorizedPing();
    }
    
    public interface ISubterfugeGroupChatApi
    {
        /// <summary>
        /// Creates a message group between players within a lobby
        /// </summary>
        /// <param name="request">The details about players to start a chat between</param>
        /// <param name="roomId">The ID of the room to chat in</param>
        /// <returns>If the message group was created and the ID of the group if it was</returns>
        Task<SubterfugeResponse<CreateMessageGroupResponse>> CreateMessageGroup(CreateMessageGroupRequest request, string roomId);
        
        /// <summary>
        /// Sends a chat message to a group chat
        /// </summary>
        /// <param name="request">Details about the message to send</param>
        /// <param name="roomId">The roomID to send the message to</param>
        /// <param name="groupId">The groupID to send the message to</param>
        /// <returns>If the message was successfully sent</returns>
        Task<SubterfugeResponse<SendMessageResponse>> SendMessage(SendMessageRequest request, string roomId, string groupId);
        
        /// <summary>
        /// Gets a list of message groups you are a member of within the room.
        ///
        /// Administrators can see all message groups in a lobby.
        /// </summary>
        /// <param name="roomId">The ID of the room to view groups of</param>
        /// <returns>A list of message groups the player is a member of within the room</returns>
        Task<SubterfugeResponse<GetMessageGroupsResponse>> GetMessageGroups(string roomId);
        
        /// <summary>
        /// Gets a list of chat messages sent within a group chat for a room.
        ///
        /// Administrators can view all messages.
        /// </summary>
        /// <param name="request">TODO: remove this to just a param</param>
        /// <param name="roomId">The ID of the room to get messages for</param>
        /// <param name="groupId">The ID of the group to get messages from</param>
        /// <returns>A list of messages in the group</returns>
        Task<SubterfugeResponse<GetGroupMessagesResponse>> GetMessages(
            GetGroupMessagesRequest request,
            string roomId,
            string groupId);
    }
    
    public interface ISubterfugeSocialApi
    {
        /// <summary>
        /// Blocks a player. Blocked players cannot communicate with you or see lobbies you create.
        /// If you block a friend, they will be removed as a friend. You cannot block an administrator.
        /// </summary>
        /// <param name="request">TODO: Remove this.</param>
        /// <param name="userId">The ID of the user to block</param>
        /// <returns>If the player was blocked.</returns>
        Task<SubterfugeResponse<BlockPlayerResponse>> BlockPlayer(BlockPlayerRequest request, string userId);
        
        /// <summary>
        /// Unblocks a previously blocked player.
        /// </summary>
        /// <param name="request">TODO: remove this.</param>
        /// <param name="userId">The ID of the user to block</param>
        /// <returns>If the player was unblocked.</returns>
        Task<SubterfugeResponse<UnblockPlayerResponse>> UnblockPlayer(UnblockPlayerRequest request, string userId);
        
        /// <summary>
        /// Get a list of all players you have blocked. Normal players can only view their own list
        /// Administrators can view anyone's blocked player list.
        /// </summary>
        /// <param name="userId">
        /// The userID to view the blocked players of. Normal users can only use their own userID here. Administrators can use any userID
        /// </param>
        /// <returns>A list of players that the user has blocked</returns>
        Task<SubterfugeResponse<ViewBlockedPlayersResponse>> ViewBlockedPlayers(string userId);
        
        /// <summary>
        /// Sends a friend request to another player.
        /// If you have an incoming friend request, accepts the friend requests.
        /// </summary>
        /// <param name="userId">The used ID to add as a friend</param>
        /// <returns>If the request was successful</returns>
        Task<SubterfugeResponse<AddAcceptFriendResponse>> AddAcceptFriendRequest(string userId);
        
        /// <summary>
        /// Removes a player as your friend.
        /// If you have an incoming friend request, removes the friend request.
        /// </summary>
        /// <param name="userId">The user ID to remove as a friend</param>
        /// <returns>If the user was removed as a friend</returns>
        Task<SubterfugeResponse<DenyFriendRequestResponse>> RemoveRejectFriend(string userId);
        
        /// <summary>
        /// Gets a list of your incoming friend requests.
        /// Administrators can view anyone's friend requests.
        /// </summary>
        /// <param name="userId">The userID to view friend requests for.</param>
        /// <returns>A list of friend requests for the player</returns>
        Task<SubterfugeResponse<ViewFriendRequestsResponse>> ViewFriendRequests(string userId);
        
        /// <summary>
        /// Gets a list of your friends.
        /// Administrators can view anyone's friend list.
        /// </summary>
        /// <param name="userId">The userID to get the friend list of.</param>
        /// <returns>A list of the player's friends</returns>
        Task<SubterfugeResponse<ViewFriendsResponse>> GetFriendList(string userId);
    }
    
    public interface ISubterfugeCustomSpecialistApi
    {
        /// <summary>
        /// Create a custom specialist
        /// </summary>
        /// <param name="submitCustomSpecialistRequest">Details about the specialist configuration</param>
        /// <returns>If the specialist was submitted</returns>
        Task<SubterfugeResponse<SubmitCustomSpecialistResponse>> SubmitCustomSpecialist(SubmitCustomSpecialistRequest submitCustomSpecialistRequest);
        
        /// <summary>
        /// Query a list of custom specialists
        /// </summary>
        /// <param name="getCustomSpecialistsRequest">Query parameters</param>
        /// <returns>A list of custom specialists matching the query parameters</returns>
        Task<SubterfugeResponse<GetCustomSpecialistsResponse>> GetCustomSpecialists(GetCustomSpecialistsRequest getCustomSpecialistsRequest);
        
        /// <summary>
        /// Gets details about a particular specialist
        /// </summary>
        /// <param name="specialistId">The specialist ID</param>
        /// <returns>The specialist configuration details</returns>
        Task<SubterfugeResponse<GetCustomSpecialistsResponse>> GetCustomSpecialist(string specialistId);
    }

    public interface ISubterfugeSpecialistPackageApi
    {
        /// <summary>
        /// Creates a specialist package which contains a collection of multiple specialists or packages.
        /// </summary>
        /// <param name="createSpecialistPackageRequest">Details about the specialists included in the package</param>
        /// <returns>Details about the created package</returns>
        Task<SubterfugeResponse<CreateSpecialistPackageResponse>> CreateSpecialistPackage(CreateSpecialistPackageRequest createSpecialistPackageRequest);
        
        /// <summary>
        /// Query a list of specialist packages matching the query parameters
        /// </summary>
        /// <param name="getSpecialistPackagesRequest">Query parameters</param>
        /// <returns>A list of specialist packages matching the query parameters</returns>
        Task<SubterfugeResponse<GetSpecialistPackagesResponse>> GetSpecialistPackages(GetSpecialistPackagesRequest getSpecialistPackagesRequest);
        
        /// <summary>
        /// Gets a specialist package by ID
        /// </summary>
        /// <param name="packageId">The specialist package ID</param>
        /// <returns>Details about the specialist package and included specialists.</returns>
        Task<SubterfugeResponse<GetSpecialistPackagesResponse>> GetSpecialistPackages(string packageId);
    }

    public interface ISubterfugeAnnouncementApi
    {
        /// <summary>
        /// Submites a new announcement to share to members of the game.
        /// Admin Only.
        /// </summary>
        /// <param name="announcementRequest">The new announcement to create</param>
        /// <returns>The ID of the created announcement</returns>
        Task<SubterfugeResponse<CreateAnnouncementResponse>> CreateAnnouncement(CreateAnnouncementRequest announcementRequest);
        
        /// <summary>
        /// Submites a new announcement to share to members of the game.
        /// Admin Only.
        /// </summary>
        /// <param name="announcementRequest">The ID of the announcement to update</param>
        /// <param name="announcementRequest">The new announcement to create</param>
        /// <returns>The ID of the created announcement</returns>
        Task<SubterfugeResponse<CreateAnnouncementResponse>> UpdateAnnouncement(string id, CreateAnnouncementRequest announcementRequest);
        
        /// <summary>
        /// Get a list of all announcements visible to the current player.
        /// </summary>
        /// <returns>Announcements visible to the current player</returns>
        Task<SubterfugeResponse<GetAnnouncementsResponse>> GetAnnouncements();
    }
}