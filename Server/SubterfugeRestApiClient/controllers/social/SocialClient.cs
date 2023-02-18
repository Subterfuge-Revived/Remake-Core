using Subterfuge.Remake.Api.Client.controllers.Client;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Api.Network.Api;

namespace Subterfuge.Remake.Api.Client.controllers.social;

public class SocialClient : ISubterfugeSocialApi
{
    private SubterfugeHttpClient client;

    public SocialClient(SubterfugeHttpClient client)
    {
        this.client = client;
    }

    public async Task<SubterfugeResponse<UnblockPlayerResponse>> UnblockPlayer(UnblockPlayerRequest request, string userId)
    {
        return await client.Post<UnblockPlayerRequest, UnblockPlayerResponse>($"api/user/{userId}/unblock", request);
    }

    public async Task<SubterfugeResponse<ViewBlockedPlayersResponse>> ViewBlockedPlayers(string userId)
    {
        return await client.Get<ViewBlockedPlayersResponse>($"api/user/{userId}/blocks", null);
    }

    public async Task<SubterfugeResponse<AddAcceptFriendResponse>> AddAcceptFriendRequest(string userId)
    {
        return await client.Get<AddAcceptFriendResponse>($"api/user/{userId}/addFriend", null);
    }

    public async Task<SubterfugeResponse<ViewFriendRequestsResponse>> ViewFriendRequests(string userId)
    {
        return await client.Get<ViewFriendRequestsResponse>($"api/user/{userId}/friendRequests", null);
    }
    
    public async Task<SubterfugeResponse<BlockPlayerResponse>> BlockPlayer(BlockPlayerRequest request, string userId)
    {
        return await client.Post<BlockPlayerRequest, BlockPlayerResponse>($"api/user/{userId}/block", request);
    }
    
    public async Task<SubterfugeResponse<DenyFriendRequestResponse>> RemoveRejectFriend(string userId)
    {
        return await client.Get<DenyFriendRequestResponse>($"api/user/{userId}/removeFriend", null);
    }
    
    public async Task<SubterfugeResponse<ViewFriendsResponse>> GetFriendList(string userId)
    {
        return await client.Get<ViewFriendsResponse>($"api/user/{userId}/friends", null);
    }
}