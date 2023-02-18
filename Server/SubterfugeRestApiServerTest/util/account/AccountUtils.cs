using NUnit.Framework;
using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Server.Test.util.account;

public class AccountUtils
{
    public static async Task<AccountRegistrationResponse> AssertRegisterAccountAndAuthorized(
        string username,
        string email = "someEmail@email.com",
        string deviceId = null,
        string phone = null
    ) {
        var accountRegistrationResponse = await TestUtils.GetClient().UserApi.RegisterAccount(new AccountRegistrationRequest()
        {
            DeviceIdentifier = deviceId ?? Guid.NewGuid().ToString(),
            Password = username,
            PhoneNumber = phone ?? Guid.NewGuid().ToString(),
            Username = username
        });
        Assert.True(accountRegistrationResponse.IsSuccess());
        Assert.IsNotNull(accountRegistrationResponse.GetOrThrow().Token);
        Assert.IsNotNull(accountRegistrationResponse.GetOrThrow().User);
        Assert.AreEqual(accountRegistrationResponse.GetOrThrow().User.Username, username);

        await AssertUserAuthorized();
        return accountRegistrationResponse.GetOrThrow();
    }

    public static async Task<AuthorizationResponse> AssertLogin(string username)
    {
        var loginResponse = await TestUtils.GetClient().UserApi.Login(new AuthorizationRequest()
        {
            Password = username,
            Username = username
        });
        Assert.True(loginResponse.IsSuccess());
        Assert.IsNotNull(loginResponse.GetOrThrow().Token);
        Assert.IsNotNull(loginResponse.GetOrThrow().User);
        Assert.AreEqual(loginResponse.GetOrThrow().User.Username, username);
        
        await AccountUtils.AssertUserAuthorized();
        return loginResponse.GetOrThrow();
    }

    public static async Task AssertUserAuthorized()
    {
        var pingResponse = await TestUtils.GetClient().HealthClient.AuthorizedPing();
        Assert.True(pingResponse.IsSuccess());
    }
}