using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.social;

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
        Assert.IsTrue(response.Status.IsSuccess);
    }

    [Test]
    public async Task WhenAPlayerGetsAFriendRequestTheyCanSeeIt()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequests.Status.IsSuccess);
        Assert.AreEqual(1, friendRequests.FriendRequests.Count);
        Assert.IsTrue(friendRequests.FriendRequests.Any(request => request.Id == userOne.User.Id));
    }
    
    [Test]
    public async Task AfterSendingAFriendRequestTheRequestDoesNotAppearInTheOriginatingPlayersFriendRequestList()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);

        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userOne.User.Id);
        Assert.IsTrue(friendRequests.Status.IsSuccess);
        Assert.AreEqual(0, friendRequests.FriendRequests.Count);
    }

    [Test]
    public async Task PlayerCannotSendMultipleFriendRequests()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.DUPLICATE, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCanRemoveAFriendRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var removeRequest = await TestUtils.GetClient().SocialClient.RemoveRejectFriend(userOne.User.Id);
        Assert.IsTrue(removeRequest.Status.IsSuccess);
        
        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequests.Status.IsSuccess);
        Assert.AreEqual(0, friendRequests.FriendRequests.Count);
    }

    [Test]
    public async Task PlayerCanAcceptAFriendRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var addFriendRequest = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        Assert.IsTrue(addFriendRequest.Status.IsSuccess);
        
        var friendRequests = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequests.Status.IsSuccess);
        Assert.AreEqual(0, friendRequests.FriendRequests.Count);
    }

    [Test]
    public async Task AcceptingPlayerCanViewFriendAfterAcceptingRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var addFriendRequest = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        Assert.IsTrue(addFriendRequest.Status.IsSuccess);
        
        var friends = await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        Assert.IsTrue(friends.Status.IsSuccess);
        Assert.AreEqual(1, friends.Friends.Count);
        Assert.IsTrue(friends.Friends.Any(it => it.Id == userOne.User.Id));
    }

    [Test]
    public async Task OriginalPlayerCanViewFriendAfterOtherPlayerAcceptsRequest()
    {
        var response = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        var addFriendRequest = await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        Assert.IsTrue(addFriendRequest.Status.IsSuccess);

        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        
        var friends = await TestUtils.GetClient().SocialClient.GetFriendList(userOne.User.Id);
        Assert.IsTrue(friends.Status.IsSuccess);
        Assert.AreEqual(1, friends.Friends.Count);
        Assert.IsTrue(friends.Friends.Any(it => it.Id == userTwo.User.Id));
    }
    
    [Test]
    public async Task NonAdminsCannotViewAnotherPlayersFriendList()
    {
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.GetFriendList(userTwo.User.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
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
        Assert.IsTrue(friendList.Status.IsSuccess);
        Assert.AreEqual(1, friendList.Friends.Count);
        Assert.IsTrue(friendList.Friends.Any(it => it.Id == userOne.User.Id));
    }

    [Test]
    public async Task PlayerCannotSendAFriendRequestToNonExistingPlayer()
    {
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest("InvalidUserId");
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCannotGetAFriendRequestFromABlockedPlayer()
    {
        // Block user two from player one.
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.Status.IsSuccess);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        // Try to add friend from player two to player one.
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userOne.User.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCannotSendAFriendRequestToABlockedPlayer()
    {
        // Block user two from player one.
        var blockResponse = await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        Assert.IsTrue(blockResponse.Status.IsSuccess);
        
        // Try to add friend from player one to player two.
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task BlockingAPlayerWithAnIncomingFriendRequestRemovesTheFriendRequests()
    {
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var friendRequestsBeforeBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsBeforeBlock.Status.IsSuccess);
        Assert.AreEqual(1, friendRequestsBeforeBlock.FriendRequests.Count);
        Assert.IsTrue(friendRequestsBeforeBlock.FriendRequests.Any(request => request.Id == userOne.User.Id));

        await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userOne.User.Id);
        
        var friendRequestsAfterBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsAfterBlock.Status.IsSuccess);
        Assert.AreEqual(0, friendRequestsAfterBlock.FriendRequests.Count);
    }

    [Test]
    public async Task BlockingAPlayerAfterSendingThemAFriendRequestRemovesTheFriendRequest()
    {
        await TestUtils.GetClient().SocialClient.AddAcceptFriendRequest(userTwo.User.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var friendRequestsBeforeBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsBeforeBlock.Status.IsSuccess);
        Assert.AreEqual(1, friendRequestsBeforeBlock.FriendRequests.Count);
        Assert.IsTrue(friendRequestsBeforeBlock.FriendRequests.Any(request => request.Id == userOne.User.Id));

        TestUtils.GetClient().UserApi.SetToken(userOne.Token);

        await TestUtils.GetClient().SocialClient.BlockPlayer(new BlockPlayerRequest(), userTwo.User.Id);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        var friendRequestsAfterBlock = await TestUtils.GetClient().SocialClient.ViewFriendRequests(userTwo.User.Id);
        Assert.IsTrue(friendRequestsAfterBlock.Status.IsSuccess);
        Assert.AreEqual(0, friendRequestsAfterBlock.FriendRequests.Count);
    }
}