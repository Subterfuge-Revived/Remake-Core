using System.Net;
using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.account;

public class UserControllerTest
{
    private SubterfugeClient client = TestUtils.GetClient();

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public async Task CanLoginAsAdmin()
    {
        var account = await TestUtils.Mongo.CreateTestingSuperUser();
        var response = await client.UserApi.Login(new AuthorizationRequest() { Username = "admin", Password = "admin" });
        Assert.True(response.Status.IsSuccess);
        Assert.IsNotNull(response.Token);
        Assert.IsNotNull(response.User);
    }

    [Test]
    public async Task UserCanRegisterNewAccount()
    {
        var response = await client.UserApi.RegisterAccount(new AccountRegistrationRequest()
        {
            DeviceIdentifier = "MyDevice",
            Email = "someEmail@email.com",
            Password = "test",
            PhoneNumber = "1231231231",
            Username = "test"
        });
        Assert.True(response.Status.IsSuccess);
        Assert.IsNotNull(response.Token);
        Assert.IsNotNull(response.User);
    }
    
    [Test]
    public async Task UserIsLoggedInAfterRegistering()
    {
        await AccountUtils.AssertRegisterAccountAndAuthorized("SomeUsername");
    }

    [Test]
    public async Task PlayerCanLoginAfterRegisteringNewAccount()
    {
        string username = "OtherUsername";
        await AccountUtils.AssertRegisterAccountAndAuthorized(username);
        TestUtils.GetClient().UserApi.Logout();
        await AccountUtils.AssertLogin(username);
        await AccountUtils.AssertUserAuthorized();
    }

    [Test]
    public async Task PlayerCannotRegisterWithTheSameUsername()
    {
        string username = "OtherUsername";
        await AccountUtils.AssertRegisterAccountAndAuthorized(username);
        var exception = Assert.ThrowsAsync<SubterfugeClientException>( async () => {
            await AccountUtils.AssertRegisterAccountAndAuthorized(username);
        });
        Assert.AreEqual(exception.response.ResponseType, ResponseType.DUPLICATE);
    }

    [Test]
    public async Task SuperUserAccountHasAdminClaims()
    {
        var account = await TestUtils.Mongo.CreateTestingSuperUser();
        var loginResponse = await TestUtils.GetClient().UserApi.Login(new AuthorizationRequest() { Username = "admin", Password = "admin" });
        var response = await TestUtils.GetClient().UserRoles.GetRoles(loginResponse.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        Assert.AreEqual(response.Status.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, response.Claims);
        Assert.Contains(UserClaim.Administrator, response.Claims);
    }
    
    [Test]
    public async Task NewAccountRegistrationsHaveNormalUserClaims()
    {
        var registerResponse = await AccountUtils.AssertRegisterAccountAndAuthorized("user");
        var response = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        Assert.AreEqual(response.Status.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, response.Claims);
    }
    
    [Test]
    public async Task AdministratorsCanListAllUsers()
    {
        // Make some dummy accounts
        await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");
        await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        
        var loginResponse = await TestUtils.CreateSuperUserAndLogin();
        var response = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest());
        Assert.AreEqual(3, response.Users.Length);
    }
    
    [Test]
    public async Task NonAdministratorsCannotListAllUsers()
    {
        // Make some dummy accounts
        await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");
        await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        await AccountUtils.AssertRegisterAccountAndAuthorized("UserThree");

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest());
        });
        Assert.AreEqual(HttpStatusCode.Forbidden, exception.rawResponse.StatusCode);
    }
}