using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class GameEventControllerTest
{
    private SubterfugeClient client = TestUtils.GetClient();

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerInAGameCanSubmitAnEvent()
    {
    }

    [Test]
    public void PlayerCannotSubmitEventsToAGameThatDoesNotExist()
    {
    }

    [Test]
    public void PlayerCannotSubmitEventsToAGameTheyAreNotIn()
    {
    }

    [Test]
    public void PlayerCannotSubmitAnEventThatOccursInThePast()
    {
    }

    [Test]
    public void PlayerCanDeleteAnEventThatTheySubmitted()
    {
    }

    [Test]
    public void PlayerCannotDeleteAnotherPlayersEvent()
    {
    }

    [Test]
    public void PlayerCannotDeleteEventsThatHaveAlreadyHappened()
    {
    }

    [Test]
    public void PlayerCanUpdateAGameEvent()
    {
    }

    [Test]
    public void PlayerCannotUpdateAGameEventWithInvalidEventId()
    {
    }

    [Test]
    public void PlayerCannotUpdateAGameEventThatHasAlreadyOccurred()
    {
    }

    [Test]
    public void PlayerCannotUpdateAnotherPlayersEvent()
    {
    }

    [Test]
    public void PlayersCanViewAnyEventThatHasAlreadyOccurred()
    {
    }

    [Test]
    public void PlayerCanViewTheirOwnEventsThatOccurInTheFutureButOthersCannot()
    {
    }

    [Test]
    public void AdminsCanSeeAllGameEvents()
    {
    }
}