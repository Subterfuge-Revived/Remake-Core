using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.specialists;

public class SpecialistPackageControllerTest
{
    private AccountRegistrationResponse _userOne;
    private AccountRegistrationResponse _userTwo;

    [SetUp]
    public async Task Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();

        _userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        _userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");
    }

    [Test]
    public async Task CanCreateASpecialistPackage()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse .Status.IsSuccess);
        Assert.NotNull(packageResponse.SpecialistPackageId);
    }

    [Test]
    public async Task CanViewAPlayersSpecialistPackages()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse .Status.IsSuccess);
        Assert.NotNull(packageResponse.SpecialistPackageId);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(new GetSpecialistPackagesRequest()
        {
            CreatedByUserId = _userOne.User.Id,
        });
            
        Assert.AreEqual(1, playerPackages.SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(1, playerPackages.SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialist.SpecialistConfigurationId, playerPackages.SpecialistPackages[0].SpecialistIds[0]);
    }

    [Test]
    public async Task CanAddMultipleSpecialistsToAPackage()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var specialistTwo = await submitCustomSpecialist("MySpecialistTwo");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.SpecialistConfigurationId, specialistTwo.SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse .Status.IsSuccess);
        Assert.NotNull(packageResponse.SpecialistPackageId);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(new GetSpecialistPackagesRequest()
        {
            CreatedByUserId = _userOne.User.Id,
        });
            
        Assert.AreEqual(1, playerPackages.SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(2, playerPackages.SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialist.SpecialistConfigurationId, playerPackages.SpecialistPackages[0].SpecialistIds[0]);
        Assert.AreEqual(specialistTwo.SpecialistConfigurationId, playerPackages.SpecialistPackages[0].SpecialistIds[1]);
    }

    [Test]
    public async Task CanAddASpecialistPackageToAPackage()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageOneResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.SpecialistConfigurationId });
        Assert.IsTrue(packageOneResponse .Status.IsSuccess);
        Assert.NotNull(packageOneResponse.SpecialistPackageId);
        
        var specialistTwo = await submitCustomSpecialist("MySpecialistTwo");
        var packageTwoResponse = await submitSpecialistPackage(
            "MyPackageTwo", 
            new List<string>() { specialistTwo.SpecialistConfigurationId }, 
            new List<string>(){ packageOneResponse.SpecialistPackageId }
        );
        Assert.IsTrue(packageTwoResponse .Status.IsSuccess);
        Assert.NotNull(packageTwoResponse.SpecialistPackageId);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(packageTwoResponse.SpecialistPackageId);
            
        Assert.AreEqual(1, playerPackages.SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(1, playerPackages.SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialistTwo.SpecialistConfigurationId, playerPackages.SpecialistPackages[0].SpecialistIds[0]);
        Assert.AreEqual(packageOneResponse.SpecialistPackageId, playerPackages.SpecialistPackages[0].PackageIds[0]);
    }

    [Test]
    public async Task CannotReferenceAnInvalidSpecialistPackageId()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await submitSpecialistPackage(
                "MyPackage",
                new List<string>() { specialist.SpecialistConfigurationId },
                new List<string>() { "Non-existent-package" }
            );
        });

        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
        Assert.AreEqual(false, exception.response.Status.IsSuccess);
    }

    [Test]
    public async Task CannotReferenceAnInvalidSpecialistId()
    {
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await submitSpecialistPackage(
                "MyPackage",
                new List<string>() { "Non-existent-Specialist" }
            );
        });
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
        Assert.AreEqual(false, exception.response.Status.IsSuccess);
    }

    [Test]
    public async Task CanViewOtherPlayersPackages()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse .Status.IsSuccess);
        Assert.NotNull(packageResponse.SpecialistPackageId);
        
        TestUtils.GetClient().UserApi.SetToken(_userTwo.Token);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(new GetSpecialistPackagesRequest()
        {
            CreatedByUserId = _userOne.User.Id,
        });
            
        Assert.AreEqual(1, playerPackages.SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(1, playerPackages.SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialist.SpecialistConfigurationId, playerPackages.SpecialistPackages[0].SpecialistIds[0]);
    }
    
    private async Task<CreateSpecialistPackageResponse> submitSpecialistPackage(
        string packageName,
        List<string> specialistIds,
        List<string> packageIds = null
    ) {
        return await TestUtils.GetClient().SpecialistClient.CreateSpecialistPackage(await createSpecialistPackageRequest(packageName, specialistIds, packageIds));
    }

    private async Task<CreateSpecialistPackageRequest> createSpecialistPackageRequest(string packageName, List<string> specialistIds, List<string> packageIds = null)
    {
        return new CreateSpecialistPackageRequest()
        {
            PackageIds = packageIds ?? new List<string>(),
            SpecialistIds = specialistIds,
            PackageName = packageName,
        };
    }
    
    private async Task<SubmitCustomSpecialistResponse> submitCustomSpecialist(String specialistName)
    {
        return await TestUtils.GetClient().SpecialistClient.SubmitCustomSpecialist(createSpecialistRequest(specialistName));
    }
    
    private SubmitCustomSpecialistRequest createSpecialistRequest(String specialistName) {
        return new SubmitCustomSpecialistRequest()
        {
            
            Priority = 1,
            SpecialistName = specialistName,
            SpecialistEffects = new List<SpecialistEffectConfiguration>()
            {
                new SpecialistEffectConfiguration()
                {
                    EffectModifier = EffectModifier.Driller,
                    EffectScale = new SpecialistEffectScale()
                    {
                        EffectScale = EffectScale.ConstantValue,
                        EffectScaleTarget = EffectTarget.NoTarget,
                        EffectTriggerRange = EffectTriggerRange.Self
                    },
                    EffectTarget = EffectTarget.Friendly,
                    EffectTrigger = EffectTrigger.SubCombat,
                    EffectTriggerRange = EffectTriggerRange.Self,
                    Value = 15,
                }
            },
            PromotesFromSpecialistId = "",
        };
    }

}