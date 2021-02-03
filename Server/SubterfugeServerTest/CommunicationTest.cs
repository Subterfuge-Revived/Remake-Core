using System.Runtime.CompilerServices;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class CommunicationTest
    {
        SubterfugeClient.SubterfugeClient client;
        private AuthTestHelper authHelper;

        private string playerOneInGame = "playerOneInGame";
        private string playerTwoInGame = "playerTwoInGame";
        private string playerThreeInGame = "playerThreeInGame";
        private string playerOutOfGame = "playerOutOfGame";
        private string gameId;
        
        [SetUp]
        public void Setup()
        {
            client = ClientHelper.GetClient();
            
            // Clear the database every test.
            RedisConnector.Server.FlushDatabase();
            
            // Create two new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount(playerOneInGame);
            authHelper.createAccount(playerTwoInGame);
            authHelper.createAccount(playerThreeInGame);
            authHelper.createAccount(playerOutOfGame);
            authHelper.loginToAccount(playerOneInGame);
            gameId = client.CreateNewRoom(new CreateRoomRequest()
            {
                MaxPlayers = 3,
                RoomName = "TestRoom",
                MinutesPerTick = (1.0/60.0), // One second per tick
            }).CreatedRoom.RoomId;
            authHelper.loginToAccount(playerTwoInGame);
            client.JoinRoom(new JoinRoomRequest()
            {
                RoomId = gameId
            });
            authHelper.loginToAccount(playerThreeInGame);
            client.JoinRoom(new JoinRoomRequest()
            {
                RoomId = gameId
            });
            
            // Game has begun.
            authHelper.loginToAccount(playerOneInGame);
        }

        [Test]
        public void PlayersCanStartAChatWithAnotherPlayer()
        {
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, true);
            Assert.IsTrue(groupResponse.GroupId != null);
        }

        [Test]
        public void PlayersCanStartAGroupChatWithMultipleOtherPlayers()
        {
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerThreeInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, true);
            Assert.IsTrue(groupResponse.GroupId != null);
        }

        [Test]
        public void PlayersCannotStartAChatWithPlayersWhoAreNotInTheGame()
        {
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOutOfGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, false);
        }

        [Test]
        public void PlayersCannotStartAChatWithTheSamePeople()
        {
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, true);
            Assert.IsTrue(groupResponse.GroupId != null);
            
            CreateMessageGroupResponse groupResponseTwo = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponseTwo.Status.IsSuccess, false);
        }

        [Test]
        public void PlayersCanSendMessagesToAGroup()
        {
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, true);
            Assert.IsTrue(groupResponse.GroupId != null);
            var groupId = groupResponse.GroupId;

            SendMessageResponse response = client.SendMessage(new SendMessageRequest()
            {
                GroupId = groupId,
                Message = "Hello!",
                RoomId = gameId,
            });
            Assert.AreEqual(response.Status.IsSuccess, true);
        }

        [Test]
        public void PlayerCanViewTheirOwnMessageAfterSendingInAGroupChat()
        { 
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, true);
            Assert.IsTrue(groupResponse.GroupId != null);
            var groupId = groupResponse.GroupId;

            SendMessageResponse response = client.SendMessage(new SendMessageRequest()
            {
                GroupId = groupId,
                Message = "Hello!",
                RoomId = gameId,
            });
            Assert.AreEqual(response.Status.IsSuccess, true);

            GetGroupMessagesResponse messageResponse = client.GetGroupMessages(new GetGroupMessagesRequest()
            {
                GroupId = groupId,
                RoomId = gameId,
                Pagination = 1,
            });
            Assert.AreEqual(messageResponse.Status.IsSuccess, true);
            Assert.AreEqual(messageResponse.Group.Messages.Count, 1);
            Assert.AreEqual(messageResponse.Group.Messages[0].SenderId, authHelper.getAccountId(playerOneInGame));
        }
        
        [Test]
        public void PlayerCanViewAnotherUsersMessageInAChat()
        { 
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, true);
            Assert.IsTrue(groupResponse.GroupId != null);
            var groupId = groupResponse.GroupId;
            
            SendMessageRequest messageRequest = new SendMessageRequest()
            {
                GroupId = groupId,
                Message = "Hello!",
                RoomId = gameId,
            };

            client.SendMessage(messageRequest);
            client.SendMessage(messageRequest);
            client.SendMessage(messageRequest);
            client.SendMessage(messageRequest);
            client.SendMessage(messageRequest);
            client.SendMessage(messageRequest);

            authHelper.loginToAccount(playerTwoInGame);
            
            GetGroupMessagesResponse messageResponse = client.GetGroupMessages(new GetGroupMessagesRequest()
            {
                GroupId = groupId,
                RoomId = gameId,
                Pagination = 1,
            });
            Assert.AreEqual(messageResponse.Status.IsSuccess, true);
            Assert.AreEqual(6, messageResponse.Group.Messages.Count);
            Assert.AreEqual(messageResponse.Group.Messages[0].SenderId, authHelper.getAccountId(playerOneInGame));
        }

        [Test]
        public void AllPlayersCanChatInAGroupChatAndMessagesAreOrderedByDate()
        {
            CreateMessageGroupRequest request = new CreateMessageGroupRequest()
            {
                RoomId = gameId,
            };
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerTwoInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerOneInGame));
            request.UserIdsInGroup.Add(authHelper.getAccountId(playerThreeInGame));

            CreateMessageGroupResponse groupResponse = client.CreateMessageGroup(request);
            Assert.AreEqual(groupResponse.Status.IsSuccess, true);
            Assert.IsTrue(groupResponse.GroupId != null);
            var groupId = groupResponse.GroupId;
            
            SendMessageRequest messageRequest = new SendMessageRequest()
            {
                GroupId = groupId,
                Message = "Hello!",
                RoomId = gameId,
            };

            client.SendMessage(messageRequest);

            authHelper.loginToAccount(playerTwoInGame);
            client.SendMessage(messageRequest);
            
            authHelper.loginToAccount(playerThreeInGame);
            client.SendMessage(messageRequest);
            
            GetGroupMessagesResponse messageResponse = client.GetGroupMessages(new GetGroupMessagesRequest()
            {
                GroupId = groupId,
                RoomId = gameId,
                Pagination = 1,
            });
            Assert.AreEqual(messageResponse.Status.IsSuccess, true);
            Assert.AreEqual(3, messageResponse.Group.Messages.Count);
            
            // Ensure messages are ordered by the most recently recieved ;)
            Assert.AreEqual(messageResponse.Group.Messages[0].SenderId, authHelper.getAccountId(playerThreeInGame));
            Assert.AreEqual(messageResponse.Group.Messages[1].SenderId, authHelper.getAccountId(playerTwoInGame));
            Assert.AreEqual(messageResponse.Group.Messages[2].SenderId, authHelper.getAccountId(playerOneInGame));
        }

        [Test]
        public void PlayersCannotSeeMessagesFromPlayersTheyHaveBlocked()
        {
            Assert.IsTrue(false);
        }
    }
}