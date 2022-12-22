using System.Net;
using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeRestApiClient.controllers.exception;
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
        Assert.IsTrue(response.Status.IsSuccess);
    }

    [Test]
    public async Task UnauthenticatedUsersCannotUseAuthorizedPing()
    {
        var response = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await client.HealthClient.AuthorizedPing();
        });
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.rawResponse.StatusCode);
    }
    
    [Test]
    public async Task AuthorizedUserCanUseAuthorziedPing()
    {
        var user = await AccountUtils.AssertRegisterAccountAndAuthorized("NewUser");
        var response = await client.HealthClient.Ping();
        Assert.IsTrue(response.Status.IsSuccess);
    }

}