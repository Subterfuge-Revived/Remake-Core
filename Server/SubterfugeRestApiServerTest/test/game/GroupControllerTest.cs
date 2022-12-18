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
    }

    [Test]
    public void PlayersCanStartAGroupChatWithMultipleOtherPlayers()
    {
    }

    [Test]
    public void PlayersCannotStartAChatWithPlayersWhoAreNotInTheGame()
    {
    }

    [Test]
    public void PlayersCannotStartAChatWithTheSamePeople()
    {
    }

    [Test]
    public void PlayersCanSendMessagesToAGroup()
    {
    }

    [Test]
    public void PlayerCanViewTheirOwnMessageAfterSendingInAGroupChat()
    {
    }

    [Test]
    public void PlayerCanViewAnotherUsersMessageInAChat()
    {
    }

    [Test]
    public void AllPlayersCanChatInAGroupChatAndMessagesAreOrderedByDate()
    {
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayersCannotSeeMessagesFromPlayersTheyHaveBlocked()
    {
    }
}