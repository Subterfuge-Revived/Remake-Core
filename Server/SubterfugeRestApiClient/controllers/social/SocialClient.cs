using SubterfugeCore.Models.GameEvents;
using SubterfugeCore.Models.GameEvents.Api;
using SubterfugeRestApiClient.controllers.exception;

namespace SubterfugeRestApiClient.controllers.social;

public class SocialClient : ISubterfugeSocialApi
{
    private HttpClient client;

    public SocialClient(HttpClient client)
    {
        this.client = client;
    }

    public async Task<UnblockPlayerResponse> UnblockPlayer(UnblockPlayerRequest request, string userId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/user/{userId}/unblock", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<UnblockPlayerResponse>();
    }

    public async Task<ViewBlockedPlayersResponse> ViewBlockedPlayers(string userId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/user/{userId}/blocks");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<ViewBlockedPlayersResponse>();
    }

    public async Task<AddAcceptFriendResponse> AddAcceptFriendRequest(string userId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/user/{userId}/addFriend");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<AddAcceptFriendResponse>();
    }

    public async Task<ViewFriendRequestsResponse> ViewFriendRequests(string userId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/user/{userId}/friendRequests");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<ViewFriendRequestsResponse>();
    }
    
    public async Task<BlockPlayerResponse> BlockPlayer(BlockPlayerRequest request, string userId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync($"api/user/{userId}/block", request);
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<BlockPlayerResponse>();
    }
    
    public async Task<DenyFriendRequestResponse> RemoveRejectFriend(string userId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/user/{userId}/removeFriend");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<DenyFriendRequestResponse>();
    }
    
    public async Task<ViewFriendsResponse> GetFriendList(string userId)
    {
        HttpResponseMessage response = await client.GetAsync($"api/user/{userId}/friends");
        if (!response.IsSuccessStatusCode)
        {
            throw await SubterfugeClientException.CreateFromResponseMessage(response);
        }
        return await response.Content.ReadAsAsync<ViewFriendsResponse>();
    }
}