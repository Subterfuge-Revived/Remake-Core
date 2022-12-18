using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest;

public class AccountUtils
{
    public static async Task<AccountRegistrationResponse> AssertRegisterAccountAndAuthorized(string username)
    {
        var accountRegistrationResponse = await TestUtils.GetClient().UserApi.RegisterAccount(new AccountRegistrationRequest()
        {
            DeviceIdentifier = "MyDevice",
            Email = "someEmail@email.com",
            Password = username,
            PhoneNumber = "1231231231",
            Username = username
        });
        Assert.True(accountRegistrationResponse.Status.IsSuccess);
        Assert.IsNotNull(accountRegistrationResponse.Token);
        Assert.IsNotNull(accountRegistrationResponse.User);
        Assert.AreEqual(accountRegistrationResponse.User.Username, username);

        await AssertUserAuthorized();
        return accountRegistrationResponse;
    }

    public static async Task<AuthorizationResponse> AssertLogin(string username)
    {
        var loginResponse = await TestUtils.GetClient().UserApi.Login(new AuthorizationRequest()
        {
            Password = "OtherUsername",
            Username = "OtherUsername"
        });
        Assert.True(loginResponse.Status.IsSuccess);
        Assert.IsNotNull(loginResponse.Token);
        Assert.IsNotNull(loginResponse.User);
        Assert.AreEqual(loginResponse.User.Username, username);
        
        await AccountUtils.AssertUserAuthorized();
        return loginResponse;
    }

    public static async Task AssertUserAuthorized()
    {
        var pingResponse = await TestUtils.GetClient().HealthClient.AuthorizedPing();
        Assert.True(pingResponse.Status.IsSuccess);
    }
}