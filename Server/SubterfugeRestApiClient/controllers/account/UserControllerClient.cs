#nullable enable
using System.Net.Http.Headers;
using System.Web;
using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.account;

public class UserControllerClient : ISubterfugeAccountApi
{
    private HttpClient client;

    public UserControllerClient(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<AuthorizationResponse> Login(AuthorizationRequest request)
    {
        Console.WriteLine("Login");
        HttpResponseMessage response = await client.PostAsJsonAsync("api/user/login", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }

        // return URI of the created resource.
        var parsedResponse = await response.Content.ReadAsAsync<AuthorizationResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parsedResponse.Token);
        return parsedResponse;
    }

    public void Logout()
    {
        client.DefaultRequestHeaders.Authorization = null;
    }
    
    public void SetToken(string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    public async Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest request)
    {
        Console.WriteLine("RegisterAccount");
        HttpResponseMessage response = await client.PostAsJsonAsync("api/user/register", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }

        // return URI of the created resource.
        var parsedResponse = await response.Content.ReadAsAsync<AccountRegistrationResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parsedResponse.Token);
        return parsedResponse;
    }

    public async Task<GetUserResponse> GetUser(string userId)
    {
        Console.WriteLine("GetUsers");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["userId"] = userId;
        string queryString = query.ToString();
        
        HttpResponseMessage response = await client.GetAsync($"api/user/query?{queryString}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }

        // return URI of the created resource.
        return await response.Content.ReadAsAsync<GetUserResponse>();
    }

    public async Task<GetDetailedUsersResponse> GetUsers(GetUserRequest request)
    {
        Console.WriteLine("GetUsers");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["pagination"] = request.pagination.ToString();
        query["UsernameSearch"] = request.UsernameSearch;
        query["DeviceIdentifierSearch"] = request.DeviceIdentifierSearch;
        query["UserIdSearch"] = request.UserIdSearch;
        query["RequireUserClaims"] = request.RequireUserClaim.ToString();
        query["isBanned"] = request.isBanned.ToString();
        string queryString = query.ToString();
        
        HttpResponseMessage response = await client.GetAsync($"api/user/query?{queryString}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }

        // return URI of the created resource.
        return await response.Content.ReadAsAsync<GetDetailedUsersResponse>();
    }

    public async Task<GetPlayerChatMessagesResponse> GetPlayerChatMessages(string playerId, int pagination = 1)
    {
        Console.WriteLine("GetUsers");
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["playerId"] = playerId;
        query["pagination"] = pagination.ToString();
        string queryString = query.ToString();
        
        HttpResponseMessage response = await client.GetAsync($"api/user/messages?{queryString}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }

        // return URI of the created resource.
        return await response.Content.ReadAsAsync<GetPlayerChatMessagesResponse>();
    }
}