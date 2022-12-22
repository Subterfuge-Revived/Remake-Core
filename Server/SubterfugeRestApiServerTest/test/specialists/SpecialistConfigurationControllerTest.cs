using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.specialists;

public class SpecialistConfigurationControllerTest
{
    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerCanCreateACustomSpecialist()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanViewCustomSpecialistAfterCreating()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void CanGetAllSpecialistsCreatedByAPlayer()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void CanViewSpecialistsCreatedByAnyPlayer()
    {
        throw new NotImplementedException();
    }
}