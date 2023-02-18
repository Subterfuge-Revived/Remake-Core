using NUnit.Framework;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Test.util;
using Subterfuge.Remake.Server.Test.util.account;

namespace Subterfuge.Remake.Server.Test.test.account;

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
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        Assert.AreEqual(response.ResponseDetail.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, response.GetOrThrow().Claims);
    }
    
    [Test]
    public async Task AdministratorsCanGetRolesForAnyone()
    {
        var registerResponse = await AccountUtils.AssertRegisterAccountAndAuthorized("user");
        
        var adminResponse = await TestUtils.CreateSuperUserAndLogin();
        var response = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(response.ResponseDetail.IsSuccess);
        Assert.AreEqual(response.ResponseDetail.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, response.GetOrThrow().Claims);
    }
    
    [Test]
    public async Task NonAdministratorsCannotGetRolesForAnotherPlayer()
    {
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");
        var userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("userTwo");
        
        var response =  await TestUtils.GetClient().UserRoles.GetRoles(userOne.User.Id);
        Assert.IsFalse(response.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, response.ResponseDetail.ResponseType);
    }
    
    [Test]
    public async Task AdministratorsCanUpdateOtherUserRoles()
    {
        var registerResponse = await AccountUtils.AssertRegisterAccountAndAuthorized("user");
        
        var adminResponse = await TestUtils.CreateSuperUserAndLogin();
        var roleResponse = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(roleResponse.ResponseDetail.IsSuccess);
        Assert.AreEqual(roleResponse.ResponseDetail.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, roleResponse.GetOrThrow().Claims);
        Assert.IsFalse(roleResponse.GetOrThrow().Claims.Any(claim => claim == UserClaim.Administrator));

        var newRoleList = roleResponse.GetOrThrow().Claims.ToList();
        newRoleList.Add(UserClaim.Administrator);
        var updateRole = await TestUtils.GetClient().UserRoles.SetRoles(registerResponse.User.Id,
            new UpdateRolesRequest()
            {
                Claims = newRoleList.ToArray(),
            });
        Assert.IsTrue(updateRole.ResponseDetail.IsSuccess);
        
        var updatedRoleResponse = await TestUtils.GetClient().UserRoles.GetRoles(registerResponse.User.Id);
        Assert.IsTrue(updatedRoleResponse.ResponseDetail.IsSuccess);
        Assert.AreEqual(updatedRoleResponse.ResponseDetail.ResponseType, ResponseType.SUCCESS);
        Assert.Contains(UserClaim.User, updatedRoleResponse.GetOrThrow().Claims);
        Assert.Contains(UserClaim.Administrator, updatedRoleResponse.GetOrThrow().Claims);
    }
    
    [Test]
    public async Task NonAdministratorsCannotUpdateOtherUserRoles()
    {
        var userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("userOne");
        var userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("userTwo");

        var updateRole = await TestUtils.GetClient().UserRoles.SetRoles(userOne.User.Id,
                new UpdateRolesRequest()
                {
                    Claims = new[] { UserClaim.Administrator, UserClaim.User },
                });
        Assert.IsFalse(updateRole.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, updateRole.ResponseDetail.ResponseType);
    }
}