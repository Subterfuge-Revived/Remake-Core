#nullable enable
using System.Web;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.game;

public class LobbyClient : ISubterfugeGameLobbyApi
{
    private HttpClient client;

    public LobbyClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<GetLobbyResponse> GetLobbies(GetLobbyRequest lobbyRequest)
    {
        Console.WriteLine("GetLobbies");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["Pagination"] = lobbyRequest.Pagination.ToString();
        query["CreatedByUserId"] = lobbyRequest.CreatedByUserId;
        query["RoomStatus"] = lobbyRequest.RoomStatus.ToString();
        query["UserIdInRoom"] = lobbyRequest.UserIdInRoom;
        query["RoomId"] = lobbyRequest.RoomId;
        query["Goal"] = lobbyRequest.Goal.ToString();
        query["MinPlayers"] = lobbyRequest.MinPlayers.ToString();
        query["MaxPlayers"] = lobbyRequest.MaxPlayers.ToString();
        query["IsAnonymous"] = lobbyRequest.IsAnonymous.ToString();
        query["IsRanked"] = lobbyRequest.IsRanked.ToString();
        string queryString = query.ToString();
        
        HttpResponseMessage response = await client.GetAsync($"api/lobby?{queryString}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetLobbyResponse>();
    }

    public async Task<CreateRoomResponse> CreateNewRoom(CreateRoomRequest request)
    {
        Console.WriteLine("CreateNewRoom");
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/lobby/create", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<CreateRoomResponse>();
    }
    
    public async Task<JoinRoomResponse> JoinRoom(JoinRoomRequest request, string guid)
    {
        Console.WriteLine("JoinRoom");
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/lobby/{guid}/join", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<JoinRoomResponse>();
    }
    
    public async Task<LeaveRoomResponse> LeaveRoom(string guid)
    {
        Console.WriteLine("LeaveRoom");
        HttpResponseMessage response = await client.GetAsync($"api/lobby/{guid}/leave");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<LeaveRoomResponse>();
    }
    
    public async Task<StartGameEarlyResponse> StartGameEarly(string guid)
    {
        Console.WriteLine("StartGameEarly");
        HttpResponseMessage response = await client.GetAsync($"api/lobby/{guid}/start");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<StartGameEarlyResponse>();
    }
}