using System.Net;
using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.health;

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