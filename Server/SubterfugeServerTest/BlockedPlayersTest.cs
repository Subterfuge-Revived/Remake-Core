using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class BlockedPlayersTest
    {
        SubterfugeClient.SubterfugeClient client;
        private AuthTestHelper authHelper;

        [SetUp]
        public void Setup()
        {
            client = ClientHelper.GetClient();
            
            // Clear the database every test.
            RedisConnector.Server.FlushDatabase();
            
            // Create two new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount("userOne");
            authHelper.createAccount("userTwo");
            authHelper.loginToAccount("userOne");
        }

        [Test]
        public void PlayerCanBlockAnotherPlayer()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            BlockPlayerResponse response = client.BlockPlayer(request);
            Assert.IsTrue(response != null);
        }
        
        [Test]
        public void PlayerCannotBlockInvalidPlayerId()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = "asdfasdfasdf"
            };

            var exception = Assert.Throws<RpcException>(() => client.BlockPlayer(request));
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.NotFound);
        }
        
        [Test]
        public void PlayerCannotBlockTheSamePlayerTwice()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            BlockPlayerResponse response = client.BlockPlayer(request);
            Assert.IsTrue(response != null);

            var exception = Assert.Throws<RpcException>(() => client.BlockPlayer(request));
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.AlreadyExists);
        }
        
        [Test]
        public void AfterBlockingAPlayerTheyAppearOnTheBlockList()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            BlockPlayerResponse response = client.BlockPlayer(request);
            Assert.IsTrue(response != null);

            ViewBlockedPlayersResponse blockResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(1, blockResponse.BlockedUsers.Count);
            Assert.IsTrue(blockResponse.BlockedUsers.Any( it => it.Id == authHelper.getAccountId("userTwo")));
        }
        
        [Test]
        public void PlayerCanUnblockAPlayerAfterBlockingThem()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            BlockPlayerResponse response = client.BlockPlayer(request);
            Assert.IsTrue(response != null);

            ViewBlockedPlayersResponse blockResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(1, blockResponse.BlockedUsers.Count);
            Assert.IsTrue(blockResponse.BlockedUsers.Any( it => it.Id == authHelper.getAccountId("userTwo")));
            
            UnblockPlayerRequest unblockRequest = new UnblockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            UnblockPlayerResponse unblockResponse = client.UnblockPlayer(unblockRequest);
            Assert.IsTrue(unblockResponse != null);
            
            ViewBlockedPlayersResponse blockedUserResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(0, blockedUserResponse.BlockedUsers.Count);
        }
        
        [Test]
        public void CannotUnblockNonValidPlayerId()
        {
            UnblockPlayerRequest unblockRequest = new UnblockPlayerRequest()
            {
                UserIdToBlock = "asdfasdf"
            };

            var exception = Assert.Throws<RpcException>(() => client.UnblockPlayer(unblockRequest));
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.NotFound);
        }
        
        [Test]
        public void CannotUnblockPlayerWhoIsNotBlocked()
        {
            UnblockPlayerRequest unblockRequest = new UnblockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            var exception = Assert.Throws<RpcException>(() => client.UnblockPlayer(unblockRequest));
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.NotFound);
        }

        [Test]
        public void BlockingAFriendRemovesThemAsAFriend()
        {
            client.SendFriendRequest(new SendFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userTwo")
            });

            authHelper.loginToAccount("userTwo");
            client.AcceptFriendRequest(new AcceptFriendRequestRequest()
            {
                FriendId = authHelper.getAccountId("userOne"),
            });
            
            // Ensure players are friends
            ViewFriendsResponse friends = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(1, friends.Friends.Count);


            BlockPlayerResponse blockResponse = client.BlockPlayer(new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userOne"),
            });
            Assert.IsTrue(blockResponse != null);
            
            // Ensure players are not friends
            ViewFriendsResponse friendsAfterBlock = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(0, friendsAfterBlock.Friends.Count);
        }
        
        [Test]
        public void CannotBlockAnAdmin()
        {
            SuperUser admin = authHelper.CreateSuperUser();

            var exception = Assert.Throws<RpcException>(() => client.BlockPlayer(new BlockPlayerRequest()
            {
                UserIdToBlock = admin.userModel.UserModel.Id,
            }));
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.PermissionDenied);
            
            ViewBlockedPlayersResponse blockedUserResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(0, blockedUserResponse.BlockedUsers.Count);
        }
    }
}