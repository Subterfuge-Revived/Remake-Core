using NUnit.Framework;
using NUnit.Framework.Internal;
using SubterfugeCore.Models.GameEvents;
using SubterfugeDatabaseProvider.Models;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.specialists;

public class SpecialistConfigurationControllerTest
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
    public async Task PlayerCanCreateACustomSpecialist()
    {
        var specialist = createSpecialistRequest("New Specialist");
        
        SubmitCustomSpecialistResponse response = await TestUtils.GetClient().SpecialistClient.SubmitCustomSpecialist(specialist);
        Assert.IsTrue(response.Status.IsSuccess);
        Assert.NotNull(response.SpecialistConfigurationId);
    }

    [Test]
    public async Task PlayerCanViewCustomSpecialistAfterCreating()
    {
        String specName = "MyCustomSpecialist";
        SubmitCustomSpecialistResponse response = await submitCustomSpecialist(specName);
            
        string specUuid = response.SpecialistConfigurationId;
            
        GetCustomSpecialistsResponse specResponse = await TestUtils.GetClient().SpecialistClient.GetCustomSpecialist(specUuid);
        Assert.IsTrue(specResponse.CustomSpecialists.Count == 1);
        Assert.AreEqual(specUuid, specResponse.CustomSpecialists[0].Id);
        Assert.AreEqual(specName, specResponse.CustomSpecialists[0].SpecialistName);
        Assert.AreEqual(1, specResponse.CustomSpecialists[0].Priority);
        Assert.AreEqual(EffectModifier.Driller, specResponse.CustomSpecialists[0].SpecialistEffects[0].EffectModifier);
    }

    [Test]
    public async Task CanGetAllSpecialistsCreatedByAPlayer()
    {
        String specName = "MyCustomSpecialist";
        await submitCustomSpecialist(specName);
            
        String secondSpecName = "MyCustomSpecialist2";
        await submitCustomSpecialist(secondSpecName);
            
            
        GetCustomSpecialistsResponse specialistList = await TestUtils.GetClient().SpecialistClient.GetCustomSpecialists(new GetCustomSpecialistsRequest()
        {
            CreatedByPlayerId = _userOne.User.Id
        });
        Assert.AreEqual(2, specialistList.CustomSpecialists.Count);
        Assert.AreEqual(1, specialistList.CustomSpecialists.Count(it => it.SpecialistName == specName));
        Assert.AreEqual(1, specialistList.CustomSpecialists.Count(it => it.SpecialistName == secondSpecName));
    }
    
    [Test]
    public async Task CanGetAllSpecialistsByNameCaseInsensitive()
    {
        String specName = "MyCustomSpecialist";
        await submitCustomSpecialist(specName);
            
        String secondSpecName = "MyCustomSpecialist2";
        await submitCustomSpecialist(secondSpecName);

        var search = "custom";
        GetCustomSpecialistsResponse specialistList = await TestUtils.GetClient().SpecialistClient.GetCustomSpecialists(new GetCustomSpecialistsRequest()
        {
            SearchTerm = search
        });
        Assert.AreEqual(2, specialistList.CustomSpecialists.Count);
        Assert.AreEqual(2, specialistList.CustomSpecialists.Count(it => it.SpecialistName.ToLower().Contains(search)));
    }

    [Test]
    public async Task CanViewSpecialistsCreatedByAnyPlayer()
    {
        String userOneSpecName = "UserOneSpecialists";
        await submitCustomSpecialist(userOneSpecName);

        TestUtils.GetClient().UserApi.SetToken(_userTwo.Token);

        String userTwoSpecialist = "MyCustomSpecialist";
        await submitCustomSpecialist(userTwoSpecialist);

        GetCustomSpecialistsResponse specResponse = await TestUtils.GetClient().SpecialistClient.GetCustomSpecialists(new GetCustomSpecialistsRequest());
        Assert.AreEqual(2, specResponse.CustomSpecialists.Count);
        Assert.AreEqual(1, specResponse.CustomSpecialists.Count(it => it.SpecialistName == userOneSpecName));
        Assert.AreEqual(1, specResponse.CustomSpecialists.Count(it => it.SpecialistName == userTwoSpecialist));
        Assert.AreEqual(1, specResponse.CustomSpecialists.Count(it => it.Creator.Id == _userOne.User.Id));
        Assert.AreEqual(1, specResponse.CustomSpecialists.Count(it => it.Creator.Id == _userTwo.User.Id));
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