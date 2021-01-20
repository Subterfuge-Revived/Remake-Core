using System;
using System.Linq;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class FriendRequestTest
    {
        SubterfugeClient.SubterfugeClient client;
        // private const String Hostname = "server"; // For docker
        private const String Hostname = "localhost"; // For local
        private const int Port = 5000;
            
        // private const String dbHost = "db"; // For docker
        private const String dbHost = "localhost"; // For local
        private const int dbPort = 6379;


        private AuthTestHelper authHelper;

        [SetUp]
        public void Setup()
        {
            RedisConnector db = new RedisConnector(dbHost, dbPort.ToString(), true);
            client = new SubterfugeClient.SubterfugeClient(Hostname, Port.ToString());
            
            // Clear the database every test.
            RedisConnector.Server.FlushDatabase();
            
            // Create two new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount("userOne");
            authHelper.createAccount("userTwo");
            authHelper.loginToAccount("userOne");
        }

        [Test]
        public void UserCanSendFriendRequestToOtherUser()
        {
            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            };

            SendFriendRequestResponse response = client.SendFriendRequest(request);
            Assert.IsTrue(response != null);
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
            Assert.IsTrue(response != null);

            authHelper.loginToAccount("userTwo");

            ViewFriendRequestsResponse friendRequestresponse = client.ViewFriendRequests(new ViewFriendRequestsRequest());
            Console.WriteLine(friendRequestresponse);
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
            Assert.IsTrue(response != null);

            var exception = Assert.Throws<RpcException>(() => client.SendFriendRequest(request));
            Assert.IsTrue(exception != null);
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.AlreadyExists);
            Assert.AreEqual(exception.Status.Detail, "You have already sent a request to this player.");

            authHelper.loginToAccount("userTwo");
        }

        [Test]
        public void PlayerCanRemoveAFriendRequest()
        {
            // Currently not possible but should be.
            Assert.IsTrue(false);
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
            Assert.IsTrue(response != null);

            authHelper.loginToAccount("userTwo");
            
            AcceptFriendRequestRequest friendRequest = new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            AcceptFriendRequestResponse acceptResponse = client.AcceptFriendRequest(friendRequest);
            Assert.IsTrue(acceptResponse != null);
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
            Assert.IsTrue(response != null);

            authHelper.loginToAccount("userTwo");
            
            AcceptFriendRequestRequest friendRequest = new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            AcceptFriendRequestResponse acceptResponse = client.AcceptFriendRequest(friendRequest);
            Assert.IsTrue(acceptResponse != null);
            
            ViewFriendsResponse friendResponse = client.ViewFriends(new ViewFriendsRequest());
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
            Assert.IsTrue(response != null);

            authHelper.loginToAccount("userTwo");
            
            AcceptFriendRequestRequest friendRequest = new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            };

            AcceptFriendRequestResponse acceptResponse = client.AcceptFriendRequest(friendRequest);
            Assert.IsTrue(acceptResponse != null);

            authHelper.loginToAccount("userOne");
            
            ViewFriendsResponse friendResponse = client.ViewFriends(new ViewFriendsRequest());
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

            var exception = Assert.Throws<RpcException>(() => client.SendFriendRequest(request));
            Console.WriteLine(exception);
            Assert.IsTrue(exception.Status.StatusCode == StatusCode.Unavailable);
        }
        
        [Test]
        public void PlayerCannotSendFriendRequestToInvalidGuid()
        {
            authHelper.loginToAccount("userOne");

            SendFriendRequestRequest request = new SendFriendRequestRequest()
            {
                FriendId = "asdfasdfasdf"
            };

            var exception = Assert.Throws<RpcException>(() => client.SendFriendRequest(request));
            Console.WriteLine(exception);
            Assert.IsTrue(exception.Status.StatusCode == StatusCode.InvalidArgument);
        }
    }
}