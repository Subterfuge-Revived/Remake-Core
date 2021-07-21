using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;
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
            MongoConnector.FlushCollections();

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
            Assert.AreEqual(response.Status.IsSuccess, true);
        }
        
        [Test]
        public void PlayerCannotBlockInvalidPlayerId()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = "asdfasdfasdf"
            };

            var response = client.BlockPlayer(request);
            Assert.AreEqual(response.Status.IsSuccess, false);
            Assert.AreEqual(response.Status.Detail, ResponseType.PLAYER_DOES_NOT_EXIST.ToString());
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

            var errorResonse = client.BlockPlayer(request);
            Assert.AreEqual(errorResonse.Status.IsSuccess, false);
            Assert.AreEqual(errorResonse.Status.Detail, ResponseType.DUPLICATE.ToString());
        }
        
        [Test]
        public void AfterBlockingAPlayerTheyAppearOnTheBlockList()
        {
            BlockPlayerRequest request = new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            BlockPlayerResponse response = client.BlockPlayer(request);
            Assert.AreEqual(response.Status.IsSuccess, true);

            ViewBlockedPlayersResponse blockResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(blockResponse.Status.IsSuccess, true);
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
            Assert.AreEqual(response.Status.IsSuccess, true);

            ViewBlockedPlayersResponse blockResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(blockResponse.Status.IsSuccess, true);
            Assert.AreEqual(1, blockResponse.BlockedUsers.Count);
            Assert.IsTrue(blockResponse.BlockedUsers.Any( it => it.Id == authHelper.getAccountId("userTwo")));
            
            UnblockPlayerRequest unblockRequest = new UnblockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            UnblockPlayerResponse unblockResponse = client.UnblockPlayer(unblockRequest);
            Assert.AreEqual(unblockResponse.Status.IsSuccess, true);
            
            ViewBlockedPlayersResponse blockedUserResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(blockedUserResponse.Status.IsSuccess, true);
            Assert.AreEqual(0, blockedUserResponse.BlockedUsers.Count);
        }
        
        [Test]
        public void CannotUnblockNonValidPlayerId()
        {
            UnblockPlayerRequest unblockRequest = new UnblockPlayerRequest()
            {
                UserIdToBlock = "asdfasdf"
            };

            var response = client.UnblockPlayer(unblockRequest);
            Assert.AreEqual(response.Status.IsSuccess, false);
            Assert.AreEqual(response.Status.Detail, ResponseType.PLAYER_DOES_NOT_EXIST.ToString());
        }
        
        [Test]
        public void CannotUnblockPlayerWhoIsNotBlocked()
        {
            UnblockPlayerRequest unblockRequest = new UnblockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userTwo")
            };

            var response = client.UnblockPlayer(unblockRequest);
            Assert.AreEqual(response.Status.IsSuccess, false);
            Assert.AreEqual(response.Status.Detail, ResponseType.INVALID_REQUEST.ToString());
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
            Assert.AreEqual(friends.Status.IsSuccess, true);
            Assert.AreEqual(1, friends.Friends.Count);


            BlockPlayerResponse blockResponse = client.BlockPlayer(new BlockPlayerRequest()
            {
                UserIdToBlock = authHelper.getAccountId("userOne"),
            });
            Assert.AreEqual(blockResponse.Status.IsSuccess, true);
            
            // Ensure players are not friends
            ViewFriendsResponse friendsAfterBlock = client.ViewFriends(new ViewFriendsRequest());
            Assert.AreEqual(friendsAfterBlock.Status.IsSuccess, true);
            Assert.AreEqual(0, friendsAfterBlock.Friends.Count);
        }

        [Ignore("Blocking doesn't check if admin"), Test]
        public void CannotBlockAnAdmin()
        {
            SuperUser admin = authHelper.CreateSuperUser();

            var errorResponse = client.BlockPlayer(new BlockPlayerRequest()
            {
                UserIdToBlock = admin.DbUserModel.UserModel.Id,
            });
            Assert.AreEqual(errorResponse.Status.IsSuccess, false);
            Assert.AreEqual(errorResponse.Status.Detail, ResponseType.PERMISSION_DENIED.ToString());
            
            ViewBlockedPlayersResponse blockedUserResponse = client.ViewBlockedPlayers(new ViewBlockedPlayersRequest());
            Assert.AreEqual(errorResponse.Status.IsSuccess, true);
            Assert.AreEqual(0, blockedUserResponse.BlockedUsers.Count);
        }
    }
}