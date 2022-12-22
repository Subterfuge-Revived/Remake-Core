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
        var response = await TestUtils.GetClient().UserApi.GetUsers();
        Assert.AreEqual(3, response.Users.Count);
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
            await TestUtils.GetClient().UserApi.GetUsers();
        });
        Assert.AreEqual(HttpStatusCode.Forbidden, exception.rawResponse.StatusCode);
    }
    
    [Test]
    public async Task QueryParamsWorkOnGetUsersEndpoint()
    {
        // Make some dummy accounts with different data to filter on
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized(
            username: "UserOne",
            email: "MyFakeEmail@someEmail.com",
            deviceId: "FakeDeviceId",
            phone: "9999999999"
        );
        
        
        var userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized(
            username: "UserTwo",
            email: "RealEmail@realEmail.com",
            deviceId: "RealDeviceId",
            phone: "0000000000"
        );
        
        
        var userThree = await AccountUtils.AssertRegisterAccountAndAuthorized(
            username: "UserThree",
            email: "RealEmailAsWell@realEmail.com",
            deviceId: "DifferentDeviceId",
            phone: "1231231231"
        );
        
        
        await TestUtils.CreateSuperUserAndLogin();
        
        // Can search by username
        var usernameResponse = await TestUtils.GetClient().UserApi.GetUsers(username: "UserOne");
        Assert.AreEqual(1, usernameResponse.Users.Count);
        Assert.IsTrue(usernameResponse.Users.All(user => user.Username.Contains("UserOne")));
        
        // Can search by email
        /*
        var emailResponse = await TestUtils.GetClient().UserApi.GetUsers(email: "RealEmail");
        Assert.AreEqual(2, emailResponse.Users.Count);
        Assert.IsTrue(emailResponse.Users.All(user => user.Email.Contains("RealEmail")));
        
        // Can search by email AND username
        var emailAndUsernameResponse = await TestUtils.GetClient().UserApi.GetUsers(email: "RealEmail", username: "UserTwo");
        Assert.AreEqual(1, emailAndUsernameResponse.Users.Count);
        Assert.IsTrue(emailAndUsernameResponse.Users.All(user => user.Email.Contains("RealEmail") && user.Username.Contains("UserTwo")));
        
        // Can search by deviceId
        var deviceIdResponse = await TestUtils.GetClient().UserApi.GetUsers(deviceIdentifier: "FakeDeviceId");
        Assert.AreEqual(1, deviceIdResponse.Users.Length);
        Assert.IsTrue(deviceIdResponse.Users.All(user => user.DeviceIdentifier.Contains("FakeDeviceId")));
        
        // Can search by phone
        var phoneResponse = await TestUtils.GetClient().UserApi.GetUsers(phone: "1231231231");
        Assert.AreEqual(1, phoneResponse.Users.Length);
        Assert.IsTrue(phoneResponse.Users.All(user => user.PhoneNumber == "1231231231"));
        
        // Can search by claims
        var claimsResponse = await TestUtils.GetClient().UserApi.GetUsers(claim: UserClaim.User);
        
        // There is 4! The super user is also considered a normal user!
        Assert.AreEqual(4, claimsResponse.Users.Length);
        Assert.IsTrue(claimsResponse.Users.All(user => user.Claims.Contains(UserClaim.User)));
        
        // Can search by userId
        var userIdResponse = await TestUtils.GetClient().UserApi.GetUsers(userId: userOne.User.Id);
        Assert.AreEqual(1, userIdResponse.Users.Length);
        Assert.IsTrue(userIdResponse.Users.All(user => user.Id == userOne.User.Id));
        */
    }
}