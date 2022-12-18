using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.game;

public class GameEventClient
{
    private HttpClient client;

    public GameEventClient(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<GetGameRoomEventsResponse> GetGameRoomEvents(string roomId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/room/{roomId}/events");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetGameRoomEventsResponse>();
    }
    
    public async Task<SubmitGameEventResponse> SubmitGameEvent(string roomId, SubmitGameEventRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/room/{roomId}/events", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<SubmitGameEventResponse>();
    }
    
    public async Task<SubmitGameEventResponse> UpdateGameEvent(string roomId, string eventGuid, UpdateGameEventRequest request)
    {
        HttpResponseMessage response = await client.PutAsJsonAsync($"api/room/{roomId}/events/{eventGuid}", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<SubmitGameEventResponse>();
    }
    
    public async Task<DeleteGameEventResponse> DeleteGameEvent(string roomId, string eventGuid)
    {
        HttpResponseMessage response = await client.DeleteAsync($"api/room/{roomId}/events/{eventGuid}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<DeleteGameEventResponse>();
    }
}