#nullable enable
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.game;

public class LobbyClient : ISubterfugeGameLobbyApi
{
    private SubterfugeHttpClient client;

    public LobbyClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }

    public async Task<SubterfugeResponse<GetLobbyResponse>> GetLobbies(GetLobbyRequest lobbyRequest)
    {
        return await client.Get<GetLobbyResponse>($"api/lobby", new Dictionary<string, string>()
        {
            {"Pagination", lobbyRequest.Pagination.ToString() },
            {"CreatedByUserId", lobbyRequest.CreatedByUserId },
            {"RoomStatus", lobbyRequest.RoomStatus.ToString() },
            {"UserIdInRoom", lobbyRequest.UserIdInRoom },
            {"RoomId", lobbyRequest.RoomId },
            {"Goal", lobbyRequest.Goal.ToString() },
            {"MinPlayers", lobbyRequest.MinPlayers.ToString() },
            {"MaxPlayers", lobbyRequest.MaxPlayers.ToString() },
            {"IsAnonymous", lobbyRequest.IsAnonymous.ToString() },
            {"IsRanked", lobbyRequest.IsRanked.ToString() },
        });
    }

    public async Task<SubterfugeResponse<CreateRoomResponse>> CreateNewRoom(CreateRoomRequest request)
    {
        return await client.Post<CreateRoomRequest, CreateRoomResponse>($"api/lobby/create", request);
    }
    
    public async Task<SubterfugeResponse<JoinRoomResponse>> JoinRoom(JoinRoomRequest request, string guid)
    {
        return await client.Post<JoinRoomRequest, JoinRoomResponse>($"api/lobby/{guid}/join", request);
    }
    
    public async Task<SubterfugeResponse<LeaveRoomResponse>> LeaveRoom(string guid)
    {
        return await client.Get<LeaveRoomResponse>($"api/lobby/{guid}/leave", null);
    }
    
    public async Task<SubterfugeResponse<StartGameEarlyResponse>> StartGameEarly(string guid)
    {
        return await client.Get<StartGameEarlyResponse>($"api/lobby/{guid}/start", null);
    }
}