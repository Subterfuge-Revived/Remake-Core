using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.game;

public class GameEventClient : ISubterfugeGameEventApi
{
    private SubterfugeHttpClient client;

    public GameEventClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }
    
    public async Task<SubterfugeResponse<GetGameRoomEventsResponse>> GetGameRoomEvents(string roomId)
    {
        return await client.Get<GetGameRoomEventsResponse>($"api/room/{roomId}/events", null);
    }

    public async Task<SubterfugeResponse<SubmitGameEventResponse>> SubmitGameEvent(SubmitGameEventRequest request, string roomId)
    {
        return await client.Post<SubmitGameEventRequest, SubmitGameEventResponse>($"api/room/{roomId}/events", request);
    }

    public async Task<SubterfugeResponse<SubmitGameEventResponse>> UpdateGameEvent(UpdateGameEventRequest request, string roomId, string eventGuid)
    {
        return await client.Put<UpdateGameEventRequest, SubmitGameEventResponse>($"api/room/{roomId}/events/{eventGuid}", request);
    }

    public async Task<SubterfugeResponse<DeleteGameEventResponse>> DeleteGameEvent(string roomId, string eventGuid)
    {
        return await client.Delete<DeleteGameEventResponse>($"api/room/{roomId}/events/{eventGuid}", null);
    }
}