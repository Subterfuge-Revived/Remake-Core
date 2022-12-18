using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.specialists;

public class SpecialistConfigurationControllerTest
{
    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerCanCreateACustomSpecialist()
    {
    }

    [Test]
    public void PlayerCanViewCustomSpecialistAfterCreating()
    {
    }

    [Test]
    public void CanGetAllSpecialistsCreatedByAPlayer()
    {
    }

    [Test]
    public void CanViewSpecialistsCreatedByAnyPlayer()
    {
    }
}