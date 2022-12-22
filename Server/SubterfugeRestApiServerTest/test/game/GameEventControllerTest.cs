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
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerInAGameCanSubmitAnEvent()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSubmitEventsToAGameThatDoesNotExist()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSubmitEventsToAGameTheyAreNotIn()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSubmitAnEventThatOccursInThePast()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanDeleteAnEventThatTheySubmitted()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotDeleteAnotherPlayersEvent()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotDeleteEventsThatHaveAlreadyHappened()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanUpdateAGameEvent()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotUpdateAGameEventWithInvalidEventId()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotUpdateAGameEventThatHasAlreadyOccurred()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotUpdateAnotherPlayersEvent()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayersCanViewAnyEventThatHasAlreadyOccurred()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanViewTheirOwnEventsThatOccurInTheFutureButOthersCannot()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void AdminsCanSeeAllGameEvents()
    {
        throw new NotImplementedException();
    }
}