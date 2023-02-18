using NUnit.Framework;
using Subterfuge.Remake.Api.Client;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Test.util;
using Subterfuge.Remake.Server.Test.util.account;

namespace Subterfuge.Remake.Server.Test.test.health;

public class HealthControllerTest
{
    private SubterfugeClient client = TestUtils.GetClient();

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public async Task AnyoneCanPing()
    {
        var response = await client.HealthClient.Ping();
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
    }

    [Test]
    public async Task UnauthenticatedUsersCannotUseAuthorizedPing()
    {
        var response = await client.HealthClient.AuthorizedPing();
        Assert.IsFalse(response.IsSuccess());
        Assert.AreEqual(ResponseType.UNAUTHORIZED, response.ResponseDetail.ResponseType);
    }
    
    [Test]
    public async Task AuthorizedUserCanUseAuthorziedPing()
    {
        var user = await AccountUtils.AssertRegisterAccountAndAuthorized("NewUser");
        var response = await client.HealthClient.AuthorizedPing();
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
    }
    
    [Test]
    public async Task AuthorizedPingReturnsTheLoggedInUserInfo()
    {
        var user = await AccountUtils.AssertRegisterAccountAndAuthorized("NewUser");
        var response = await client.HealthClient.AuthorizedPing();
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        Assert.AreEqual(response.GetOrThrow().LoggedInUser.Id, user.User.Id);
    }

}