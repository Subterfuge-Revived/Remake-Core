using NUnit.Framework;
using SubterfugeRestApiClient;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class LobbyControllerTest
{
    private SubterfugeClient client = TestUtils.GetClient();

    [SetUp]
    public void Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
    }

    [Test]
    public void PlayerCanCreateAGameRoom()
    {
    }

    [Test]
    public void PlayerCanJoinAGameRoom()
    {
    }

    [Test]
    public void PlayerCannotJoinTheSameGameTwice()
    {
    }

    [Test]
    public void PlayerCannotJoinAGameThatHasAlreadyStarted()
    {
    }

    [Test]
    public void BeingTheLastPlayerToJoinAGameWillStartTheGame()
    {
    }

    [Test]
    public void PlayerCanLeaveAGameRoom()
    {
    }

    [Test]
    public void PlayerCanSeeAListOfAvaliableRooms()
    {
    }

    [Test]
    public void PlayerWhoCreatesALobbyIsAMemberOfThatLobby()
    {
    }

    [Test]
    public void IfTheCreatorOfALobbyLeavesTheGameIsDestroyed()
    {
    }

    [Test]
    public void IfTheCreatorOfALobbyLeavesTheGameNoPlayersAreStuckInTheLobby()
    {
    }

    [Test]
    public void PlayerCanStartAGameEarlyIfTwoPlayersAreInTheLobby()
    {
    }

    [Test]
    public void PlayerCannotStartAGameEarlyWithNobodyInTheLobby()
    {
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayerCannotSeeALobbyThatABlockedPlayerIsIn()
    {
    }

    [Ignore("Not implemented")]
    [Test]
    public void PrivateGameRoomsCannotBeSeen()
    {
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayersCanCreatePrivateGameRooms()
    {
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayersCanJoinAPrivateLobbyIfTheyKnowTheLobbyId()
    {
    }

    [Test]
    public void PlayersWhoRegisterWithTheSameDeviceIdCannotJoinTheSameGame()
    {
    }

    [Test]
    public void AdminsCanViewAnyOngoingGameTheyAreNotIn()
    {
    }
}