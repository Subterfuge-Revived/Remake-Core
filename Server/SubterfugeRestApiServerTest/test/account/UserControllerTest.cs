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
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public async Task CanLoginAsAdmin()
    {
        var account = await TestUtils.Mongo.CreateSuperUser();
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
        Assert.AreEqual(ResponseType.DUPLICATE, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task SuperUserAccountHasAdminClaims()
    {
        var account = await TestUtils.Mongo.CreateSuperUser();
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
        Assert.AreEqual(3, response.users.Count);
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
    
    [Test]
    public async Task AdminsCanGetUsersByUsername()
    {
        await SeedUsersInDatabase();

        // Can search by username
        var usernameResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest() { UsernameSearch = "UserOne"});
        Assert.AreEqual(1, usernameResponse.users.Count);
        Assert.IsTrue(usernameResponse.users.All(user => user.Username.Contains("UserOne")));
    }

    [Test]
    public async Task AdminsCanGetUsersByUsernameCaseInsensitive()
    {
        await SeedUsersInDatabase();

        // Can search by username
        var usernameResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest() { UsernameSearch = "userone"});
        Assert.AreEqual(1, usernameResponse.users.Count);
        Assert.IsTrue(usernameResponse.users.All(user => user.Username.Contains("UserOne")));
    }
    
    [Test]
    public async Task AdminsCanGetUsersByEmail()
    {
        await SeedUsersInDatabase();

        var emailResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest() { EmailSearch = "RealEmail" });
        Assert.AreEqual(2, emailResponse.users.Count);
        Assert.IsTrue(emailResponse.users.All(user => user.Email.Contains("RealEmail")));
    }
    
    [Test]
    public async Task AdminsCanGetUsersByEmailCaseInsensitive()
    {
        await SeedUsersInDatabase();

        var emailResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest() { EmailSearch = "realemail" });
        Assert.AreEqual(2, emailResponse.users.Count);
        Assert.IsTrue(emailResponse.users.All(user => user.Email.Contains("RealEmail")));
    }
    
    [Test]
    public async Task AdminsCanGetUsersByEmailAndUsername()
    {
        await SeedUsersInDatabase();

        // Can search by email AND username
        var emailAndUsernameResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest() { EmailSearch = "realemail", UsernameSearch = "userTwo"});
        Assert.AreEqual(1, emailAndUsernameResponse.users.Count);
        Assert.IsTrue(emailAndUsernameResponse.users.All(user => user.Email.Contains("RealEmail") && user.Username.Contains("UserTwo")));
    }
    
    [Test]
    public async Task AdminsCanGetUsersByDeviceId()
    {
        await SeedUsersInDatabase();

        // Can search by deviceId
        var deviceIdResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest(){ DeviceIdentifierSearch = "FakeDeviceId"});
        Assert.AreEqual(1, deviceIdResponse.users.Count);
        // Cannot ensure the returned user has the device ID because we hash them when they get returned.
    }
    
    [Test]
    public async Task AdminsCanGetUsersHavingSpecificRoles()
    {
        await SeedUsersInDatabase();

        // Can search by Claims
        var claimsResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest(){ RequireUserClaim = UserClaim.User});
        // There is 4! The super user is also considered a normal user!
        Assert.AreEqual(4, claimsResponse.users.Count);
        Assert.IsTrue(claimsResponse.users.All(user => user.Claims.Contains(UserClaim.User)));
    }

    [Test]
    public async Task AdminsCanGetUsersByUserId()
    {
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized(
            username: "UserOne",
            email: "MyFakeEmail@someEmail.com",
            deviceId: "FakeDeviceId",
            phone: "9999999999"
        );
        
        await TestUtils.CreateSuperUserAndLogin();

        var userIdResponse = await TestUtils.GetClient().UserApi.GetUsers(new GetUserRequest(){ UserIdSearch = userOne.User.Id });
        Assert.AreEqual(1, userIdResponse.users.Count);
        Assert.IsTrue(userIdResponse.users.All(user => user.Id == userOne.User.Id));
    }

    private async Task SeedUsersInDatabase()
    {
        // Make some dummy accounts with different data to filter on
        await AccountUtils.AssertRegisterAccountAndAuthorized(
            username: "UserOne",
            email: "MyFakeEmail@someEmail.com",
            deviceId: "FakeDeviceId",
            phone: "9999999999"
        );
        
        
        await AccountUtils.AssertRegisterAccountAndAuthorized(
            username: "UserTwo",
            email: "RealEmail@realEmail.com",
            deviceId: "RealDeviceId",
            phone: "0000000000"
        );
        
        
        await AccountUtils.AssertRegisterAccountAndAuthorized(
            username: "UserThree",
            email: "RealEmailAsWell@realEmail.com",
            deviceId: "DifferentDeviceId",
            phone: "1231231231"
        );
        
        
        await TestUtils.CreateSuperUserAndLogin();
    }
}