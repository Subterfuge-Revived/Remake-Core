using System.Threading.Tasks;

namespace SubterfugeCore.Models.GameEvents.Api
{
    public interface IAccountApi
    {
        Task<AuthorizationResponse> Login(AuthorizationRequest request);
    }
    
    public interface IGameLobbyApi
    {
        OpenLobbiesResponse GetOpenLobbies(OpenLobbiesRequest lobbyRequest);
        GetLobbyResponse GetPlayerCurrentGames(PlayerCurrentGamesRequest currentGamesRequest);
        CreateRoomResponse CreateNewRoom(CreateRoomRequest createRoomRequest);
        JoinRoomResponse JoinRoom(JoinRoomRequest joinRoomRequest);
        LeaveRoomResponse LeaveRoom();
        StartGameEarlyResponse StartGameEarly();
    }
    
    public interface IGameEventApi
    {
        GetGameRoomEventsResponse GetGameRoomEvents();
        SubmitGameEventResponse SubmitGameEvent(SubmitGameEventRequest submitGameEventRequest);
        SubmitGameEventResponse UpdateGameEvent(UpdateGameEventRequest updateGameEventRequest);
        DeleteGameEventResponse DeleteGameEvent(DeleteGameEventRequest deleteGameEventRequest);
    }
    
    public interface INetworkHealthApi
    {
        PingResponse Ping(PingRequest pingRequest);
        PingResponse AuthorizedHealthCheck(AuthorizedPingRequest authorizedPingRequest);
    }
    
    public interface IGroupChatApi
    {
        CreateMessageGroupResponse CreateMessageGroup(CreateMessageGroupRequest createMessageGroupRequest);
        SendMessageResponse SendMessage(SendMessageRequest sendMessageRequest);
        GetMessageGroupsResponse GetMessageGroups(GetMessageGroupsRequest getMessageGroupsRequest);
        GetGroupMessagesResponse GetGroupMessages(GetGroupMessagesRequest getGroupMessagesRequest);
    }
    
    public interface ISocialApi
    {
        BlockPlayerResponse BlockPlayer(BlockPlayerRequest blockPlayerRequest);
        UnblockPlayerResponse UnblockPlayer(UnblockPlayerRequest unblockPlayerRequest);
        ViewBlockedPlayersResponse ViewBlockedPlayers(ViewBlockedPlayersRequest viewBlockedPlayersRequest);
        AddAcceptFriendResponse AddAcceptFriendRequest(SendFriendRequestRequest sendFriendRequestRequest);
        DenyFriendRequestResponse DenyFriendRequest(DenyFriendRequestRequest denyFriendRequestRequest);
        ViewFriendRequestsResponse ViewFriendRequests(ViewFriendRequestsRequest viewFriendRequestsRequest);
        RemoveFriendResponse RemoveFriend(RemoveFriendRequest removeFriendRequest);
        ViewFriendsResponse ViewFriends(ViewFriendsRequest viewFriendsRequest);
    }
    
    public interface ICustomSpecialistApi
    {

        SubmitCustomSpecialistResponse SubmitCustomSpecialist(
            SubmitCustomSpecialistRequest submitCustomSpecialistRequest);

        GetCustomSpecialistsResponse GetCustomSpecialists(GetCustomSpecialistsRequest getCustomSpecialistsRequest);

        CreateSpecialistPackageResponse CreateSpecialistPackage(CreateSpecialistPackageRequest createSpecialistPackageRequest);

        GetSpecialistPackagesResponse GetSpecialistPackages(GetSpecialistPackagesRequest getSpecialistPackagesRequest);
    }
}