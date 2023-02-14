using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.health;

public class HealthClient : ISubterfugeHealthApi
{
    private SubterfugeHttpClient client;

    public HealthClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }
    
    public async Task<SubterfugeResponse<PingResponse>> Ping()
    {
        return await client.Get<PingResponse>($"api/Health/Ping", null);
    }
    
    public async Task<SubterfugeResponse<AuthorizedPingResponse>> AuthorizedPing()
    {
        return await client.Get<AuthorizedPingResponse>($"api/Health/AuthorizedPing", null);
    }
}