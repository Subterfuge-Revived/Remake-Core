using Subterfuge.Remake.Api.Client.controllers.Client;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;

namespace Subterfuge.Remake.Api.Client.controllers.account;

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