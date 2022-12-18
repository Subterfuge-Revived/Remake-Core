using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.game;

public class LobbyClient
{
    private HttpClient client;

    public LobbyClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<GetLobbyResponse> GetLobbies()
    {
        HttpResponseMessage response = await client.GetAsync($"api/lobby");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetLobbyResponse>();
    }
    
    public async Task<GetLobbyResponse> GetPlayerCurrentGames(string userId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/{userId}/lobbies");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetLobbyResponse>();
    }
    
    public async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/lobby/create", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<CreateRoomResponse>();
    }
    
    public async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, string guid)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/lobby/{guid}/join", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<JoinRoomResponse>();
    }
    
    public async Task<LeaveRoomResponse> LeaveRoom(string guid)
    {
        HttpResponseMessage response = await client.GetAsync($"api/lobby/{guid}/leave");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<LeaveRoomResponse>();
    }
    
    public async Task<StartGameEarlyResponse> StartGameEarly(string guid)
    {
        HttpResponseMessage response = await client.GetAsync($"api/lobby/{guid}/start");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<StartGameEarlyResponse>();
    }
}