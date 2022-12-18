using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.health;

public class HealthClient
{
    private HttpClient client;

    public HealthClient(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<PingResponse> Ping()
    {
        HttpResponseMessage response = await client.GetAsync($"api/Health/Ping");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<PingResponse>();
    }
    
    public async Task<PingResponse> AuthorizedPing()
    {
        HttpResponseMessage response = await client.GetAsync($"api/Health/AuthorizedPing");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<PingResponse>();
    }
}