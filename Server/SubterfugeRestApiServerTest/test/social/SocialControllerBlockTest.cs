using NUnit.Framework;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.social;

public class SocialControllerBlockTest
{
    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerCanBlockAnotherPlayer()
    {
    }

    [Test]
    public void PlayerCannotBlockInvalidPlayerId()
    {
    }

    [Test]
    public void PlayerCannotBlockTheSamePlayerTwice()
    {
    }

    [Test]
    public void AfterBlockingAPlayerTheyAppearOnTheBlockList()
    {
    }

    [Test]
    public void PlayerCanUnblockAPlayerAfterBlockingThem()
    {
    }

    [Test]
    public void CannotUnblockNonValidPlayerId()
    {
    }

    [Test]
    public void CannotUnblockPlayerWhoIsNotBlocked()
    {
    }

    [Test]
    public void BlockingAFriendRemovesThemAsAFriend()
    {
    }

    [Ignore("Blocking doesn't check if admin"), Test]
    public void CannotBlockAnAdmin()
    {
    }
}