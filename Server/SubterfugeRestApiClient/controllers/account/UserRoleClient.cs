using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.account;

public class UserRoleClient : ISubterfugeUserRoleApi
{
    private HttpClient client;

    public UserRoleClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<GetRolesResponse> GetRoles(string userId)
    {
        Console.WriteLine("GetRoles");
        HttpResponseMessage response = await client.GetAsync($"api/user/{userId}/roles");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetRolesResponse>();
    }
    
    public async Task<GetRolesResponse> SetRoles(string userId, UpdateRolesRequest request)
    {
        Console.WriteLine("SetRoles");
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/user/{userId}/roles", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetRolesResponse>();
    }
}