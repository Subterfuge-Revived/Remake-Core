using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class GroupControllerTest
{
    private AccountRegistrationResponse userOne;
    private AccountRegistrationResponse userTwo;
    private AccountRegistrationResponse userThree;
    private AccountRegistrationResponse userNotInGame;

    private CreateRoomResponse gameRoom;
    
    [SetUp]
    public async Task Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
        
        userNotInGame = await AccountUtils.AssertRegisterAccountAndAuthorized("UserOutOfGame");
        userThree = await AccountUtils.AssertRegisterAccountAndAuthorized("UserThree");
        userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");
        
        gameRoom = await TestUtils.GetClient().LobbyClient.CreateNewRoom(getCreateRoomRequest());
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), gameRoom.GameConfiguration.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), gameRoom.GameConfiguration.Id);
        // Game has begun.
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
    }

    [Test]
    public async Task PlayersCanStartAChatWithAnotherPlayer()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);
    }

    [Test]
    public async Task PlayersCanStartAGroupChatWithMultipleOtherPlayers()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id, userThree.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);
    }

    [Test]
    public async Task PlayersCannotStartAChatWithPlayersWhoAreNotInTheGame()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id, userNotInGame.User.Id }
        };

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GroupClient.CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayersCannotStartAChatWithTheSamePeople()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);
        
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GroupClient.CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayersCanSendMessagesToAGroup()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);

        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = "Hello World!" },
            gameRoom.GameConfiguration.Id, response.GroupId);
        Assert.IsTrue(sendResponse.Status.IsSuccess);
    }

    [Test]
    public async Task PlayerCanViewTheirOwnMessageAfterSendingInAGroupChat()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);

        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = "Hello World!" },
            gameRoom.GameConfiguration.Id, response.GroupId);
        Assert.IsTrue(sendResponse.Status.IsSuccess);

        var messagesInGroup = await TestUtils.GetClient().GroupClient.GetMessages(new GetGroupMessagesRequest(),
            gameRoom.GameConfiguration.Id, response.GroupId);
        Assert.IsTrue(messagesInGroup.Status.IsSuccess);
        Assert.AreEqual(1, messagesInGroup.Messages.Count);
    }
    
    [Test]
    public async Task GettingGroupDataReturnsTheLatestMessagesInTheGroupByDefault()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);

        var message = "Hello World!";
        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = message },
            gameRoom.GameConfiguration.Id, response.GroupId);
        Assert.IsTrue(sendResponse.Status.IsSuccess);

        var messageGroups = await TestUtils.GetClient().GroupClient.GetMessageGroups(gameRoom.GameConfiguration.Id);
        Assert.IsTrue(messageGroups.Status.IsSuccess);
        Assert.AreEqual(1, messageGroups.MessageGroups.Count);
        Assert.AreEqual(1, messageGroups.MessageGroups[0].Messages.Count);
        Assert.IsTrue(messageGroups.MessageGroups[0].Messages.Any(groupMessage => groupMessage.Message == message));
    }

    [Test]
    public async Task PlayerCanViewAnotherUsersMessageInAChat()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);

        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = "Hello World!" },
            gameRoom.GameConfiguration.Id, response.GroupId);
        Assert.IsTrue(sendResponse.Status.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var messagesInGroup = await TestUtils.GetClient().GroupClient.GetMessages(new GetGroupMessagesRequest(),
            gameRoom.GameConfiguration.Id, response.GroupId);
        Assert.IsTrue(messagesInGroup.Status.IsSuccess);
        Assert.AreEqual(1, messagesInGroup.Messages.Count);
        Assert.IsTrue(messagesInGroup.Messages.Any(it => it.SentBy.Id == userOne.User.Id));
    }

    [Test]
    public async Task AllPlayersCanChatInAGroupChatAndMessagesAreOrderedByDate()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id, userThree.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);

        var messageToSend = new SendMessageRequest() { Message = "Hello World!" };
        await TestUtils.GetClient().GroupClient.SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GroupId);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().GroupClient.SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GroupId);
        
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        await TestUtils.GetClient().GroupClient.SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GroupId);

        var messagesInGroup = await TestUtils.GetClient().GroupClient.GetMessages(new GetGroupMessagesRequest(),
            gameRoom.GameConfiguration.Id, response.GroupId);
        Assert.IsTrue(messagesInGroup.Status.IsSuccess);
        Assert.AreEqual(3, messagesInGroup.Messages.Count);
        
        // Ensure messages are ordered by the most recently recieved ;)
        Assert.AreEqual(messagesInGroup.Messages[0].SentBy.Id, userThree.User.Id);
        Assert.AreEqual(messagesInGroup.Messages[1].SentBy.Id, userTwo.User.Id);
        Assert.AreEqual(messagesInGroup.Messages[2].SentBy.Id, userOne.User.Id);
    }

    [Test]
    public async Task CannotSendMessageToGroupYouAreNotIn()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id, userThree.User.Id }
        };

        var response = await TestUtils.GetClient().GroupClient
            .CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(response.Status.IsSuccess, true);
        Assert.IsTrue(response.GroupId != null);
        
        TestUtils.GetClient().UserApi.SetToken(userNotInGame.Token);

        var messageToSend = new SendMessageRequest() { Message = "Hello World!" };
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GroupClient
                .SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GroupId);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayersCannotSeeMessagesFromPlayersTheyHaveBlocked()
    {
        throw new NotImplementedException();
    }
    
    private CreateRoomRequest getCreateRoomRequest()
    {
        return new CreateRoomRequest()
        {
            GameSettings = new GameSettings()
            {
                IsAnonymous = false,
                Goal = Goal.Domination,
                IsRanked = false,
                MaxPlayers = 3,
                MinutesPerTick = (1.0 / 60.0), // One second per tick
            },
            RoomName = "TestRoom",
            MapConfiguration = new MapConfiguration()
            {
                Seed = 123123,
                OutpostsPerPlayer = 3,
                MinimumOutpostDistance = 100,
                MaximumOutpostDistance = 1200,
                DormantsPerPlayer = 3,
                OutpostDistribution = new OutpostDistribution()
                {
                    FactoryWeight = 0.33f,
                    GeneratorWeight = 0.33f,
                    WatchtowerWeight = 0.33f,
                }
            }
        };
    }
}