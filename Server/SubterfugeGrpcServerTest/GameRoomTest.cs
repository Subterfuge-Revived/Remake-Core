using System;
using System.Collections.Generic;
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
            MongoConnector.FlushCollections();
            
            
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
            var roomId = createRoom();

            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].Id);
            Assert.AreEqual(authHelper.getAccountId("userOne"),openLobbiesResponse.Rooms[0].Creator.Id);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Creator.Username);
            Assert.AreEqual("My room!",openLobbiesResponse.Rooms[0].RoomName);
            Assert.AreEqual(RoomStatus.Open,openLobbiesResponse.Rooms[0].RoomStatus);
            Assert.AreEqual(false,openLobbiesResponse.Rooms[0].GameSettings.Anonymous);
            Assert.AreEqual(Goal.Domination,openLobbiesResponse.Rooms[0].GameSettings.Goal);
            Assert.AreEqual(1,openLobbiesResponse.Rooms[0].Players.Count);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Players[0].Username);
        }

        [Test]
        public void PlayerCanJoinAGameRoom()
        {
            var roomId = createRoom();

            authHelper.loginToAccount("userTwo");
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].Id);
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
            Assert.AreEqual(roomId,openLobbiesResponseAfterJoin.Rooms[0].Id);
            Assert.AreEqual(2,openLobbiesResponseAfterJoin.Rooms[0].Players.Count);
            Assert.IsTrue(openLobbiesResponseAfterJoin.Rooms[0].Players.Any(it => it.Id == authHelper.getAccountId("userTwo")));
        }

        [Test]
        public void PlayerCannotJoinTheSameGameTwice()
        {
            CreateRoomResponse roomResponse = client.CreateNewRoom(createRoomRequest("My room!"));
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.Id != null);
            var roomId = roomResponse.CreatedRoom.Id;

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
            CreateRoomResponse roomResponse = client.CreateNewRoom(createRoomRequest("My room!", maxPlayers: 2));
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.Id != null);
            var roomId = roomResponse.CreatedRoom.Id;

            authHelper.loginToAccount("userTwo");

            JoinRoomRequest joinRequest = new JoinRoomRequest()
            {
                RoomId = roomId,
            };

            JoinRoomResponse joinResponse = client.JoinRoom(joinRequest);
            assertSuccessResponse(joinResponse.Status);

            authHelper.loginToAccount("userThree");
            var exception = client.JoinRoom(joinRequest);
            assertResponseFailure(exception.Status, ResponseType.ROOM_IS_FULL);
        }
        
        [Test]
        public void BeingTheLastPlayerToJoinAGameWillStartTheGame()
        {
            CreateRoomResponse roomResponse = client.CreateNewRoom(createRoomRequest("My room!", maxPlayers: 2));
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.Id != null);
            var roomId = roomResponse.CreatedRoom.Id;

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
            CreateRoomResponse roomResponse = client.CreateNewRoom(createRoomRequest("My room!"));
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.Id != null);
            var roomId = roomResponse.CreatedRoom.Id;

            authHelper.loginToAccount("userTwo");
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].Id);
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
            Assert.AreEqual(roomId,openLobbiesResponsAfterJoin.Rooms[0].Id);
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
            Assert.AreEqual(roomId,openLobbiesResponsAfterLeave.Rooms[0].Id);
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
            var roomId = createRoom();
            
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
            Assert.AreEqual(roomId,openLobbiesResponse.Rooms[0].Id);
            Assert.AreEqual(authHelper.getAccountId("userOne"),openLobbiesResponse.Rooms[0].Creator.Id);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Creator.Username);
            Assert.AreEqual(1,openLobbiesResponse.Rooms[0].Players.Count);
            Assert.AreEqual("userOne",openLobbiesResponse.Rooms[0].Players[0].Username);
        }

        [Test]
        public void IfTheCreatorOfALobbyLeavesTheGameIsDestroyed()
        {
            var roomId = createRoom();
            
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
            assertSuccessResponse(openLobbiesResponsAfterLeave.Status);
            Assert.AreEqual(0,openLobbiesResponsAfterLeave.Rooms.Count);
            
            // Ensure the player is not in the game.
             PlayerCurrentGamesResponse gamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
             assertSuccessResponse(gamesResponse.Status);
            Assert.AreEqual(0,gamesResponse.Games.Count);
        }
        
        [Test]
        public void IfTheCreatorOfALobbyLeavesTheGameNoPlayersAreStuckInTheLobby()
        {
            var roomId = createRoom();
            
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
            assertSuccessResponse(leaveResponse.Status);
            
            // Ensure that the player has left the game.
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponsAfterLeave = client.GetOpenLobbies(new OpenLobbiesRequest());
            assertSuccessResponse(openLobbiesResponsAfterLeave.Status);
            Assert.AreEqual(0,openLobbiesResponsAfterLeave.Rooms.Count);
            
            // Ensure the player is not in the game.
            PlayerCurrentGamesResponse gamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            assertSuccessResponse(gamesResponse.Status);
            Assert.AreEqual(0,gamesResponse.Games.Count);
            
            authHelper.loginToAccount("userTwo");
            // Ensure the player is not in the game.
            PlayerCurrentGamesResponse gamesTwoResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            assertSuccessResponse(gamesTwoResponse.Status);
            Assert.AreEqual(0,gamesTwoResponse.Games.Count);
        }

        [Test]
        public void PlayerCanStartAGameEarlyIfTwoPlayersAreInTheLobby()
        {
            var roomId = createRoom();

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
            assertSuccessResponse(openLobbiesResponse.Status);
            Assert.AreEqual(0,openLobbiesResponse.Rooms.Count);
        }

        [Test]
        public void PlayerCannotStartAGameEarlyWithNobodyInTheLobby()
        {
            var roomId = createRoom();
            
            StartGameEarlyResponse startGameEarlyResponse = client.StartGameEarly(new StartGameEarlyRequest()
            {
                RoomId = roomId,
            });
            Assert.AreEqual(startGameEarlyResponse.Status.IsSuccess, false);

            // Ensure game is still open
            // View open rooms.
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            assertSuccessResponse(openLobbiesResponse.Status);
            Assert.AreEqual(1,openLobbiesResponse.Rooms.Count);
        }

        [Ignore("Not implemented")]
        [Test]
        public void PlayerCannotSeeALobbyThatABlockedPlayerIsIn()
        {
            Assert.IsTrue(false);
        }

        [Ignore("Not implemented")]
        [Test]
        public void PrivateGameRoomsCannotBeSeen()
        {
            Assert.IsTrue(false);
        }

        [Ignore("Not implemented")]
        [Test]
        public void PlayersCanCreatePrivateGameRooms()
        {
            Assert.IsTrue(false);
        }

        [Ignore("Not implemented")]
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

            var roomId = createRoom(maxPlayers: 4);

            client.Login(new AuthorizationRequest()
            {
                Username = one,
                Password = one,
            });

            var exception = client.JoinRoom(new JoinRoomRequest()
            {
                RoomId = roomId
            });
            
            assertResponseFailure(exception.Status, ResponseType.PERMISSION_DENIED);
        }

        [Test]
        public void AdminsCanViewAnyOngoingGameTheyAreNotIn()
        {
            var roomId = createRoom(maxPlayers: 2);

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
                Username = superUser.DbUserModel.UserModel.Username,
            });
            
            PlayerCurrentGamesResponse adminGamesResponse = client.GetPlayerCurrentGames(new PlayerCurrentGamesRequest());
            Assert.AreEqual(adminGamesResponse.Status.IsSuccess, true);
            Assert.AreEqual(1,adminGamesResponse.Games.Count);
        }
        
        private void assertResponseFailure(ResponseStatus response, ResponseType expectedError)
        {
            Assert.AreEqual(response.IsSuccess, false);
            Assert.AreEqual(response.Detail, expectedError.ToString());
        }

        private void assertSuccessResponse(ResponseStatus response)
        {
            Assert.AreEqual(true, response.IsSuccess);
            Assert.AreEqual(ResponseType.SUCCESS.ToString(), response.Detail);
        }
        
        private MapConfiguration createMapConfiguration()
        {
            return new MapConfiguration()
            {
                DormantsPerPlayer = 5,
                MaximumOutpostDistance = 100,
                MinimumOutpostDistance = 5,
                OutpostDistribution = new OutpostWeighting()
                {
                    FactoryWeight = 0.33f, 
                    GeneratorWeight = 0.33f,
                    WatchtowerWeight = 0.33f,
                },
                OutpostsPerPlayer = 3,
                Seed = 123123,
            };
        }

        private CreateRoomRequest createRoomRequest(string roomName, bool anon = false, bool isRanked = false, Goal goal = Goal.Domination, int maxPlayers = 5)
        {
            return new CreateRoomRequest()
            {
                GameSettings = new GameSettings()
                {
                    Anonymous = anon,
                    Goal = goal,
                    IsRanked = isRanked,
                    MaxPlayers = maxPlayers,
                    MinutesPerTick = 15,
                    AllowedSpecialists = { }
                },
                IsPrivate = false,
                MapConfiguration = createMapConfiguration(),
                RoomName = roomName,
            };
        }

        private string createRoom(int maxPlayers = 5)
        {
            CreateRoomResponse roomResponse = client.CreateNewRoom(createRoomRequest("My room!", maxPlayers: maxPlayers));
            Assert.AreEqual(roomResponse.Status.IsSuccess, true);
            Assert.IsTrue(roomResponse.CreatedRoom.Id != null);
            return roomResponse.CreatedRoom.Id;
        }

        private void assertOpenLobbies(List<string> lobbyIds)
        {
            OpenLobbiesResponse openLobbiesResponse = client.GetOpenLobbies(new OpenLobbiesRequest());
            Assert.AreEqual(openLobbiesResponse.Status.IsSuccess, true);
            Assert.AreEqual(lobbyIds.Count,openLobbiesResponse.Rooms.Count);
            Assert.True(openLobbiesResponse.Rooms.All(it => lobbyIds.Contains(it.Id)));
        }
    }
}