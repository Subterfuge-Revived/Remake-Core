using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.social;

public class SocialControllerBlockTest
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
    public async Task PlayerCanBlockAnotherPlayer()
    {
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.ResponseDetail.IsSuccess);
    }

    [Test]
    public async Task PlayerCannotBlockInvalidPlayerId()
    {
        var exception = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), "InvalidUserId");
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task AfterBlockingAPlayerTheyAppearOnTheBlockList()
    {
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.ResponseDetail.IsSuccess);
        
        var blockList = await TestUtils.GetClient().SocialClient.ViewBlockedPlayers(userOne.User.Id);
        Assert.IsTrue(blockList.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, blockList.GetOrThrow().BlockedUsers.Count);
        Assert.IsTrue(blockList.GetOrThrow().BlockedUsers.Any(blockedUser => blockedUser.Id == userTwo.User.Id));
    }

    [Test]
    public async Task PlayerCanUnblockAPlayerAfterBlockingThem()
    {
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.ResponseDetail.IsSuccess);
        
        var blockList = await TestUtils.GetClient().SocialClient.ViewBlockedPlayers(userOne.User.Id);
        Assert.IsTrue(blockList.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, blockList.GetOrThrow().BlockedUsers.Count);
        Assert.IsTrue(blockList.GetOrThrow().BlockedUsers.Any(blockedUser => blockedUser.Id == userTwo.User.Id));
        
        var unblockResonse = await TestUtils.GetClient().SocialClient.UnblockPlayer(new UnblockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(unblockResonse.ResponseDetail.IsSuccess);
        
        var blockListAfterUnblock = await TestUtils.GetClient().SocialClient.ViewBlockedPlayers(userOne.User.Id);
        Assert.IsTrue(blockListAfterUnblock.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, blockListAfterUnblock.GetOrThrow().BlockedUsers.Count);
    }

    [Test]
    public async Task CannotUnblockNonValidPlayerId()
    {
        var exception = await TestUtils.GetClient().SocialClient.UnblockPlayer(new UnblockPlayerRequest(), "InvalidUserId");
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.ResponseDetail.ResponseType);
    }

    [Ignore("If the player is not blocked, unblocking is going to do nothing anyway.")]
    public void CannotUnblockPlayerWhoIsNotBlocked()
    {
        throw new NotImplementedException();
    }

    [Test]
    public async Task BlockingAPlayerRemovesThemAsAFriend()
    {
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        
        var friendsBeforeBlock = await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        Assert.IsTrue(friendsBeforeBlock.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, friendsBeforeBlock.GetOrThrow().Friends.Count);
        Assert.IsTrue(friendsBeforeBlock.GetOrThrow().Friends.Any(it => it.Id == userOne.User.Id));

        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userOne.User.Id);
        Assert.IsTrue(blockResponse.ResponseDetail.IsSuccess);
        
        var friendsAfterBlock = await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        Assert.IsTrue(friendsAfterBlock.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, friendsAfterBlock.GetOrThrow().Friends.Count);
    }

    [Test]
    public async Task CannotBlockAnAdmin()
    {
        var admin = await TestUtils.CreateSuperUserAndLogin();

        TestUtils.GetClient().UserApi.SetToken(userOne.Token);

        var exception = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), admin.User.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, exception.ResponseDetail.ResponseType);
    }
}