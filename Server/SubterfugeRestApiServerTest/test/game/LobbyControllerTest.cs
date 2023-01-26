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
    private AccountRegistrationResponse sameDeviceUserOne;
    private AccountRegistrationResponse sameDeviceUserTwo;
    

    [SetUp]
    public async Task Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
        
        sameDeviceUserOne = await AccountUtils.AssertRegisterAccountAndAuthorized("SameDeviceUserOne", deviceId: "SameDevice");
        sameDeviceUserTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("SameDeviceUserTwo", deviceId: "SameDevice");
        
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
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinResponse.Status.IsSuccess);
        
        // View open rooms.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
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
        
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinResponseOne = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinResponseOne.Status.IsSuccess);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        });
        Assert.AreEqual(ResponseType.DUPLICATE, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCannotJoinAGameThatHasAlreadyStarted()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby(maxPlayers: 2);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.Status.IsSuccess);
        
        
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        });
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task BeingTheLastPlayerToJoinAGameWillStartTheGame()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby(maxPlayers: 2);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.Status.IsSuccess);
        
        // Check to see that the game room is ONGOING
        GetLobbyResponse openLobbiesResponseAfterJoin = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest() { RoomStatus = RoomStatus.Ongoing });
        Assert.AreEqual(openLobbiesResponseAfterJoin.Status.IsSuccess, true);
        Assert.AreEqual(1, openLobbiesResponseAfterJoin.Lobbies.Length);
    }

    [Test]
    public async Task PlayerCanLeaveAGameRoom()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
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
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
        Assert.AreEqual(4,openLobbiesResponse.Lobbies.Length);
    }

    private async Task<CreateRoomResponse[]> InitLobbiesForQueryParams()
    {
        var roomOne = await LobbyUtils.CreateLobby(
            "Room 1",
            maxPlayers: 7,
            isRanked: false,
            goal: Goal.Domination
        );
        
        // Login to a different account to test searching for players in a room.
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
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
        var roomFour = await LobbyUtils.CreateLobby(
            "Room 4",
            maxPlayers: 10,
            isRanked: true,
            goal: Goal.Mining
        );

        return new CreateRoomResponse[] { roomOne, roomTwo, roomThree, roomFour };
    }

    [Test]
    public async Task PlayerCanQueryLobbiesByRoomStatus()
    {
        await InitLobbiesForQueryParams();
        
        // All rooms are listed in the default lobby view.
        GetLobbyResponse allOpenLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open});
        Assert.AreEqual(allOpenLobbyResponse.Status.IsSuccess, true);
        Assert.AreEqual(4,allOpenLobbyResponse.Lobbies.Length);
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByCreatorId()
    {
        await InitLobbiesForQueryParams();
        
        // Check filter by creator
        GetLobbyResponse createdByResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ CreatedByUserId = userOne.User.Id});
        Assert.AreEqual(1,createdByResponse.Lobbies.Length);
        Assert.IsTrue(createdByResponse.Lobbies.All(lobby => lobby.Creator.Id == userOne.User.Id));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByRoomId()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by roomId
        GetLobbyResponse roomIdResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomId = rooms[1].GameConfiguration.Id});
        Assert.AreEqual(1,roomIdResponse.Lobbies.Length);
        Assert.IsTrue(roomIdResponse.Lobbies.All(lobby => lobby.Id == rooms[1].GameConfiguration.Id));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByGameMode()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by game mode
        GetLobbyResponse goalFilterResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ Goal = Goal.Mining});
        Assert.AreEqual(2,goalFilterResponse.Lobbies.Length);
        Assert.IsTrue(goalFilterResponse.Lobbies.All(lobby => lobby.GameSettings.Goal == Goal.Mining));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByRankedStatus()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by ranked
        GetLobbyResponse rankedResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ IsRanked = false});
        Assert.AreEqual(2,rankedResponse.Lobbies.Length);
        Assert.IsTrue(rankedResponse.Lobbies.All(lobby => lobby.GameSettings.IsRanked == false));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByWithMultipleFilters()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by ranked and Game Mode
        GetLobbyResponse rankedGameModeResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ IsRanked = true, Goal = Goal.Mining});
        Assert.AreEqual(1,rankedGameModeResponse.Lobbies.Length);
        Assert.IsTrue(rankedGameModeResponse.Lobbies.All(lobby => lobby.GameSettings.IsRanked && lobby.GameSettings.Goal == Goal.Mining));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByMinPlayers()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by minPlayers
        GetLobbyResponse minPlayersResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ MinPlayers = 9});
        Assert.AreEqual(2,minPlayersResponse.Lobbies.Length);
        Assert.IsTrue(minPlayersResponse.Lobbies.All(lobby => lobby.GameSettings.MaxPlayers > 9));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByMaxPlayers()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by maxPlayers
        GetLobbyResponse maxPlayersResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ MaxPlayers = 9});
        Assert.AreEqual(2,maxPlayersResponse.Lobbies.Length);
        Assert.IsTrue(maxPlayersResponse.Lobbies.All(lobby => lobby.GameSettings.MaxPlayers < 9));
    }
    
    [Test]
    public async Task PlayerCannotQueryLobbiesThatAnotherPlayerIsIn()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Ensure normal user cannot search for lobbies another player is in.
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ UserIdInRoom = userOne.User.Id});
        });
        Assert.AreEqual(HttpStatusCode.Forbidden, exception.rawResponse.StatusCode);
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesTheyHaveJoined()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Normal user can search for lobbies they have already joined.
        GetLobbyResponse lobbyUserIsIn = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ UserIdInRoom = userTwo.User.Id});
        Assert.AreEqual(3,lobbyUserIsIn.Lobbies.Length);
        Assert.IsTrue(lobbyUserIsIn.Lobbies.All(lobby => lobby.PlayersInLobby.Any(it => it.Id == userTwo.User.Id)));
    }
    
    [Test]
    public async Task AdminCanViewAllLobbies()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        await TestUtils.CreateSuperUserAndLogin();
        // Ensure admin can view all lobbies.
        GetLobbyResponse adminAllOpenLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(4,adminAllOpenLobbyResponse.Lobbies.Length);
        Assert.IsTrue(adminAllOpenLobbyResponse.Lobbies.All(lobby => lobby.RoomStatus == RoomStatus.Open));
    }
    
    [Test]
    public async Task AdminCanSearchLobbiesOtherPlayersAreIn()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        await TestUtils.CreateSuperUserAndLogin();
        // Ensure admin can view all lobbies for other players.
        GetLobbyResponse adminPlayerLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ UserIdInRoom = userTwo.User.Id});
        Assert.AreEqual(3,adminPlayerLobbyResponse.Lobbies.Length);
        Assert.IsTrue(adminPlayerLobbyResponse.Lobbies.All(lobby => lobby.PlayersInLobby.Any(player => player.Id == userTwo.User.Id)));
    }

    [Test]
    public async Task PlayerWhoCreatesALobbyIsAMemberOfThatLobby()
    {
        await LobbyUtils.CreateLobby();
        
        // View open rooms.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        
        // Ensure the creator is a member
        Assert.IsTrue(openLobbiesResponse.Lobbies[0].PlayersInLobby.Any(it => it.Username == userOne.User.Username));
    }

    [Test]
    public async Task WhenALobbyIsCreatedTheGameConfigurationsAreCorrect()
    {
        await LobbyUtils.CreateLobby();
        
        // View open rooms.
        GetLobbyResponse openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
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
    public async Task IfTheCreatorOfALobbyLeavesTheGameIsDestroyed()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();
        
        // Leave the lobby right away
        await TestUtils.GetClient().LobbyClient.LeaveRoom(response.GameConfiguration.Id);
        
        // View open rooms.
        GetLobbyResponse lobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(lobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,lobbies.Lobbies.Length);
    }

    [Test]
    public async Task IfTheCreatorOfALobbyLeavesTheGameNoPlayersAreStuckInTheLobby()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();
        
        // Login as another player and join the created lobby
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest() { }, response.GameConfiguration.Id);
        
        // Leave the room as the creator
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        await TestUtils.GetClient().LobbyClient.LeaveRoom(response.GameConfiguration.Id);
        
        // View open rooms.
        GetLobbyResponse lobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(lobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,lobbies.Lobbies.Length);
        
        // Login as player two and try to get a list of lobbies you are in
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        GetLobbyResponse userTwoLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest() { UserIdInRoom = userTwo.User.Id });
        Assert.AreEqual(userTwoLobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,userTwoLobbies.Lobbies.Length);
    }

    [Test]
    public async Task PlayerCanStartAGameEarlyIfTwoPlayersAreInTheLobby()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();
        
        // Login as another player and join the created lobby
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest() { }, response.GameConfiguration.Id);
        
        // Start the room as the creator
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        var timeBeforeStart = DateTime.UtcNow;
        await TestUtils.GetClient().LobbyClient.StartGameEarly(response.GameConfiguration.Id);
        
        // View open rooms should not show any lobbies.
        GetLobbyResponse openLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open});
        Assert.AreEqual(openLobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,openLobbies.Lobbies.Length);
        
        // Ongoing rooms will show the game as started
        GetLobbyResponse ongoingLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(ongoingLobbies.Status.IsSuccess, true);
        Assert.AreEqual(1,ongoingLobbies.Lobbies.Length);
        Assert.Less(timeBeforeStart, ongoingLobbies.Lobbies[0].TimeStarted);
    }
    
    [Test]
    public async Task PlayerCannotSeeOngoingLobbiesTheyAreNotIn()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();
        
        // Login as another player and join the created lobby
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest() { }, response.GameConfiguration.Id);
        
        // Start the room as the creator
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        var timeBeforeStart = DateTime.UtcNow;
        await TestUtils.GetClient().LobbyClient.StartGameEarly(response.GameConfiguration.Id);
        
        // View open rooms should not show any lobbies.
        GetLobbyResponse openLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open});
        Assert.AreEqual(openLobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,openLobbies.Lobbies.Length);
        
        // Ongoing rooms will show the game as started
        GetLobbyResponse ongoingLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(ongoingLobbies.Status.IsSuccess, true);
        Assert.AreEqual(1,ongoingLobbies.Lobbies.Length);
        Assert.Less(timeBeforeStart, ongoingLobbies.Lobbies[0].TimeStarted);
        
        // Player three cannot see the game.
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        GetLobbyResponse playerThreeLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(playerThreeLobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,playerThreeLobbies.Lobbies.Length);
    }

    [Test]
    public async Task PlayerCannotStartAGameEarlyWithNobodyInTheLobby()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();

        // Start the room as the creator
        var timeBeforeStart = DateTime.UtcNow;
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.StartGameEarly(response.GameConfiguration.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
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
    public async Task PlayersWhoRegisterWithTheSameDeviceIdCannotJoinTheSameGame()
    {
        // Create a lobby
        TestUtils.GetClient().UserApi.SetToken(sameDeviceUserOne.Token);
        var response = await LobbyUtils.CreateLobby();
        
        // Login as another player using the same device ID and join the created lobby
        TestUtils.GetClient().UserApi.SetToken(sameDeviceUserTwo.Token);
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest() { }, response.GameConfiguration.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
    }

    [Test]
    public async Task AdminsCanViewAnyOngoingGameTheyAreNotIn()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();
        
        // Login as another player and join the created lobby
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest() { }, response.GameConfiguration.Id);
        
        // Start the room as the creator
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
        var timeBeforeStart = DateTime.UtcNow;
        await TestUtils.GetClient().LobbyClient.StartGameEarly(response.GameConfiguration.Id);
        
        // View open rooms should not show any lobbies.
        GetLobbyResponse openLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open});
        Assert.AreEqual(openLobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,openLobbies.Lobbies.Length);
        
        // Ongoing rooms will show the game as started
        GetLobbyResponse ongoingLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(ongoingLobbies.Status.IsSuccess, true);
        Assert.AreEqual(1,ongoingLobbies.Lobbies.Length);
        Assert.Less(timeBeforeStart, ongoingLobbies.Lobbies[0].TimeStarted);
        
        // Player three cannot see the game.
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        GetLobbyResponse playerThreeLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(playerThreeLobbies.Status.IsSuccess, true);
        Assert.AreEqual(0,playerThreeLobbies.Lobbies.Length);
        
        // Admin can see the game.
        await TestUtils.CreateSuperUserAndLogin();
        GetLobbyResponse adminLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(adminLobbies.Status.IsSuccess, true);
        Assert.AreEqual(1,adminLobbies.Lobbies.Length);
    }
}