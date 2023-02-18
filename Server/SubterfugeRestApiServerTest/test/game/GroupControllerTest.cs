using NUnit.Framework;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Test.util;
using Subterfuge.Remake.Server.Test.util.account;

namespace Subterfuge.Remake.Server.Test.test.game;

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
        
        gameRoom = (await TestUtils.GetClient().LobbyClient.CreateNewRoom(getCreateRoomRequest())).GetOrThrow();
        
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);
    }

    [Test]
    public async Task PlayersCannotStartAChatWithPlayersWhoAreNotInTheGame()
    {
        var request = new CreateMessageGroupRequest()
        {
            UserIdsInGroup = new List<string>() { userOne.User.Id, userTwo.User.Id, userNotInGame.User.Id }
        };

        var error = await TestUtils.GetClient().GroupClient.CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.VALIDATION_ERROR, error.ResponseDetail.ResponseType);
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);
        
        var error = await TestUtils.GetClient().GroupClient.CreateMessageGroup(request, gameRoom.GameConfiguration.Id);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.DUPLICATE, error.ResponseDetail.ResponseType);
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);

        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = "Hello World!" },
            gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsTrue(sendResponse.ResponseDetail.IsSuccess);
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);

        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = "Hello World!" },
            gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsTrue(sendResponse.ResponseDetail.IsSuccess);

        var messagesInGroup = await TestUtils.GetClient().GroupClient.GetMessages(new GetGroupMessagesRequest(),
            gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsTrue(messagesInGroup.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, messagesInGroup.GetOrThrow().Messages.Count);
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);

        var message = "Hello World!";
        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = message },
            gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsTrue(sendResponse.ResponseDetail.IsSuccess);

        var messageGroups = await TestUtils.GetClient().GroupClient.GetMessageGroups(gameRoom.GameConfiguration.Id);
        Assert.IsTrue(messageGroups.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, messageGroups.GetOrThrow().MessageGroups.Count);
        Assert.AreEqual(1, messageGroups.GetOrThrow().MessageGroups[0].Messages.Count);
        Assert.IsTrue(messageGroups.GetOrThrow().MessageGroups[0].Messages.Any(groupMessage => groupMessage.Message == message));
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);

        var sendResponse = await TestUtils.GetClient().GroupClient.SendMessage(
            new SendMessageRequest() { Message = "Hello World!" },
            gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsTrue(sendResponse.ResponseDetail.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var messagesInGroup = await TestUtils.GetClient().GroupClient.GetMessages(new GetGroupMessagesRequest(),
            gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsTrue(messagesInGroup.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, messagesInGroup.GetOrThrow().Messages.Count);
        Assert.IsTrue(messagesInGroup.GetOrThrow().Messages.Any(it => it.SentBy.Id == userOne.User.Id));
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);

        var messageToSend = new SendMessageRequest() { Message = "Hello World!" };
        await TestUtils.GetClient().GroupClient.SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().GroupClient.SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        await TestUtils.GetClient().GroupClient.SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);

        var messagesInGroup = await TestUtils.GetClient().GroupClient.GetMessages(new GetGroupMessagesRequest(),
            gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsTrue(messagesInGroup.ResponseDetail.IsSuccess);
        Assert.AreEqual(3, messagesInGroup.GetOrThrow().Messages.Count);
        
        // Ensure messages are ordered by the most recently recieved ;)
        Assert.AreEqual(messagesInGroup.GetOrThrow().Messages[0].SentBy.Id, userThree.User.Id);
        Assert.AreEqual(messagesInGroup.GetOrThrow().Messages[1].SentBy.Id, userTwo.User.Id);
        Assert.AreEqual(messagesInGroup.GetOrThrow().Messages[2].SentBy.Id, userOne.User.Id);
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
        Assert.AreEqual(response.ResponseDetail.IsSuccess, true);
        Assert.IsTrue(response.GetOrThrow().GroupId != null);
        
        TestUtils.GetClient().UserApi.SetToken(userNotInGame.Token);

        var messageToSend = new SendMessageRequest() { Message = "Hello World!" };
        var error = await TestUtils.GetClient().GroupClient
                .SendMessage(messageToSend, gameRoom.GameConfiguration.Id, response.GetOrThrow().GroupId);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, error.ResponseDetail.ResponseType);
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