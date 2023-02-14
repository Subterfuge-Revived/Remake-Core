#nullable enable
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.Client;

namespace SubterfugeRestApiClient.controllers.account;

public class UserControllerClient : ISubterfugeAccountApi
{
    private SubterfugeHttpClient client;

    public UserControllerClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }
    
    public async Task<SubterfugeResponse<AuthorizationResponse>> Login(AuthorizationRequest request)
    {
        var response = await client.Post<AuthorizationRequest, AuthorizationResponse>("api/user/login", request);
        if (response.IsSuccess())
        {
            client.SetAuthHeader(response.GetOrThrow().Token);
        }

        return response;
    }

    public void Logout()
    {
        client.SetAuthHeader(null);
    }
    
    public void SetToken(string token)
    {
        client.SetAuthHeader(token);
    }
    
    public async Task<SubterfugeResponse<AccountRegistrationResponse>> RegisterAccount(AccountRegistrationRequest request)
    {
        var response = await client.Post<AccountRegistrationRequest, AccountRegistrationResponse>("api/user/register", request);
        if (response.IsSuccess())
        {
            client.SetAuthHeader(response.GetOrThrow().Token);
        }
        return response;
    }

    public async Task<SubterfugeResponse<AccountVadliationResponse>> VerifyPhone(AccountValidationRequest validationRequest)
    {
        return await client.Post<AccountValidationRequest, AccountVadliationResponse>("api/user/verifyPhone", validationRequest);
    }

    public async Task<SubterfugeResponse<GetUserResponse>> GetUser(string userId)
    {
        return await client.Get<GetUserResponse>($"api/user/query", new Dictionary<string, string>()
        {
            { "userId", userId },
        });
    }

    public async Task<SubterfugeResponse<GetDetailedUsersResponse>> GetUsers(GetUserRequest request)
    {
        return await client.Get<GetDetailedUsersResponse>($"api/user/query", new Dictionary<string, string>()
        {
            { "pagination", request.pagination.ToString() },
            { "UsernameSearch", request.UsernameSearch },
            { "DeviceIdentifierSearch", request.DeviceIdentifierSearch },
            { "UserIdSearch", request.UserIdSearch },
            { "RequireUserClaims", request.RequireUserClaim.ToString() },
            { "isBanned", request.isBanned.ToString() },
        });
    }

    public async Task<SubterfugeResponse<GetPlayerChatMessagesResponse>> GetPlayerChatMessages(string playerId, int pagination = 1)
    {
        return await client.Get<GetPlayerChatMessagesResponse>($"api/user/messages", new Dictionary<string, string>()
        {
            { "playerId", playerId },
            { "pagination", pagination.ToString() },
        });
    }
}