using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class GroupControllerTest
{
    private SubterfugeClient client = TestUtils.GetClient();

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
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