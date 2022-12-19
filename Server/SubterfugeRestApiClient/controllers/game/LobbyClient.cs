#nullable enable
using System.Web;
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

    public async Task<GetLobbyResponse> GetLobbies(
        int pagination = 1,
        RoomStatus roomStatus = RoomStatus.Open,
        string? createdByUserId = null,
        string? userIdInRoom = null,
        string? roomId = null,
        Goal? goal = null,
        int? minPlayers = 0,
        int? maxPlayers = 999,
        bool? isAnonymous = null,
        bool? isRanked = null
    ) {
        
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["pagination"] = pagination.ToString();
        query["createdByUserId"] = createdByUserId;
        query["roomStatus"] = roomStatus.ToString();
        query["userIdInRoom"] = userIdInRoom;
        query["roomId"] = roomId;
        query["goal"] = goal.ToString();
        query["minPlayers"] = minPlayers.ToString();
        query["maxPlayers"] = maxPlayers.ToString();
        query["isAnonymous"] = isAnonymous.ToString();
        query["isRanked"] = isRanked.ToString();
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