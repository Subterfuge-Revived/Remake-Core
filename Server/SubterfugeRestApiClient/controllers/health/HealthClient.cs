using Subterfuge.Remake.Api.Client.controllers.Client;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;

namespace Subterfuge.Remake.Api.Client.controllers.health;

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