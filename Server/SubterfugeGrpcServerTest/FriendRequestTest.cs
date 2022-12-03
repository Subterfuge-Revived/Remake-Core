using System;
using System.Linq;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Responses;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class FriendRequestTest
    {
        SubterfugeClient.SubterfugeClient client;
        private AuthTestHelper authHelper;

        [SetUp]
        public void Setup()
        {
            client = ClientHelper.GetClient();
            
            // Clear the database every test.
            MongoConnector.FlushCollections();
            
            // Create two new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount("userOne");
            authHelper.createAccount("userTwo");
            authHelper.loginToAccount("userOne");
        }

        [Test]
        public void PlayerCanSendFriendRequestToOtherPlayer()
        {
            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);
        }
        
        [Test]
        public void WhenAPlayerGetsAFriendRequestTheyCanSeeIt()
        {
            authHelper.loginToAccount("userOne");
            
            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            authHelper.loginToAccount("userTwo");

            ViewFriendRequestsResponse friendRequestresponse = client.ViewFriendRequests(new ViewFriendRequestsRequest());
            Assert.AreEqual(friendRequestresponse.Status.IsSuccess, true);
            Assert.AreEqual(1, friendRequestresponse.IncomingFriends.Count);
            Assert.IsTrue(friendRequestresponse.IncomingFriends.Any((user) => user.Id == authHelper.getAccountId("userOne")));
        }

        [Test]
        public void PlayerCannotSendMultipleFriendRequests()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            var errorResponse = client.SendFriendRequest(request);
            Assert.AreEqual(errorResponse.Status.IsSuccess, false);
            Assert.AreEqual(errorResponse.Status.Detail, ResponseType.DUPLICATE.ToString());
        }

        [Test]
        public void PlayerCanRemoveAFriendRequest()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            authHelper.loginToAccount("userTwo");
            
            DenyFriendRequestRequest friendRequest = new DenyFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            DenyFriendRequestResponse acceptResponse = client.DenyFriendRequest(friendRequest);
            Assert.AreEqual(acceptResponse.Status.IsSuccess, true);
            
            ViewFriendRequestsResponse friendResponse = client.ViewFriendRequests(new ViewFriendRequestsRequest());
            Assert.AreEqual(friendResponse.Status.IsSuccess, true);
            Assert.AreEqual(0, friendResponse.IncomingFriends.Count);
        }
        
        [Test]
        public void PlayerCanAcceptAFriendRequest()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            authHelper.loginToAccount("userTwo");
            
            AcceptFriendRequestRequest friendRequest = new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            AcceptFriendRequestResponse acceptResponse = client.AcceptFriendRequest(friendRequest);
            Assert.AreEqual(acceptResponse.Status.IsSuccess, true);
        }
        
        [Test]
        public void AcceptingPlayerCanViewFriendAfterAcceptingRequest()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            authHelper.loginToAccount("userTwo");
            
            AcceptFriendRequestRequest friendRequest = new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            AcceptFriendRequestResponse acceptResponse = client.AcceptFriendRequest(friendRequest);
            Assert.AreEqual(acceptResponse.Status.IsSuccess, true);
            
            ViewFriendsResponse friendResponse = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(friendResponse.Status.IsSuccess, true);
            Assert.AreEqual(1, friendResponse.Friends.Count);
            Assert.IsTrue(friendResponse.Friends.Any((friend) => friend.Id == authHelper.getAccountId("userOne")));
        }
        
        [Test]
        public void OriginalPlayerCanViewFriendAfterOtherPlayerAcceptsRequest()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            authHelper.loginToAccount("userTwo");
            
            AcceptFriendRequestRequest friendRequest = new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            AcceptFriendRequestResponse acceptResponse = client.AcceptFriendRequest(friendRequest);
            Assert.AreEqual(acceptResponse.Status.IsSuccess, true);

            authHelper.loginToAccount("userOne");
            
            ViewFriendsResponse friendResponse = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(friendResponse.Status.IsSuccess, true);
            Assert.AreEqual(1, friendResponse.Friends.Count);
            Assert.IsTrue(friendResponse.Friends.Any((friend) => friend.Id == authHelper.getAccountId("userTwo")));
        }

        [Test]
        public void PlayerCannotSendAFriendRequestToNonExistingPlayer()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = Guid.NewGuid().ToString()
            };

            var exception = client.SendFriendRequest(request);
            Assert.AreEqual(exception.Status.IsSuccess, false);
            Assert.AreEqual(exception.Status.Detail, ResponseType.PLAYER_DOES_NOT_EXIST.ToString());
        }
        
        [Test]
        public void PlayerCannotSendFriendRequestToInvalidPlayerId()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = "asdfasdfasdf"
            };

           var exception = client.SendFriendRequest(request);
           Assert.AreEqual(exception.Status.IsSuccess, false);
           Assert.AreEqual(exception.Status.Detail, ResponseType.PLAYER_DOES_NOT_EXIST.ToString());
        }

        [Test]
        public void PlayerCannotGetAFriendRequestFromABlockedPlayer()
        {
            authHelper.loginToAccount("userOne");
            
            BlockPlayerRequest blockPlayerRequest = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            client.BlockPlayer(blockPlayerRequest);
            authHelper.loginToAccount("userTwo");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne")
            };

            var exception = client.SendFriendRequest(request);
            Assert.AreEqual(exception.Status.IsSuccess, false);
            Assert.AreEqual(exception.Status.Detail, ResponseType.PLAYER_IS_BLOCKED.ToString());

            authHelper.loginToAccount("userOne");
            
            var friendResponse = client.ViewFriendRequests(new ViewFriendRequestsRequest());
            Assert.AreEqual(friendResponse.Status.IsSuccess, true);
            Assert.AreEqual(0, friendResponse.IncomingFriends.Count);
        }
        
        [Test]
        public void PlayerCannotSendAFriendRequestToABlockedPlayer()
        {
            authHelper.loginToAccount("userOne");
            
            BlockPlayerRequest blockPlayerRequest = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            client.BlockPlayer(blockPlayerRequest);

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            var exception = client.SendFriendRequest(request);
            Assert.AreEqual(exception.Status.IsSuccess, false);
            Assert.AreEqual(exception.Status.Detail, ResponseType.PLAYER_IS_BLOCKED.ToString());
        }

        [Test]
        public void BlockingAPlayerRemovesThemAsAFriend()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            authHelper.loginToAccount("userTwo");
            
            AcceptFriendRequestRequest friendRequest = new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            AcceptFriendRequestResponse acceptResponse = client.AcceptFriendRequest(friendRequest);
            Assert.AreEqual(acceptResponse.Status.IsSuccess, true);
            
            ViewFriendsResponse friendResponse = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(friendResponse.Status.IsSuccess, true);
            Assert.AreEqual(1, friendResponse.Friends.Count);
            Assert.IsTrue(friendResponse.Friends.Any((friend) => friend.Id == authHelper.getAccountId("userOne")));
            
            BlockPlayerRequest blockRequest = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userOne")
            };

            BlockPlayerResponse blockResponse = client.BlockPlayer(blockRequest);
            Assert.AreEqual(blockResponse.Status.IsSuccess, true);
            
            // Make sure the players are not friends.
            ViewFriendsResponse blockFriendListResponse = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(blockFriendListResponse.Status.IsSuccess, true);
            Assert.AreEqual(0, blockFriendListResponse.Friends.Count);

            authHelper.loginToAccount("userOne");
            
            // Make sure the players are not friends.
            ViewFriendsResponse blockFriendListResponseUserOne = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(blockFriendListResponseUserOne.Status.IsSuccess, true);
            Assert.AreEqual(0, blockFriendListResponseUserOne.Friends.Count);
        }
        
        [Test]
        public void BlockingAPlayerWithAnIncomingFriendRequestRemovesTheFriendRequests()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            authHelper.loginToAccount("userTwo");

            BlockPlayerRequest blockRequest = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userOne")
            };

            client.BlockPlayer(blockRequest);
            
            // Make sure the players are not friends.
            ViewFriendRequestsResponse blockFriendListResponse = client.ViewFriendRequests(new ViewFriendRequestsRequest());
            Assert.AreEqual(blockFriendListResponse.Status.IsSuccess, true);
            Assert.AreEqual(0, blockFriendListResponse.IncomingFriends.Count);
        }
        
        [Test]
        public void BlockingAPlayerAfterSendingThemAFriendRequestRemovesTheFriendRequest()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            BlockPlayerRequest blockRequest = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            client.BlockPlayer(blockRequest);
            authHelper.loginToAccount("userTwo");
            
            // Make sure the players are not friends.
            ViewFriendRequestsResponse blockFriendListResponse = client.ViewFriendRequests(new ViewFriendRequestsRequest());
            Assert.AreEqual(blockFriendListResponse.Status.IsSuccess, true);
            Assert.AreEqual(0, blockFriendListResponse.IncomingFriends.Count);
        }
        
    }
}