using NUnit.Framework;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Test.util;
using Subterfuge.Remake.Server.Test.util.account;

namespace Subterfuge.Remake.Server.Test.test.social;

public class SocialControllerFriendTest
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
    public async Task PlayerCanSendFriendRequestToOtherPlayer()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
    }

    [Test]
    public async Task WhenAPlayerGetsAFriendRequestTheyCanSeeIt()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequests.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, friendRequests.GetOrThrow().FriendRequests.Count);
        Assert.IsTrue(friendRequests.GetOrThrow().FriendRequests.Any(request => request.Id == userOne.User.Id));
    }
    
    [Test]
    public async Task AfterSendingAFriendRequestTheRequestDoesNotAppearInTheOriginatingPlayersFriendRequestList()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);

        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userOne.User.Id);
        Assert.IsTrue(friendRequests.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, friendRequests.GetOrThrow().FriendRequests.Count);
    }

    [Test]
    public async Task PlayerCannotSendMultipleFriendRequests()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);

        var exception = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.DUPLICATE, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCanRemoveAFriendRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var removeRequest = await TestUtils.GetClient().SocialClient.RemoveRejectFriend(userOne.User.Id);
        Assert.IsTrue(removeRequest.ResponseDetail.IsSuccess);
        
        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequests.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, friendRequests.GetOrThrow().FriendRequests.Count);
    }

    [Test]
    public async Task PlayerCanAcceptAFriendRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var addFriendRequest = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        Assert.IsTrue(addFriendRequest.ResponseDetail.IsSuccess);
        
        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequests.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, friendRequests.GetOrThrow().FriendRequests.Count);
    }

    [Test]
    public async Task AcceptingPlayerCanViewFriendAfterAcceptingRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var addFriendRequest = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        Assert.IsTrue(addFriendRequest.ResponseDetail.IsSuccess);
        
        var friends = await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        Assert.IsTrue(friends.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, friends.GetOrThrow().Friends.Count);
        Assert.IsTrue(friends.GetOrThrow().Friends.Any(it => it.Id == userOne.User.Id));
    }

    [Test]
    public async Task OriginalPlayerCanViewFriendAfterOtherPlayerAcceptsRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        var addFriendRequest = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        Assert.IsTrue(addFriendRequest.ResponseDetail.IsSuccess);

        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        
        var friends = await TestUtils.GetClient().SocialClient.GetFriendList(userOne.User.Id);
        Assert.IsTrue(friends.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, friends.GetOrThrow().Friends.Count);
        Assert.IsTrue(friends.GetOrThrow().Friends.Any(it => it.Id == userTwo.User.Id));
    }
    
    [Test]
    public async Task NonAdminsCannotViewAnotherPlayersFriendList()
    {
        var exception = await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, exception.ResponseDetail.ResponseType);
    }
    
    [Test]
    public async Task AdminsCanViewAnotherPlayersFriendList()
    {
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        await TestUtils.CreateSuperUserAndLogin();
        var friendList = await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        Assert.IsTrue(friendList.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, friendList.GetOrThrow().Friends.Count);
        Assert.IsTrue(friendList.GetOrThrow().Friends.Any(it => it.Id == userOne.User.Id));
    }

    [Test]
    public async Task PlayerCannotSendAFriendRequestToNonExistingPlayer()
    {
        var exception = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest("InvalidUserId");
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCannotGetAFriendRequestFromABlockedPlayer()
    {
        // Block user two from player one.
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.ResponseDetail.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        // Try to add friend from player two to player one.
        var exception = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCannotSendAFriendRequestToABlockedPlayer()
    {
        // Block user two from player one.
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.ResponseDetail.IsSuccess);
        
        // Try to add friend from player one to player two.
        var exception = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task BlockingAPlayerWithAnIncomingFriendRequestRemovesTheFriendRequests()
    {
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var friendRequestsBeforeBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsBeforeBlock.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, friendRequestsBeforeBlock.GetOrThrow().FriendRequests.Count);
        Assert.IsTrue(friendRequestsBeforeBlock.GetOrThrow().FriendRequests.Any(request => request.Id == userOne.User.Id));

        await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userOne.User.Id);
        
        var friendRequestsAfterBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsAfterBlock.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, friendRequestsAfterBlock.GetOrThrow().FriendRequests.Count);
    }

    [Test]
    public async Task BlockingAPlayerAfterSendingThemAFriendRequestRemovesTheFriendRequest()
    {
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var friendRequestsBeforeBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsBeforeBlock.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, friendRequestsBeforeBlock.GetOrThrow().FriendRequests.Count);
        Assert.IsTrue(friendRequestsBeforeBlock.GetOrThrow().FriendRequests.Any(request => request.Id == userOne.User.Id));

        TestUtils.GetClient().UserApi.SetToken(userOne.Token);

        await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        var friendRequestsAfterBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsAfterBlock.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, friendRequestsAfterBlock.GetOrThrow().FriendRequests.Count);
    }
}