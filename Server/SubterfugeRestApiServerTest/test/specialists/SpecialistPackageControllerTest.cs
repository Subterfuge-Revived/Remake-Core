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
        throw new NotImplementedException();
    }

    [Test]
    public void CanViewAPlayersSpecialistPackages()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void CanAddMultipleSpecialistsToAPackage()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void CanAddASpecialistPackageToAPackage()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void CanViewOtherPlayersPackages()
    {
        throw new NotImplementedException();
    }

}