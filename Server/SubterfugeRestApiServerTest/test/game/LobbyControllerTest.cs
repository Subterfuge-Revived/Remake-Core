using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
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
        TestUtils.Mongo.FlushAll();
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
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinResponse.ResponseDetail.IsSuccess);
        
        // View open rooms.
        var openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(openLobbiesResponse.IsSuccess(), true);

        var openLobbiesResponseData = openLobbiesResponse.GetOrThrow();
        
        Assert.AreEqual(1,openLobbiesResponseData.Lobbies.Length);
        Assert.AreEqual(userOne.User.Id,openLobbiesResponseData.Lobbies[0].Creator.Id);
        Assert.AreEqual(userOne.User.Username,openLobbiesResponseData.Lobbies[0].Creator.Username);
        Assert.AreEqual("My room!",openLobbiesResponseData.Lobbies[0].RoomName);
        Assert.AreEqual(RoomStatus.Open,openLobbiesResponseData.Lobbies[0].RoomStatus);
        Assert.AreEqual(false,openLobbiesResponseData.Lobbies[0].GameSettings.IsAnonymous);
        Assert.AreEqual(Goal.Domination,openLobbiesResponseData.Lobbies[0].GameSettings.Goal);
        Assert.AreEqual(2,openLobbiesResponseData.Lobbies[0].PlayersInLobby.Count);
        Assert.IsTrue(openLobbiesResponseData.Lobbies[0].PlayersInLobby.Any(it => it.Id == userTwo.User.Id));
    }

    [Test]
    public async Task PlayerCannotJoinTheSameGameTwice()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinResponseOne = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinResponseOne.ResponseDetail.IsSuccess);

        var exception = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.DUPLICATE, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCannotJoinAGameThatHasAlreadyStarted()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby(maxPlayers: 2);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.ResponseDetail.IsSuccess);
        
        
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        var exception = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.ROOM_IS_FULL, exception.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task BeingTheLastPlayerToJoinAGameWillStartTheGame()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby(maxPlayers: 2);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.ResponseDetail.IsSuccess);
        
        // Check to see that the game room is ONGOING
        var openLobbiesResponseAfterJoin = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest() { RoomStatus = RoomStatus.Ongoing });
        Assert.AreEqual(openLobbiesResponseAfterJoin.IsSuccess(), true);
        Assert.AreEqual(1, openLobbiesResponseAfterJoin.GetOrThrow().Lobbies.Length);
    }

    [Test]
    public async Task PlayerCanLeaveAGameRoom()
    {
        var lobbyResponse = await LobbyUtils.CreateLobby();
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinTwoResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), lobbyResponse.GameConfiguration.Id);
        Assert.IsTrue(joinTwoResponse.ResponseDetail.IsSuccess);
        
        // Check to see the player has joined
        await LobbyUtils.AssertPlayerInLobby(userTwo.User.Id);
        
        var leaveResponse = await TestUtils.GetClient().LobbyClient.LeaveRoom(lobbyResponse.GameConfiguration.Id);
        Assert.AreEqual(leaveResponse.IsSuccess(), true);
        
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
        var openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(openLobbiesResponse.IsSuccess(), true);
        Assert.AreEqual(4,openLobbiesResponse.GetOrThrow().Lobbies.Length);
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
        var allOpenLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open });
        Assert.AreEqual(allOpenLobbyResponse.IsSuccess(), true);
        Assert.AreEqual(4,allOpenLobbyResponse.GetOrThrow().Lobbies.Length);
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByCreatorId()
    {
        await InitLobbiesForQueryParams();
        
        // Check filter by creator
        var createdByResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ CreatedByUserId = userOne.User.Id, RoomStatus = RoomStatus.Open });
        Assert.AreEqual(1,createdByResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(createdByResponse.GetOrThrow().Lobbies.All(lobby => lobby.Creator.Id == userOne.User.Id));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByRoomId()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by roomId
        var roomIdResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomId = rooms[1].GameConfiguration.Id});
        Assert.AreEqual(1,roomIdResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(roomIdResponse.GetOrThrow().Lobbies.All(lobby => lobby.Id == rooms[1].GameConfiguration.Id));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByGameMode()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by game mode
        var goalFilterResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ Goal = Goal.Mining});
        Assert.AreEqual(2,goalFilterResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(goalFilterResponse.GetOrThrow().Lobbies.All(lobby => lobby.GameSettings.Goal == Goal.Mining));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByRankedStatus()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by ranked
        var rankedResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ IsRanked = false, RoomStatus = RoomStatus.Open});
        Assert.AreEqual(2,rankedResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(rankedResponse.GetOrThrow().Lobbies.All(lobby => lobby.GameSettings.IsRanked == false));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByWithMultipleFilters()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by ranked and Game Mode
        var rankedGameModeResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ IsRanked = true, Goal = Goal.Mining});
        Assert.AreEqual(1,rankedGameModeResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(rankedGameModeResponse.GetOrThrow().Lobbies.All(lobby => lobby.GameSettings.IsRanked && lobby.GameSettings.Goal == Goal.Mining));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByMinPlayers()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by minPlayers
        var minPlayersResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ MinPlayers = 9});
        Assert.AreEqual(2,minPlayersResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(minPlayersResponse.GetOrThrow().Lobbies.All(lobby => lobby.GameSettings.MaxPlayers > 9));
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesByMaxPlayers()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Check filter by maxPlayers
        var maxPlayersResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ MaxPlayers = 9, RoomStatus = RoomStatus.Open });
        Assert.AreEqual(2,maxPlayersResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(maxPlayersResponse.GetOrThrow().Lobbies.All(lobby => lobby.GameSettings.MaxPlayers < 9));
    }
    
    [Test]
    public async Task PlayerCannotQueryLobbiesThatAnotherPlayerIsIn()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Ensure normal user cannot search for lobbies another player is in.
        var exception = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ UserIdInRoom = userOne.User.Id});
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, exception.ResponseDetail.ResponseType);
    }
    
    [Test]
    public async Task PlayerCanQueryLobbiesTheyHaveJoined()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        // Normal user can search for lobbies they have already joined.
        var lobbyUserIsIn = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ UserIdInRoom = userTwo.User.Id});
        Assert.AreEqual(3,lobbyUserIsIn.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(lobbyUserIsIn.GetOrThrow().Lobbies.All(lobby => lobby.PlayersInLobby.Any(it => it.Id == userTwo.User.Id)));
    }
    
    [Test]
    public async Task AdminCanViewAllLobbies()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        await TestUtils.CreateSuperUserAndLogin();
        // Ensure admin can view all lobbies.
        var adminAllOpenLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(4,adminAllOpenLobbyResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(adminAllOpenLobbyResponse.GetOrThrow().Lobbies.All(lobby => lobby.RoomStatus == RoomStatus.Open));
    }
    
    [Test]
    public async Task AdminCanSearchLobbiesOtherPlayersAreIn()
    {
        var rooms = await InitLobbiesForQueryParams();
        
        await TestUtils.CreateSuperUserAndLogin();
        // Ensure admin can view all lobbies for other players.
        var adminPlayerLobbyResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ UserIdInRoom = userTwo.User.Id});
        Assert.AreEqual(3,adminPlayerLobbyResponse.GetOrThrow().Lobbies.Length);
        Assert.IsTrue(adminPlayerLobbyResponse.GetOrThrow().Lobbies.All(lobby => lobby.PlayersInLobby.Any(player => player.Id == userTwo.User.Id)));
    }

    [Test]
    public async Task PlayerWhoCreatesALobbyIsAMemberOfThatLobby()
    {
        await LobbyUtils.CreateLobby();
        
        // View open rooms.
        var openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        
        // Ensure the creator is a member
        Assert.IsTrue(openLobbiesResponse.GetOrThrow().Lobbies[0].PlayersInLobby.Any(it => it.Username == userOne.User.Username));
    }

    [Test]
    public async Task WhenALobbyIsCreatedTheGameConfigurationsAreCorrect()
    {
        await LobbyUtils.CreateLobby();
        
        // View open rooms.
        var openLobbiesResponse = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(openLobbiesResponse.IsSuccess(), true);

        var openLobbiesResponseData = openLobbiesResponse.GetOrThrow();
        Assert.AreEqual(1,openLobbiesResponseData.Lobbies.Length);
        Assert.AreEqual(userOne.User.Id,openLobbiesResponseData.Lobbies[0].Creator.Id);
        Assert.AreEqual(userOne.User.Username,openLobbiesResponseData.Lobbies[0].Creator.Username);
        Assert.AreEqual("My room!",openLobbiesResponseData.Lobbies[0].RoomName);
        Assert.AreEqual(RoomStatus.Open,openLobbiesResponseData.Lobbies[0].RoomStatus);
        Assert.AreEqual(false,openLobbiesResponseData.Lobbies[0].GameSettings.IsAnonymous);
        Assert.AreEqual(Goal.Domination,openLobbiesResponseData.Lobbies[0].GameSettings.Goal);
        Assert.AreEqual(1,openLobbiesResponseData.Lobbies[0].PlayersInLobby.Count);
        Assert.AreEqual(userOne.User.Username,openLobbiesResponseData.Lobbies[0].PlayersInLobby[0].Username);
    }

    [Test]
    public async Task IfTheCreatorOfALobbyLeavesTheGameIsDestroyed()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();
        
        // Leave the lobby right away
        await TestUtils.GetClient().LobbyClient.LeaveRoom(response.GameConfiguration.Id);
        
        // View open rooms.
        var lobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(lobbies.IsSuccess(), true);
        Assert.AreEqual(0,lobbies.GetOrThrow().Lobbies.Length);
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
        var lobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest());
        Assert.AreEqual(lobbies.IsSuccess(), true);
        Assert.AreEqual(0,lobbies.GetOrThrow().Lobbies.Length);
        
        // Login as player two and try to get a list of lobbies you are in
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var userTwoLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest() { UserIdInRoom = userTwo.User.Id });
        Assert.AreEqual(userTwoLobbies.IsSuccess(), true);
        Assert.AreEqual(0,userTwoLobbies.GetOrThrow().Lobbies.Length);
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
        var openLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open});
        Assert.AreEqual(openLobbies.IsSuccess(), true);
        Assert.AreEqual(0,openLobbies.GetOrThrow().Lobbies.Length);
        
        // Ongoing rooms will show the game as started
        var ongoingLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(ongoingLobbies.IsSuccess(), true);
        Assert.AreEqual(1,ongoingLobbies.GetOrThrow().Lobbies.Length);
        Assert.Less(timeBeforeStart, ongoingLobbies.GetOrThrow().Lobbies[0].TimeStarted);
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
        var openLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open});
        Assert.AreEqual(openLobbies.ResponseDetail.IsSuccess, true);
        Assert.AreEqual(0,openLobbies.GetOrThrow().Lobbies.Length);
        
        // Ongoing rooms will show the game as started
        var ongoingLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(ongoingLobbies.ResponseDetail.IsSuccess, true);
        Assert.AreEqual(1,ongoingLobbies.GetOrThrow().Lobbies.Length);
        Assert.Less(timeBeforeStart, ongoingLobbies.GetOrThrow().Lobbies[0].TimeStarted);
        
        // Player three cannot see the game.
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        var playerThreeLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(playerThreeLobbies.ResponseDetail.IsSuccess, true);
        Assert.AreEqual(0,playerThreeLobbies.GetOrThrow().Lobbies.Length);
    }

    [Test]
    public async Task PlayerCannotStartAGameEarlyWithNobodyInTheLobby()
    {
        // Create a lobby
        var response = await LobbyUtils.CreateLobby();

        // Start the room as the creator
        var timeBeforeStart = DateTime.UtcNow;
        var exception = await TestUtils.GetClient().LobbyClient.StartGameEarly(response.GameConfiguration.Id);
        Assert.IsFalse(exception.IsSuccess());
        Assert.AreEqual(ResponseType.VALIDATION_ERROR, exception.ResponseDetail.ResponseType);
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
        var openLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Open});
        Assert.AreEqual(openLobbies.ResponseDetail.IsSuccess, true);
        Assert.AreEqual(0,openLobbies.GetOrThrow().Lobbies.Length);
        
        // Ongoing rooms will show the game as started
        var ongoingLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(ongoingLobbies.ResponseDetail.IsSuccess, true);
        Assert.AreEqual(1,ongoingLobbies.GetOrThrow().Lobbies.Length);
        Assert.Less(timeBeforeStart, ongoingLobbies.GetOrThrow().Lobbies[0].TimeStarted);
        
        // Player three cannot see the game.
        TestUtils.GetClient().UserApi.SetToken(userThree.Token);
        var playerThreeLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(playerThreeLobbies.ResponseDetail.IsSuccess, true);
        Assert.AreEqual(0,playerThreeLobbies.GetOrThrow().Lobbies.Length);
        
        // Admin can see the game.
        await TestUtils.CreateSuperUserAndLogin();
        var adminLobbies = await TestUtils.GetClient().LobbyClient.GetLobbies(new GetLobbyRequest(){ RoomStatus = RoomStatus.Ongoing});
        Assert.AreEqual(adminLobbies.ResponseDetail.IsSuccess, true);
        Assert.AreEqual(1,adminLobbies.GetOrThrow().Lobbies.Length);
    }
}