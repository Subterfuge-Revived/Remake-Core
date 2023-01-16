using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.health;

public class HealthClient : ISubterfugeHealthApi
{
    private HttpClient client;

    public HealthClient(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<PingResponse> Ping()
    {
        Console.WriteLine("Ping");
        HttpResponseMessage response = await client.GetAsync($"api/Health/Ping");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<PingResponse>();
    }
    
    public async Task<PingResponse> AuthorizedPing()
    {
        Console.WriteLine("AuthorizedPing");
        HttpResponseMessage response = await client.GetAsync($"api/Health/AuthorizedPing");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<PingResponse>();
    }
}