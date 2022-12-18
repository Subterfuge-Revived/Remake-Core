using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.specialists;

public class SpecialistPackageControllerTest
{
    private SubterfugeClient client = TestUtils.GetClient();

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void CanCreateASpecialistPackage()
    {
    }

    [Test]
    public void CanViewAPlayersSpecialistPackages()
    {
    }

    [Test]
    public void CanAddMultipleSpecialistsToAPackage()
    {
    }

    [Test]
    public void CanAddASpecialistPackageToAPackage()
    {
    }

    [Test]
    public void CanViewOtherPlayersPackages()
    {
    }

}