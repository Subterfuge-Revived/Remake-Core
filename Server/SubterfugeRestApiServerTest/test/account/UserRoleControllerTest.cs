using System.Net;
using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.account;

public class UserRoleControllerTest
{
    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public async Task NonAdministratorCanGetTheirOwnRoles()
    {
        var registerResponse = await AccountUtils.AssertRegisterAccountAndAuthorized("user");
        var response = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        Assert.AreEqual(response.Status.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, response.Claims);
    }
    
    [Test]
    public async Task AdministratorsCanGetRolesForAnyone()
    {
        var registerResponse = await AccountUtils.AssertRegisterAccountAndAuthorized("user");
        
        var adminResponse = await TestUtils.CreateSuperUserAndLogin();
        var response = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(response.Status.IsSuccess);
        Assert.AreEqual(response.Status.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, response.Claims);
    }
    
    [Test]
    public async Task NonAdministratorsCannotGetRolesForAnotherPlayer()
    {
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");
        var userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("userTwo");
        
        var response = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().UserRoles.GetRoles(userOne.User.Id);
        });
        Assert.AreEqual(HttpStatusCode.Forbidden, response.rawResponse.StatusCode);
    }
    
    [Test]
    public async Task AdministratorsCanUpdateOtherUserRoles()
    {
        var registerResponse = await AccountUtils.AssertRegisterAccountAndAuthorized("user");
        
        var adminResponse = await TestUtils.CreateSuperUserAndLogin();
        var roleResponse = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(roleResponse.Status.IsSuccess);
        Assert.AreEqual(roleResponse.Status.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, roleResponse.Claims);
        Assert.IsFalse(roleResponse.Claims.Any(claim => claim == UserClaim.Administrator));

        var newRoleList = roleResponse.Claims.ToList();
        newRoleList.Add(UserClaim.Administrator);
        var updateRole = await TestUtils.GetClient().UserRoles.SetRoles(registerResponse.User.Id,
            new UpdateRolesRequest()
            {
                Claims = newRoleList.ToArray(),
            });
        Assert.IsTrue(updateRole.Status.IsSuccess);
        
        var updatedRoleResponse = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(updatedRoleResponse.Status.IsSuccess);
        Assert.AreEqual(updatedRoleResponse.Status.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, updatedRoleResponse.Claims);
        Assert.Contains(UserClaim.Administrator, updatedRoleResponse.Claims);
    }
    
    [Test]
    public async Task NonAdministratorsCannotUpdateOtherUserRoles()
    {
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");
        var userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("userTwo");

        var updateRole = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().UserRoles.SetRoles(userOne.User.Id,
                new UpdateRolesRequest()
                {
                    Claims = new[] { UserClaim.Administrator, UserClaim.User },
                });
        });
        Assert.AreEqual(HttpStatusCode.Forbidden, updateRole.rawResponse.StatusCode);
    }
}