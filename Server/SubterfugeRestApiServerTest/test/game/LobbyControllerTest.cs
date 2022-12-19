using System.Net;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class LobbyControllerTest
{
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
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        });
        Assert.AreEqual(ResponseType.ROOM_IS_FULL, exception.response.ResponseType);
    }

    [Test]
    public async Task BeingTheLastPlayerToJoinAGameWillStartTheGame()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby(maxPlayers: 2);
        await AccountUtils.AssertLogin(userTwo.User.Username);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.Status.IsSuccess);
        
        // Check to see that the game room is ONGOING
        GetLobbyResponse openLobbiesResponseAfterJoin = await TestUtils.GetClient().LobbyClient.GetLobbies(roomStatus: RoomStatus.Ongoing);
        Assert.AreEqual(openLobbiesResponseAfterJoin.Status.IsSuccess, true);
        Assert.AreEqual(1, openLobbiesResponseAfterJoin.Lobbies.Length);
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
    public async Task PlayerCanQueryTheLobbyListUsingVariousParameters()
    {
        var roomOne = await LobbyUtils.CreateLobby(
            "Room 1",
            maxPlayers: 7,
            isRanked: false,
            goal: Goal.Domination
        );
        var roomTwo = await LobbyUtils.CreateLobby(
            "Room 2",
            maxPlayers: 5,
            isRanked: true,
            goal: Goal.Domination
        );
        var roomThree = await LobbyUtils.CreateLobby(
            "Room 3",
            maxPlayers: 100,
            isRanked: false,
            goal: Goal.Mining
        );
        
        // Login to a different account to test searching for players in a room.
        await AccountUtils.AssertLogin(userTwo.User.Username);
        
        var roomFour = await LobbyUtils.CreateLobby(
            "Room 4",
            maxPlayers: 10,
            isRanked: true,
            goal: Goal.Mining
        );
        
        // All rooms are listed in the default lobby view.
        GetLobbyResponse allOpenLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies();
        Assert.AreEqual(allOpenLobbyResponse.Status.IsSuccess, true);
        Assert.AreEqual(4,allOpenLobbyResponse.Lobbies.Length);
        
        // Check filter by creator
        GetLobbyResponse createdByResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(createdByUserId: userOne.User.Id);
        Assert.AreEqual(3,createdByResponse.Lobbies.Length);
        Assert.IsTrue(createdByResponse.Lobbies.All(lobby => lobby.Creator.Id == userOne.User.Id));
        
        // Check filter by roomId
        GetLobbyResponse roomIdResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(roomId: roomOne.GameConfiguration.Id);
        Assert.AreEqual(1,roomIdResponse.Lobbies.Length);
        Assert.IsTrue(roomIdResponse.Lobbies.All(lobby => lobby.Id == roomOne.GameConfiguration.Id));
        
        // Check filter by game mode
        GetLobbyResponse goalFilterResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(goal: Goal.Mining);
        Assert.AreEqual(2,goalFilterResponse.Lobbies.Length);
        Assert.IsTrue(goalFilterResponse.Lobbies.All(lobby => lobby.GameSettings.Goal == Goal.Mining));
        
        // Check filter by ranked
        GetLobbyResponse rankedResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(isRanked: true);
        Assert.AreEqual(2,rankedResponse.Lobbies.Length);
        Assert.IsTrue(rankedResponse.Lobbies.All(lobby => lobby.GameSettings.IsRanked));
        
        // Check filter by ranked and Game Mode
        GetLobbyResponse rankedGameModeResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(isRanked: true, goal: Goal.Domination);
        Assert.AreEqual(1,rankedGameModeResponse.Lobbies.Length);
        Assert.IsTrue(rankedGameModeResponse.Lobbies.All(lobby => lobby.GameSettings.IsRanked && lobby.GameSettings.Goal == Goal.Domination));
        
        // Check filter by minPlayers
        GetLobbyResponse minPlayersResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(minPlayers: 9);
        Assert.AreEqual(2,minPlayersResponse.Lobbies.Length);
        Assert.IsTrue(minPlayersResponse.Lobbies.All(lobby => lobby.GameSettings.MaxPlayers > 9));
        
        // Check filter by maxPlayers
        GetLobbyResponse maxPlayersResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(maxPlayers: 9);
        Assert.AreEqual(2,maxPlayersResponse.Lobbies.Length);
        Assert.IsTrue(maxPlayersResponse.Lobbies.All(lobby => lobby.GameSettings.MaxPlayers < 9));
        
        // Ensure normal user cannot search for lobbies another player is in.
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.GetLobbies(userIdInRoom: userOne.User.Id);
        });
        Assert.AreEqual(HttpStatusCode.Forbidden, exception.rawResponse.StatusCode);
        
        // Normal user can search for lobbies they have already joined.
        GetLobbyResponse lobbyUserIsIn = await TestUtils.GetClient().LobbyClient.GetLobbies(userIdInRoom: userTwo.User.Id);
        Assert.AreEqual(1,lobbyUserIsIn.Lobbies.Length);
        Assert.IsTrue(lobbyUserIsIn.Lobbies.All(lobby => lobby.PlayersInLobby.Any(it => it.Id == userTwo.User.Id)));

        await TestUtils.CreateSuperUserAndLogin();
        // Ensure admin can view all lobbies.
        GetLobbyResponse adminAllOpenLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies();
        Assert.AreEqual(4,adminAllOpenLobbyResponse.Lobbies.Length);
        Assert.IsTrue(adminAllOpenLobbyResponse.Lobbies.All(lobby => lobby.RoomStatus == RoomStatus.Open));
        
        // Ensure admin can view all lobbies for other players.
        GetLobbyResponse adminPlayerLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(userIdInRoom: userOne.User.Id);
        Assert.AreEqual(3,adminPlayerLobbyResponse.Lobbies.Length);
        Assert.IsTrue(adminPlayerLobbyResponse.Lobbies.All(lobby => lobby.PlayersInLobby.Any(player => player.Id == userOne.User.Id)));
    }

    [Test]
    public async Task PlayerWhoCreatesALobbyIsAMemberOfThatLobby()
    {
        await LobbyUtils.CreateLobby();
        
        // View open rooms.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies();
        
        // Ensure the creator is a member
        Assert.IsTrue(openLobbiesResponse.Lobbies[0].PlayersInLobby.Any(it => it.Username == userOne.User.Username));
    }

    [Test]
    public async Task WhenALobbyIsCreatedTheGameConfigurationsAreCorrect()
    {
        await LobbyUtils.CreateLobby();
        
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
        Assert.AreEqual(1,openLobbiesResponse.Lobbies[0].PlayersInLobby.Count);
        Assert.AreEqual(userOne.User.Username,openLobbiesResponse.Lobbies[0].PlayersInLobby[0].Username);
    }

    [Test]
    public void IfTheCreatorOfALobbyLeavesTheGameIsDestroyed()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void IfTheCreatorOfALobbyLeavesTheGameNoPlayersAreStuckInTheLobby()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanStartAGameEarlyIfTwoPlayersAreInTheLobby()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotStartAGameEarlyWithNobodyInTheLobby()
    {
        throw new NotImplementedException();
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayerCannotSeeALobbyThatABlockedPlayerIsIn()
    {
        throw new NotImplementedException();
    }

    [Ignore("Not implemented")]
    [Test]
    public void PrivateGameRoomsCannotBeSeen()
    {
        throw new NotImplementedException();
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayersCanCreatePrivateGameRooms()
    {
        throw new NotImplementedException();
    }

    [Ignore("Not implemented")]
    [Test]
    public void PlayersCanJoinAPrivateLobbyIfTheyKnowTheLobbyId()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayersWhoRegisterWithTheSameDeviceIdCannotJoinTheSameGame()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void AdminsCanViewAnyOngoingGameTheyAreNotIn()
    {
        throw new NotImplementedException();
    }
}