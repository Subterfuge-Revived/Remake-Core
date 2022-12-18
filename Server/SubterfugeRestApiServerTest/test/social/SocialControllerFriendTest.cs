using NUnit.Framework;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.social;

public class SocialControllerFriendTest
{
    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerCanSendFriendRequestToOtherPlayer()
    {
    }

    [Test]
    public void WhenAPlayerGetsAFriendRequestTheyCanSeeIt()
    {
    }

    [Test]
    public void PlayerCannotSendMultipleFriendRequests()
    {
    }

    [Test]
    public void PlayerCanRemoveAFriendRequest()
    {
    }

    [Test]
    public void PlayerCanAcceptAFriendRequest()
    {
    }

    [Test]
    public void AcceptingPlayerCanViewFriendAfterAcceptingRequest()
    {
    }

    [Test]
    public void OriginalPlayerCanViewFriendAfterOtherPlayerAcceptsRequest()
    {
    }

    [Test]
    public void PlayerCannotSendAFriendRequestToNonExistingPlayer()
    {
    }

    [Test]
    public void PlayerCannotSendFriendRequestToInvalidPlayerId()
    {
    }

    [Test]
    public void PlayerCannotGetAFriendRequestFromABlockedPlayer()
    {
    }

    [Test]
    public void PlayerCannotSendAFriendRequestToABlockedPlayer()
    {
    }

    [Test]
    public void BlockingAPlayerRemovesThemAsAFriend()
    {
    }

    [Test]
    public void BlockingAPlayerWithAnIncomingFriendRequestRemovesTheFriendRequests()
    {
    }

    [Test]
    public void BlockingAPlayerAfterSendingThemAFriendRequestRemovesTheFriendRequest()
    {
    }
}