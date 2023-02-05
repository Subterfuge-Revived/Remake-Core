using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.admin;

public class AdminClient : ISubterfugeAdminApi
{
    private HttpClient client;

    public AdminClient(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<ServerActionLogResponse> GetActionLog(ServerActionLogRequeset request)
    {
        HttpResponseMessage response = await client.GetAsync($"api/admin/serverLog");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<ServerActionLogResponse>();
    }

    public async Task<ServerExceptionLogResponse> GetServerExceptions(ServerExceptionLogRequest request)
    {
        HttpResponseMessage response = await client.GetAsync($"api/admin/exceptions");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<ServerExceptionLogResponse>();
    }

    public async Task<NetworkResponse> BanPlayer(BanPlayerRequest banPlayerRequest)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/admin/banPlayer", banPlayerRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<NetworkResponse>();
    }

    public async Task<NetworkResponse> BanIp(BanIpRequest banIpRequest)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/admin/banIp", banIpRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<NetworkResponse>();
    }

    public async Task<GetIpBansResponse> GetIpBans(int pagination)
    {
        HttpResponseMessage response = await client.GetAsync($"api/admin/ipBans");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetIpBansResponse>();
    }

    public async Task<GetBannedPlayerResponse> GetBannedPlayers(int pagination)
    {
        HttpResponseMessage response = await client.GetAsync($"api/admin/playerBans");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetBannedPlayerResponse>();
    }

    public async Task<Echo> EchoRequest(Echo request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/admin/echo", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<Echo>();
    }
}