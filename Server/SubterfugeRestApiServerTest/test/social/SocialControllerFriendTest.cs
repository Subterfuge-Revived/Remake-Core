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
        throw new NotImplementedException();
    }

    [Test]
    public void WhenAPlayerGetsAFriendRequestTheyCanSeeIt()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSendMultipleFriendRequests()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanRemoveAFriendRequest()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanAcceptAFriendRequest()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void AcceptingPlayerCanViewFriendAfterAcceptingRequest()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void OriginalPlayerCanViewFriendAfterOtherPlayerAcceptsRequest()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSendAFriendRequestToNonExistingPlayer()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSendFriendRequestToInvalidPlayerId()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotGetAFriendRequestFromABlockedPlayer()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSendAFriendRequestToABlockedPlayer()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void BlockingAPlayerRemovesThemAsAFriend()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void BlockingAPlayerWithAnIncomingFriendRequestRemovesTheFriendRequests()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void BlockingAPlayerAfterSendingThemAFriendRequestRemovesTheFriendRequest()
    {
        throw new NotImplementedException();
    }
}