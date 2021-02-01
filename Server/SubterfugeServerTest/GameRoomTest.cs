using System;
using System.Linq;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using SubterfugeServerConsole.Connections.Models;
using SubterfugeServerConsole.Responses;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class GameRoomTest
    {
        SubterfugeClient.SubterfugeClient client;
        private AuthTestHelper authHelper;

        [SetUp]
        public void Setup()
        {
            client = ClientHelper.GetClient();
            
            // Clear the database every test.
            RedisConnector.Server.FlushDatabase();
            
            
            // Create three new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount("userOne");
            authHelper.createAccount("userTwo");
            authHelper.createAccount("userThree");
            authHelper.loginToAccount("userOne");
        }

        [Test]
        public void PlayerCanCreateAGameRoom()
        {
            var roomName = "My Room!";
            var anon = false;
            var isRanked = false;
            var goal = Goal.Domination;
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = anon,
                Goal = goal,
                MaxPlayers = 5,
                IsRanked = isRanked,
                RoomName = roomName,
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].RoomId);
            Assert.AreEqual(authHelper.getAccountId("userOne"),openLobbiesResponse.Rooms[0].Creator.Id);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Creator.Username);
            Assert.AreEqual(roomName,openLobbiesResponse.Rooms[0].RoomName);
            Assert.AreEqual(RoomStatus.Open,openLobbiesResponse.Rooms[0].RoomStatus);
            // Assert.AreEqual(isRanked,roomDataResponse.Rooms[0].RankedInformation.IsRanked);
            Assert.AreEqual(anon,openLobbiesResponse.Rooms[0].Anonymous);
            Assert.AreEqual(goal,openLobbiesResponse.Rooms[0].Goal);
            Assert.AreEqual(1,openLobbiesResponse.Rooms[0].Players.Count);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Players[0].Username);
        }

        [Test]
        public void PlayerCanJoinAGameRoom()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 5,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;

            authHelper.loginToAccount("userTwo");
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].RoomId);
            // Ensure the creator is a member of the game
            Assert.AreEqual(1,openLobbiesResponse.Rooms[0].Players.Count);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Players[0].Username);
            
            JoinRoomRequest joinRequest = new JoinRoomRequest()
            {
                RoomId = roomId,
            };

            JoinRoomResponse joinResponse = client.JoinRoom(joinRequest);
            Assert.AreEqual(joinResponse.Status.IsSuccess, true);
            
            OpenLobbiesResponse openLobbiesResponseAfterJoin = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponseAfterJoin.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponseAfterJoin.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponseAfterJoin.Rooms[0].RoomId);
            Assert.AreEqual(2,openLobbiesResponseAfterJoin.Rooms[0].Players.Count);
            Assert.IsTrue(openLobbiesResponseAfterJoin.Rooms[0].Players.Any(it => it.Id == authHelper.getAccountId("userTwo")));
        }

        [Test]
        public void PlayerCannotJoinTheSameGameTwice()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 5,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;

            authHelper.loginToAccount("userTwo");

            JoinRoomRequest joinRequest = new JoinRoomRequest()
            {
                RoomId = roomId,
            };

            JoinRoomResponse joinResponse = client.JoinRoom(joinRequest);
            Assert.AreEqual(joinResponse.Status.IsSuccess, true);
            
            var exception = client.JoinRoom(joinRequest);
            Assert.AreEqual(exception.Status.IsSuccess, false);
            Assert.AreEqual(exception.Status.Detail, ResponseType.DUPLICATE.ToString());
        }

        [Test]
        public void PlayerCannotJoinAGameThatHasAlreadyStarted()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 2,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;

            authHelper.loginToAccount("userTwo");

            JoinRoomRequest joinRequest = new JoinRoomRequest()
            {
                RoomId = roomId,
            };

            JoinRoomResponse joinResponse = client.JoinRoom(joinRequest);
            Assert.AreEqual(joinResponse.Status.IsSuccess, true);
            
            authHelper.loginToAccount("userThree");
            var exception = client.JoinRoom(joinRequest);
            Assert.AreEqual(exception.Status.IsSuccess, false);
            Assert.AreEqual(exception.Status.Detail, ResponseType.ROOM_IS_FULL.ToString());
        }
        
        [Test]
        public void BeingTheLastPlayerToJoinAGameWillStartTheGame()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 2,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;

            authHelper.loginToAccount("userTwo");

            JoinRoomRequest joinRequest = new JoinRoomRequest()
            {
                RoomId = roomId,
            };

            JoinRoomResponse joinResponse = client.JoinRoom(joinRequest);
            Assert.AreEqual(joinResponse.Status.IsSuccess, true);
            
            // Check to see the room is not visible.
            OpenLobbiesResponse openLobbiesResponseAfterJoin = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponseAfterJoin.Status.IsSuccess, true);
            Assert.AreEqual(0,openLobbiesResponseAfterJoin.Rooms.Count);
            
            // Check to see the player can see the game because they are a member.
            PlayerCurrentGamesResponse playerGamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            Assert.AreEqual(playerGamesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,playerGamesResponse.Games.Count);
        }

        [Test]
        public void PlayerCanLeaveAGameRoom()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 5,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;

            authHelper.loginToAccount("userTwo");
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].RoomId);
            // Ensure the creator is a member of the game
            Assert.AreEqual(1,openLobbiesResponse.Rooms[0].Players.Count);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Players[0].Username);
            
            JoinRoomRequest joinRequest = new JoinRoomRequest()
            {
                RoomId = roomId,
            };

            JoinRoomResponse joinResponse = client.JoinRoom(joinRequest);
            Assert.AreEqual(joinResponse.Status.IsSuccess, true);
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponsAfterJoin = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponsAfterJoin.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponsAfterJoin.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponsAfterJoin.Rooms[0].RoomId);
            Assert.AreEqual(2,openLobbiesResponsAfterJoin.Rooms[0].Players.Count);
            
            LeaveRoomRequest leaveRequest = new LeaveRoomRequest()
            {
                RoomId = roomId
            };

            LeaveRoomResponse leaveResponse = client.LeaveRoom(leaveRequest);
            Assert.AreEqual(leaveResponse.Status.IsSuccess, true);
            
            // Ensure that the player has left the game.
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponsAfterLeave = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponsAfterLeave.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponsAfterLeave.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponsAfterLeave.Rooms[0].RoomId);
            Assert.AreEqual(1,openLobbiesResponsAfterLeave.Rooms[0].Players.Count);
        }

        [Test]
        public void PlayerCanSeeAListOfAvaliableRooms()
        {
            authHelper.CreateGameRoom("room1");
            authHelper.CreateGameRoom("room2");
            authHelper.CreateGameRoom("room3");
            authHelper.CreateGameRoom("room4");

            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(4,openLobbiesResponse.Rooms.Count);
            
            
            authHelper.CreateGameRoom("room5");
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponseAfterCreate = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponseAfterCreate.Status.IsSuccess, true);
            Assert.AreEqual(5,openLobbiesResponseAfterCreate.Rooms.Count);
        }

        [Test]
        public void PlayerWhoCreatesALobbyIsAMemberOfThatLobby()
        {
            var roomName = "My Room!";
            var anon = false;
            var isRanked = false;
            var goal = Goal.Domination;
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = anon,
                Goal = goal,
                MaxPlayers = 5,
                IsRanked = isRanked,
                RoomName = roomName,
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            var roomId = roomResponse.CreatedRoom.RoomId;
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].RoomId);
            Assert.AreEqual(authHelper.getAccountId("userOne"),openLobbiesResponse.Rooms[0].Creator.Id);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Creator.Username);
            Assert.AreEqual(1,openLobbiesResponse.Rooms[0].Players.Count);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Players[0].Username);
        }

        [Test]
        public void IfTheCreatorOfALobbyLeavesTheGameIsDestroyed()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 5,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;
            
            // Have the host leave the lobby
            LeaveRoomRequest leaveRequest = new LeaveRoomRequest()
            {
                RoomId = roomId
            };

            LeaveRoomResponse leaveResponse = client.LeaveRoom(leaveRequest);
            Assert.AreEqual(leaveResponse.Status.IsSuccess, true);
            
            // Ensure that the player has left the game.
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponsAfterLeave = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponsAfterLeave.Status.IsSuccess, true);
            Assert.AreEqual(0,openLobbiesResponsAfterLeave.Rooms.Count);
            
            // Ensure the player is not in the game.
             PlayerCurrentGamesResponse gamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
             Assert.AreEqual(gamesResponse.Status.IsSuccess, true);
            Assert.AreEqual(0,gamesResponse.Games.Count);
        }
        
        [Test]
        public void IfTheCreatorOfALobbyLeavesTheGameNoPlayersAreStuckInTheLobby()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 5,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;
            
            // Have a player join the game
            authHelper.loginToAccount("userTwo");
            client.JoinRoom(new JoinRoomRequest()
            {
                RoomId = roomId,
            });

            authHelper.loginToAccount("userOne");
            // Have the host leave the lobby
            LeaveRoomRequest leaveRequest = new LeaveRoomRequest()
            {
                RoomId = roomId
            };

            LeaveRoomResponse leaveResponse = client.LeaveRoom(leaveRequest);
            Assert.AreEqual(leaveResponse.Status.IsSuccess, true);
            
            // Ensure that the player has left the game.
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponsAfterLeave = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponsAfterLeave.Status.IsSuccess, true);
            Assert.AreEqual(0,openLobbiesResponsAfterLeave.Rooms.Count);
            
            // Ensure the player is not in the game.
            PlayerCurrentGamesResponse gamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            Assert.AreEqual(gamesResponse.Status.IsSuccess, true);
            Assert.AreEqual(0,gamesResponse.Games.Count);
            
            authHelper.loginToAccount("userTwo");
            // Ensure the player is not in the game.
            PlayerCurrentGamesResponse gamesTwoResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            Assert.AreEqual(gamesTwoResponse.Status.IsSuccess, true);
            Assert.AreEqual(0,gamesTwoResponse.Games.Count);
        }

        [Test]
        public void PlayerCanStartAGameEarlyIfTwoPlayersAreInTheLobby()
        {
            CreateRoomResponse roomResponse = authHelper.CreateGameRoom("room1");
            var roomId = roomResponse.CreatedRoom.RoomId;

            authHelper.loginToAccount("userTwo");
            JoinRoomResponse joinResponse = client.JoinRoom(new JoinRoomRequest()
            {
                RoomId = roomId,
            });
            Assert.AreEqual(joinResponse.Status.IsSuccess, true);
            
            
            authHelper.loginToAccount("userOne");
            StartGameEarlyResponse startGameEarlyResponse = client.StartGameEarly(new StartGameEarlyRequest()
            {
                RoomId = roomId,
            });
            
            Assert.AreEqual(startGameEarlyResponse.Status.IsSuccess, true);
            
            // Ensure game cannot be seen in open lobbies.
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(0,openLobbiesResponse.Rooms.Count);
        }

        [Test]
        public void PlayerCannotStartAGameEarlyWithNobodyInTheLobby()
        {
            CreateRoomResponse roomResponse = authHelper.CreateGameRoom("room1");
            var roomId = roomResponse.CreatedRoom.RoomId;
            
            StartGameEarlyResponse startGameEarlyResponse = client.StartGameEarly(new StartGameEarlyRequest()
            {
                RoomId = roomId,
            });
            Assert.AreEqual(startGameEarlyResponse.Status.IsSuccess, false);

            // Ensure game is still open
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
        }

        [Test]
        public void PlayerCannotSeeALobbyThatABlockedPlayerIsIn()
        {
            Assert.IsTrue(false);
        }

        [Test]
        public void PrivateGameRoomsCannotBeSeen()
        {
            Assert.IsTrue(false);
        }

        [Test]
        public void PlayersCanCreatePrivateGameRooms()
        {
            Assert.IsTrue(false);
        }

        [Test]
        public void PlayersCanJoinAPrivateLobbyIfTheyKnowTheLobbyId()
        {
            Assert.IsTrue(false);
        }
        
        [Test]
        public void PlayersWhoRegisterWithTheSameDeviceIdCannotJoinTheSameGame()
        {

            string one = "DeviceOne";
            string two = "DeviceTwo";
            client.RegisterAccount(new AccountRegistrationRequest()
            {
                Username = one,
                Password = one,
                Email = one,
                DeviceIdentifier = one,
            });
            
            client.RegisterAccount(new AccountRegistrationRequest()
            {
                Username = two,
                Password = two,
                Email = two,
                DeviceIdentifier = one,
            });

            CreateRoomResponse createResponse = client.CreateNewRoom(new CreateRoomRequest()
            {
                RoomName = "Room",
                MaxPlayers = 4,
                Goal = Goal.Domination,
            });
            Assert.AreEqual(createResponse.Status.IsSuccess, true);
            var roomId = createResponse.CreatedRoom.RoomId;

            client.Login(new AuthorizationRequest()
            {
                Username = one,
                Password = one,
            });

            var exception = client.JoinRoom(new JoinRoomRequest()
            {
                RoomId = roomId
            });
            
            Assert.AreEqual(exception.Status.IsSuccess, false);
            Assert.AreEqual(exception.Status.Detail, ResponseType.PERMISSION_DENIED.ToString());
        }

        [Test]
        public void AdminsCanViewAnyOngoingGameTheyAreNotIn()
        {
            CreateRoomRequest createRequest = new CreateRoomRequest()
            {
                Anonymous = false,
                Goal = Goal.Domination,
                MaxPlayers = 2,
                IsRanked = false,
                RoomName = "My Room!",
                AllowedSpecialists = { "a","b","c" },
            };

            CreateRoomResponse roomResponse = client.CreateNewRoom(createRequest);
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.RoomId != null);
            var roomId = roomResponse.CreatedRoom.RoomId;

            authHelper.loginToAccount("userTwo");

            JoinRoomRequest joinRequest = new JoinRoomRequest()
            {
                RoomId = roomId,
            };

            JoinRoomResponse joinResponse = client.JoinRoom(joinRequest);
            Assert.AreEqual(joinResponse.Status.IsSuccess, true);
            
            // Check to see the room is not visible.
            OpenLobbiesResponse openLobbiesResponseAfterJoin = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponseAfterJoin.Status.IsSuccess, true);
            Assert.AreEqual(0,openLobbiesResponseAfterJoin.Rooms.Count);
            
            // Check to see the player can see the game because they are a member.
            PlayerCurrentGamesResponse playerGamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            Assert.AreEqual(playerGamesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,playerGamesResponse.Games.Count);

            SuperUser superUser = authHelper.CreateSuperUser();
            client.Login(new AuthorizationRequest()
            {
                Password = superUser.password,
                Username = superUser.userModel.UserModel.Username,
            });
            
            PlayerCurrentGamesResponse adminGamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            Assert.AreEqual(adminGamesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,adminGamesResponse.Games.Count);
        }
    }
}