using NUnit.Framework;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.social;

public class SocialControllerBlockTest
{
    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerCanBlockAnotherPlayer()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotBlockInvalidPlayerId()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotBlockTheSamePlayerTwice()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void AfterBlockingAPlayerTheyAppearOnTheBlockList()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanUnblockAPlayerAfterBlockingThem()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void CannotUnblockNonValidPlayerId()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void CannotUnblockPlayerWhoIsNotBlocked()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void BlockingAFriendRemovesThemAsAFriend()
    {
        throw new NotImplementedException();
    }

    [Ignore("Blocking doesn't check if admin"), Test]
    public void CannotBlockAnAdmin()
    {
        throw new NotImplementedException();
    }
}