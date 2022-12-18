using NUnit.Framework;
using NUnit.Framework.Internal;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class LobbyControllerTest
{
    private SubterfugeClient client = TestUtils.GetClient();

    private AccountRegistrationResponse userOne;
    private AccountRegistrationResponse userTwo;
    private AccountRegistrationResponse userThree;
    

    [SetUp]
    public async Task Setup()
    {
        TestUtils.Mongo.FlushCollections();
        TestUtils.GetClient().UserApi.Logout();
        userThree = await AccountUtils.AssertRegisterAccountAndAuthorized("UserThree");
        userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");

    }

    [Test]
    public async Task PlayerCanCreateAGameRoom()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
    }

    [Test]
    public async Task PlayerCanJoinAGameRoom()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        await AccountUtils.AssertLogin(userTwo.User.Username);
        var joinResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinResponse.Status.IsSuccess);
        
        // View open rooms.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies();
        Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
        Assert.AreEqual(1,openLobbiesResponse.Lobbies.Length);
        Assert.AreEqual(userOne.User.Id,openLobbiesResponse.Lobbies[0].Creator.Id);
        Assert.AreEqual(userOne.User.Username,openLobbiesResponse.Lobbies[0].Creator.Username);
        Assert.AreEqual("My room!",openLobbiesResponse.Lobbies[0].RoomName);
        Assert.AreEqual(RoomStatus.Open,openLobbiesResponse.Lobbies[0].RoomStatus);
        Assert.AreEqual(false,openLobbiesResponse.Lobbies[0].GameSettings.IsAnonymous);
        Assert.AreEqual(Goal.Domination,openLobbiesResponse.Lobbies[0].GameSettings.Goal);
        Assert.AreEqual(2,openLobbiesResponse.Lobbies[0].PlayersInLobby.Count);
        Assert.IsTrue(openLobbiesResponse.Lobbies[0].PlayersInLobby.Any(it => it.Id == userTwo.User.Id));
    }

    [Test]
    public async Task PlayerCannotJoinTheSameGameTwice()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        await AccountUtils.AssertLogin(userTwo.User.Username);
        var joinResponseOne = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinResponseOne.Status.IsSuccess);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        });
        Assert.AreEqual(ResponseType.PLAYER_ALREADY_IN_LOBBY, exception.response.ResponseType);
    }

    [Test]
    public async Task PlayerCannotJoinAGameThatHasAlreadyStarted()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby(maxPlayers: 2);
        await AccountUtils.AssertLogin(userTwo.User.Username);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.Status.IsSuccess);
        
        await AccountUtils.AssertLogin(userThree.User.Username);
        var joinThreeResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsFalse(joinThreeResponse.Status.IsSuccess);
    }

    [Test]
    public async Task BeingTheLastPlayerToJoinAGameWillStartTheGame()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby(maxPlayers: 2);
        await AccountUtils.AssertLogin(userTwo.User.Username);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.Status.IsSuccess);
        
        // Check to see the room is not visible.
        GetLobbyResponse openLobbiesResponseAfterJoin = await TestUtils.GetClient().LobbyClient.GetLobbies();
        Assert.AreEqual(openLobbiesResponseAfterJoin.Status.IsSuccess, true);
        Assert.AreEqual(0, openLobbiesResponseAfterJoin.Lobbies.Length);

        // TODO: Update the lobby GET endpoint to add query params.
        // Should be able to query the ongoing rooms
        throw new NotImplementedException();
    }

    [Test]
    public async Task PlayerCanLeaveAGameRoom()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        await AccountUtils.AssertLogin(userTwo.User.Username);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.Status.IsSuccess);
        
        // Check to see the player has joined
        await LobbyUtils.AssertPlayerInLobby(userTwo.User.Id);
        
        LeaveRoomResponse leaveResponse = await TestUtils.GetClient().LobbyClient.LeaveRoom(lobbyResponse.GameConfiguration.Id);
        Assert.AreEqual(leaveResponse.Status.IsSuccess, true);
        
        // Check to see the player has left
        await LobbyUtils.AssertPlayerInLobby(userTwo.User.Id, isInLobby: false);
    }

    [Test]
    public async Task PlayerCanSeeAListOfAvaliableRooms()
    {
        await LobbyUtils.CreateLobby("Room 1");
        await LobbyUtils.CreateLobby("Room 2");
        await LobbyUtils.CreateLobby("Room 3");
        await LobbyUtils.CreateLobby("Room 4");
        
        // All rooms are listed in the lobby view.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies();
        Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
        Assert.AreEqual(4,openLobbiesResponse.Lobbies.Length);
    }

    [Test]
    public async Task PlayerWhoCreatesALobbyIsAMemberOfThatLobby()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        
        // View open rooms.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies();
        
        // Ensure the creator is a member
        Assert.AreEqual(userThree.User.Username,openLobbiesResponse.Lobbies[0].PlayersInLobby[0].Username);
    }

    [Test]
    public async Task WhenALobbyIsCreatedTheGameConfigurationsAreCorrect()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        
        // View open rooms.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies();
        Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
        Assert.AreEqual(1,openLobbiesResponse.Lobbies.Length);
        Assert.AreEqual(userThree.User.Id,openLobbiesResponse.Lobbies[0].Creator.Id);
        Assert.AreEqual(userThree.User.Username,openLobbiesResponse.Lobbies[0].Creator.Username);
        Assert.AreEqual("My room!",openLobbiesResponse.Lobbies[0].RoomName);
        Assert.AreEqual(RoomStatus.Open,openLobbiesResponse.Lobbies[0].RoomStatus);
        Assert.AreEqual(false,openLobbiesResponse.Lobbies[0].GameSettings.IsAnonymous);
        Assert.AreEqual(Goal.Domination,openLobbiesResponse.Lobbies[0].GameSettings.Goal);
        Assert.AreEqual(1,openLobbiesResponse.Lobbies[0].PlayersInLobby.Count);
        Assert.AreEqual(userThree.User.Username,openLobbiesResponse.Lobbies[0].PlayersInLobby[0].Username);
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