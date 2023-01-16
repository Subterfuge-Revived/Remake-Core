#nullable enable
using System;
using System.Threading.Tasks;

namespace SubterfugeCore.Models.GameEvents.Api
{
    public interface ISubterfugeAccountApi
    {
        Task<AuthorizationResponse> Login(AuthorizationRequest request);
        Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest registrationRequeset);
        Task<GetUserResponse> GetUsers(GetUserRequest request);
    }

    public interface ISubterfugeUserRoleApi
    {
        Task<GetRolesResponse> GetRoles(string userId);
        Task<GetRolesResponse> SetRoles(string userId, UpdateRolesRequest request);
    }

    public interface ISubterfugeAdminApi
    {
        Task<ServerActionLogResponse> GetActionLog(ServerActionLogRequeset request);
        Task<ServerExceptionLogResponse> GetServerExceptions(ServerExceptionLogRequest request);
        Task<NetworkResponse> BanPlayer(string userId, DateTime until, string reason, string adminNotes);
        Task<NetworkResponse> BanIp(string directIpOrRegex, DateTime until, string adminNotes);
        Task<GetIpBansResponse> GetIpBans(int pagination);
        Task<GetBannedPlayerResponse> GetBannedPlayers(int pagination);
    }
    
    public interface ISubterfugeGameLobbyApi
    {
        Task<GetLobbyResponse> GetLobbies(GetLobbyRequest lobbyRequest);
        Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest createRoomRequest);
        Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, string guid);
        Task<LeaveRoomResponse> LeaveRoom(string guid);
        Task<StartGameEarlyResponse> StartGameEarly(string guid);
    }
    
    public interface ISubterfugeGameEventApi
    {
        Task<GetGameRoomEventsResponse> GetGameRoomEvents(string roomId);
        Task<SubmitGameEventResponse> SubmitGameEvent(SubmitGameEventRequest request, string roomId);
        Task<SubmitGameEventResponse> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid);
        Task<DeleteGameEventResponse> DeleteGameEvent(string roomId, string eventGuid);
    }
    
    public interface ISubterfugeHealthApi
    {
        Task<PingResponse> Ping();
        Task<PingResponse> AuthorizedPing();
    }
    
    public interface ISubterfugeGroupChatApi
    {
        Task<CreateMessageGroupResponse> CreateMessageGroup(CreateMessageGroupRequest request, string roomId);
        Task<SendMessageResponse> SendMessage(SendMessageRequest request, string roomId, string groupId);
        Task<GetMessageGroupsResponse> GetMessageGroups(string roomId);
        Task<GetGroupMessagesResponse> GetMessages(
            GetGroupMessagesRequest request,
            string roomId,
            string groupId);
    }
    
    public interface ISubterfugeSocialApi
    {
        Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, string userId);
        Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, string userId);
        Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(string userId);
        Task<AddAcceptFriendResponse> AddAcceptFriendRequest(string userId);
        Task<DenyFriendRequestResponse> RemoveRejectFriend(string userId);
        Task<ViewFriendRequestsResponse> ViewFriendRequests(string userId);
        Task<ViewFriendsResponse> GetFriendList(string userId);
    }
    
    public interface ISubterfugeCustomSpecialistApi
    {

        Task<SubmitCustomSpecialistResponse> SubmitCustomSpecialist(SubmitCustomSpecialistRequest submitCustomSpecialistRequest);
        Task<GetCustomSpecialistsResponse> GetCustomSpecialists(GetCustomSpecialistsRequest getCustomSpecialistsRequest);
        Task<GetCustomSpecialistsResponse> GetCustomSpecialist(string specialistId);
    }

    public interface ISubterfugeSpecialistPackageApi
    {
        Task<CreateSpecialistPackageResponse> CreateSpecialistPackage(CreateSpecialistPackageRequest createSpecialistPackageRequest);
        Task<GetSpecialistPackagesResponse> GetSpecialistPackages(GetSpecialistPackagesRequest getSpecialistPackagesRequest);
        Task<GetSpecialistPackagesResponse> GetSpecialistPackages(string packageId);
    }
}