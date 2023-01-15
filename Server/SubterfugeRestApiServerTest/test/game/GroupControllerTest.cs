using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class GroupControllerTest
{
    private AccountRegistrationResponse userOne;
    private AccountRegistrationResponse userTwo;

    [SetUp]
    public async Task Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
        
        userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");
    }

    [Test]
    public void PlayersCanStartAChatWithAnotherPlayer()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayersCanStartAGroupChatWithMultipleOtherPlayers()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayersCannotStartAChatWithPlayersWhoAreNotInTheGame()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayersCannotStartAChatWithTheSamePeople()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayersCanSendMessagesToAGroup()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanViewTheirOwnMessageAfterSendingInAGroupChat()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanViewAnotherUsersMessageInAChat()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void AllPlayersCanChatInAGroupChatAndMessagesAreOrderedByDate()
    {
        throw new NotImplementedException();
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayersCannotSeeMessagesFromPlayersTheyHaveBlocked()
    {
        throw new NotImplementedException();
    }
}