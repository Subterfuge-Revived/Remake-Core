using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.account;

public class UserRoleClient : ISubterfugeUserRoleApi
{
    private SubterfugeHttpClient client;

    public UserRoleClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }

    public async Task<SubterfugeResponse<GetRolesResponse>> GetRoles(string userId)
    {
        return await client.Get<GetRolesResponse>($"api/user/{userId}/roles", null);
    }
    
    public async Task<SubterfugeResponse<GetRolesResponse>> SetRoles(string userId, UpdateRolesRequest request)
    {
        return await client.Post<UpdateRolesRequest, GetRolesResponse>($"api/user/{userId}/roles", request);
    }
}