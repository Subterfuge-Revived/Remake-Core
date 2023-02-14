using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
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
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.GetOrThrow().SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse.IsSuccess());
        Assert.NotNull(packageResponse.GetOrThrow().SpecialistPackageId);
    }

    [Test]
    public async Task CanViewAPlayersSpecialistPackages()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.GetOrThrow().SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse.IsSuccess());
        Assert.NotNull(packageResponse.GetOrThrow().SpecialistPackageId);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(new GetSpecialistPackagesRequest()
        {
            CreatedByUserId = _userOne.User.Id,
        });
            
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.GetOrThrow().SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialist.GetOrThrow().SpecialistConfigurationId, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds[0]);
    }
    
    [Test]
    public async Task CanSearchSpecialistPackagesCaseInsensitive()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.GetOrThrow().SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse.IsSuccess());
        Assert.NotNull(packageResponse.GetOrThrow().SpecialistPackageId);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(new GetSpecialistPackagesRequest()
        {
            SearchTerm = "package",
        });
            
        Assert.IsTrue(playerPackages.IsSuccess());
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages.Count);
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages.Count(it => it.PackageName.ToLower().Contains("package")));
    }

    [Test]
    public async Task CanAddMultipleSpecialistsToAPackage()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var specialistTwo = await submitCustomSpecialist("MySpecialistTwo");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.GetOrThrow().SpecialistConfigurationId, specialistTwo.GetOrThrow().SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse.IsSuccess());
        Assert.NotNull(packageResponse.GetOrThrow().SpecialistPackageId);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(new GetSpecialistPackagesRequest()
        {
            CreatedByUserId = _userOne.User.Id,
        });
            
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.GetOrThrow().SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(2, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialist.GetOrThrow().SpecialistConfigurationId, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds[0]);
        Assert.AreEqual(specialistTwo.GetOrThrow().SpecialistConfigurationId, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds[1]);
    }

    [Test]
    public async Task CanAddASpecialistPackageToAPackage()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageOneResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.GetOrThrow().SpecialistConfigurationId });
        Assert.IsTrue(packageOneResponse.IsSuccess());
        Assert.NotNull(packageOneResponse.GetOrThrow().SpecialistPackageId);
        
        var specialistTwo = await submitCustomSpecialist("MySpecialistTwo");
        var packageTwoResponse = await submitSpecialistPackage(
            "MyPackageTwo", 
            new List<string>() { specialistTwo.GetOrThrow().SpecialistConfigurationId }, 
            new List<string>(){ packageOneResponse.GetOrThrow().SpecialistPackageId }
        );
        Assert.IsTrue(packageTwoResponse.IsSuccess());
        Assert.NotNull(packageTwoResponse.GetOrThrow().SpecialistPackageId);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(packageTwoResponse.GetOrThrow().SpecialistPackageId);
            
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.GetOrThrow().SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialistTwo.GetOrThrow().SpecialistConfigurationId, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds[0]);
        Assert.AreEqual(packageOneResponse.GetOrThrow().SpecialistPackageId, playerPackages.GetOrThrow().SpecialistPackages[0].PackageIds[0]);
    }

    [Test]
    public async Task CannotReferenceAnInvalidSpecialistPackageId()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var exception = await submitSpecialistPackage(
                "MyPackage",
                new List<string>() { specialist.GetOrThrow().SpecialistConfigurationId },
                new List<string>() { "Non-existent-package" }
            );
        Assert.AreEqual(false, exception.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task CannotReferenceAnInvalidSpecialistId()
    {
        var exception =  await submitSpecialistPackage(
                "MyPackage",
                new List<string>() { "Non-existent-Specialist" }
            );
        Assert.AreEqual(false, exception.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task CanViewOtherPlayersPackages()
    {
        var specialist = await submitCustomSpecialist("MySpecialist");
        var packageResponse = await submitSpecialistPackage("MyPackage", new List<string>() { specialist.GetOrThrow().SpecialistConfigurationId });
            
        Assert.IsTrue(packageResponse.IsSuccess());
        Assert.NotNull(packageResponse.GetOrThrow().SpecialistPackageId);
        
        TestUtils.GetClient().UserApi.SetToken(_userTwo.Token);
        
        var playerPackages = await TestUtils.GetClient().SpecialistClient.GetSpecialistPackages(new GetSpecialistPackagesRequest()
        {
            CreatedByUserId = _userOne.User.Id,
        });
            
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages.Count);
        Assert.AreEqual(_userOne.User.Id, playerPackages.GetOrThrow().SpecialistPackages[0].Creator.Id);
        Assert.AreEqual(1, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds.Count);
        Assert.AreEqual(specialist.GetOrThrow().SpecialistConfigurationId, playerPackages.GetOrThrow().SpecialistPackages[0].SpecialistIds[0]);
    }
    
    private async Task<SubterfugeResponse<CreateSpecialistPackageResponse>> submitSpecialistPackage(
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
    
    private async Task<SubterfugeResponse<SubmitCustomSpecialistResponse>> submitCustomSpecialist(String specialistName)
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