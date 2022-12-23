using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.admin;

public class AdminControllerTest
{

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }
    
    [Test]
    public async Task AdministratorCanBanAPlayer(){}
    
    [Test]
    public async Task AdministratorCanViewAPlayersBanHistory(){}
    
    [Test]
    public async Task AdministratorCanBanAnIp(){}
    
    [Test]
    public async Task AdministratorCanBanAnIpByRegex(){}
    
    [Test]
    public async Task AdministratorCanViewTheServerExceptionLog(){}
    
    [Test]
    public async Task AdministratorCanViewAListOfPlayerActions(){}
}