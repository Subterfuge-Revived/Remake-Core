using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;
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
        Assert.IsTrue(blockResponse.Status.IsSuccess);
    }

    [Test]
    public async Task PlayerCannotBlockInvalidPlayerId()
    {
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), "InvalidUserId");
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task AfterBlockingAPlayerTheyAppearOnTheBlockList()
    {
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.Status.IsSuccess);
        
        var blockList = await TestUtils.GetClient().SocialClient.ViewBlockedPlayers(userOne.User.Id);
        Assert.IsTrue(blockList.Status.IsSuccess);
        Assert.AreEqual(1, blockList.BlockedUsers.Count);
        Assert.IsTrue(blockList.BlockedUsers.Any(blockedUser => blockedUser.Id == userTwo.User.Id));
    }

    [Test]
    public async Task PlayerCanUnblockAPlayerAfterBlockingThem()
    {
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.Status.IsSuccess);
        
        var blockList = await TestUtils.GetClient().SocialClient.ViewBlockedPlayers(userOne.User.Id);
        Assert.IsTrue(blockList.Status.IsSuccess);
        Assert.AreEqual(1, blockList.BlockedUsers.Count);
        Assert.IsTrue(blockList.BlockedUsers.Any(blockedUser => blockedUser.Id == userTwo.User.Id));
        
        var unblockResonse = await TestUtils.GetClient().SocialClient.UnblockPlayer(new UnblockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(unblockResonse.Status.IsSuccess);
        
        var blockListAfterUnblock = await TestUtils.GetClient().SocialClient.ViewBlockedPlayers(userOne.User.Id);
        Assert.IsTrue(blockListAfterUnblock.Status.IsSuccess);
        Assert.AreEqual(0, blockListAfterUnblock.BlockedUsers.Count);
    }

    [Test]
    public void CannotUnblockNonValidPlayerId()
    {
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.UnblockPlayer(new UnblockPlayerRequest(), "InvalidUserId");
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
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
        Assert.IsTrue(friendsBeforeBlock.Status.IsSuccess);
        Assert.AreEqual(1, friendsBeforeBlock.Friends.Count);
        Assert.IsTrue(friendsBeforeBlock.Friends.Any(it => it.Id == userOne.User.Id));

        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userOne.User.Id);
        Assert.IsTrue(blockResponse.Status.IsSuccess);
        
        var friendsAfterBlock = await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        Assert.IsTrue(friendsAfterBlock.Status.IsSuccess);
        Assert.AreEqual(0, friendsAfterBlock.Friends.Count);
    }

    [Test]
    public async Task CannotBlockAnAdmin()
    {
        var admin = await TestUtils.CreateSuperUserAndLogin();

        TestUtils.GetClient().UserApi.SetToken(userOne.Token);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), admin.User.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }
}