#nullable enable
using System.Net.Http.Headers;
using System.Web;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.account;

public class UserControllerClient
{
    private HttpClient client;

    public UserControllerClient(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<AuthorizationResponse> Login(AuthorizationRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("api/User/Login", request);
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
    
    public void LoginWithToken(string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
    
    public async Task<AccountRegistrationResponse> RegisterAccount(AccountRegistrationRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("api/User/RegisterAccount", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }

        // return URI of the created resource.
        var parsedResponse = await response.Content.ReadAsAsync<AccountRegistrationResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", parsedResponse.Token);
        return parsedResponse;
    }
    
    public async Task<GetUserResponse> GetUsers(
        int pagination = 1,
        string? username = null,
        string? email = null,
        string? deviceIdentifier = null,
        string? userId = null,
        UserClaim? claim = null,
        string? phone = null,
        Boolean isBanned = false
    ) {
        
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["pagination"] = pagination.ToString();
        query["username"] = username;
        query["email"] = email;
        query["deviceIdentifier"] = deviceIdentifier;
        query["userId"] = userId;
        query["claims"] = claim.ToString();
        query["isBanned"] = isBanned.ToString();
        query["phone"] = phone;
        string queryString = query.ToString();
        
        HttpResponseMessage response = await client.GetAsync($"api/User/GetUsers?{queryString}");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }

        // return URI of the created resource.
        return await response.Content.ReadAsAsync<GetUserResponse>();
    }
    
    
}