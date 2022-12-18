using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.account;

public class UserRoleClient
{
    private HttpClient client;

    public UserRoleClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<GetRolesResponse> GetRoles(string userId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/user/{userId}/GetRoles");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetRolesResponse>();
    }
    
    public async Task<GetRolesResponse> SetRoles(string userId, UpdateRolesRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/user/{userId}/SetRoles", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<GetRolesResponse>();
    }
}