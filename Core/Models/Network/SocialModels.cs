using System.Collections.Generic;

namespace Subterfuge.Remake.Api.Network
{
    public class BlockPlayerRequest { }

    public class BlockPlayerResponse { }

    public class UnblockPlayerRequest { }
    
    public class UnblockPlayerResponse { }

    public class ViewBlockedPlayersRequest { }

    public class ViewBlockedPlayersResponse
    {
        public List<User> BlockedUsers { get; set; }
    }
    
    public class SendFriendRequestRequest
    { 
        public string FriendId { get; set; }
    }

    public class AddAcceptFriendResponse { }

    public class DenyFriendRequestRequest
    {
        public string FriendId { get; set; }
    }

    public class DenyFriendRequestResponse { }

    public class ViewFriendRequestsRequest { }

    public class ViewFriendRequestsResponse
    {
        public List<User> FriendRequests { get; set; }
    }

    public class RemoveFriendRequest
    {
        public string FriendId { get; set; }
    }

    public class RemoveFriendResponse { }

    public class ViewFriendsRequest { }

    public class ViewFriendsResponse
    {
        public List<User> Friends { get; set; }
    }
}