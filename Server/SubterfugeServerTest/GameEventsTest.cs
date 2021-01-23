using System.Linq;
using System.Threading;
using Grpc.Core;
using NUnit.Framework;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;
using SubterfugeServerConsole.Connections;
using Tests.AuthTestingHelper;

namespace Tests
{
    public class GameEventsTest
    {
        SubterfugeClient.SubterfugeClient client;
        private AuthTestHelper authHelper;

        private string playerOneInGame = "playerOneInGame";
        private string playerTwoInGame = "playerTwoInGame";
        private string playerOutOfGame = "playerOutOfGame";
        private string gameId;
        
        [SetUp]
        public void Setup()
        {
            client = ClientHelper.GetClient();
            
            // Clear the database every test.
            RedisConnector.Server.FlushDatabase();
            
            // Create two new user accounts.
            authHelper = new AuthTestHelper(client);
            authHelper.createAccount(playerOneInGame);
            authHelper.createAccount(playerTwoInGame);
            authHelper.createAccount(playerOutOfGame);
            authHelper.loginToAccount(playerOneInGame);
            gameId = client.CreateNewRoom(new CreateRoomRequest()
            {
                MaxPlayers = 2,
                RoomName = "TestRoom",
                MinutesPerTick = (1.0/60.0), // One second per tick
            }).CreatedRoom.RoomId;
            authHelper.loginToAccount(playerTwoInGame);
            client.JoinRoom(new JoinRoomRequest()
            {
                RoomId = gameId
            });
            
            // Game has begun.
            authHelper.loginToAccount(playerOneInGame);
        }

        [Test]
        public void PlayerInAGameCanSubmitAnEvent()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            // Submitting player can see their own events
            GetGameRoomEventsResponse gameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEvents.GameEvents.Count);
            Assert.IsTrue(gameEvents.GameEvents.Any(it => it.EventId == eventResponse.EventId));
        }

        [Test]
        public void PlayerInAGameCannotSubmitAnEventForAnotherPlayer()
        {
            var exception = Assert.Throws<RpcException>(() => client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerTwoInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            }));
            Assert.IsTrue(exception != null);
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.Unauthenticated);
        }

        [Test]
        public void PlayerCannotSubmitEventsToAGameThatDoesNotExist()
        {
            var exception = Assert.Throws<RpcException>(() => client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = "somegameId",
            }));
            Assert.IsTrue(exception != null);
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.NotFound);
        }

        [Test]
        public void PlayerCannotSubmitEventsToAGameTheyAreNotIn()
        {
            authHelper.loginToAccount(playerOutOfGame);
            var exception = Assert.Throws<RpcException>(() => client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOutOfGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            }));
            Assert.IsTrue(exception != null);
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.Unauthenticated);
        }

        [Test]
        public void PlayerCannotSubmitAnEventThatOccursInThePast()
        {
            SubmitGameEventResponse submitResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = -20,
                },
                RoomId = gameId,
            });
            Assert.IsFalse(submitResponse.Success);
        }

        [Test]
        public void PlayerCanDeleteAnEventThatTheySubmitted()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            GetGameRoomEventsResponse gameEventsBeforeDelete = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEventsBeforeDelete.GameEvents.Count);
            Assert.IsTrue(gameEventsBeforeDelete.GameEvents.Any(it => it.EventId == eventResponse.EventId));

            DeleteGameEventResponse deleteResponse = client.DeleteGameEvent(new DeleteGameEventRequest()
            {
                EventId = eventResponse.EventId,
                RoomId = gameId,
            });
            Assert.IsTrue(deleteResponse != null);
            Assert.IsTrue(deleteResponse.Success);

            GetGameRoomEventsResponse gameEventsAfterDelete = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(0, gameEventsAfterDelete.GameEvents.Count);
        }

        [Test]
        public void PlayerCannotDeleteAnotherPlayersEvent()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            GetGameRoomEventsResponse gameEventsBeforeDelete = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEventsBeforeDelete.GameEvents.Count);
            Assert.IsTrue(gameEventsBeforeDelete.GameEvents.Any(it => it.EventId == eventResponse.EventId));

            authHelper.loginToAccount(playerTwoInGame);

            DeleteGameEventResponse deleteResponse = client.DeleteGameEvent(new DeleteGameEventRequest()
            {
                EventId = eventResponse.EventId,
                RoomId = gameId,
            });
            Assert.IsFalse(deleteResponse.Success);
            
            // Login to player 1 to see the event in the future.
            authHelper.loginToAccount(playerOneInGame);
            GetGameRoomEventsResponse gameEventsAfterDelete = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEventsAfterDelete.GameEvents.Count);
        }

        [Test]
        public void PlayerCannotDeleteEventsThatHaveAlreadyHappened()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 2,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            // Submitting player can see their own events
            GetGameRoomEventsResponse gameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEvents.GameEvents.Count);
            Assert.IsTrue(gameEvents.GameEvents.Any(it => it.EventId == eventResponse.EventId));

            Thread.Sleep(3000);
            
            // Attempt to delete
            DeleteGameEventResponse deleteResponse = client.DeleteGameEvent(new DeleteGameEventRequest()
            {
                EventId = eventResponse.EventId,
                RoomId = gameId,
            });
            Assert.IsTrue(deleteResponse != null);
            Assert.IsFalse(deleteResponse.Success);
        }

        [Test]
        public void PlayerCanUpdateAGameEvent()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            GetGameRoomEventsResponse gameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEvents.GameEvents.Count);
            Assert.IsTrue(gameEvents.GameEvents.Any(it => it.EventId == eventResponse.EventId));
            
            SubmitGameEventResponse updateResponse = client.UpdateGameEvent(new UpdateGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventId = eventResponse.EventId,
                    EventData = "NewEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(updateResponse.Success);
            Assert.IsTrue(updateResponse.EventId == eventResponse.EventId);
        }

        [Test]
        public void PlayerCannotUpdateAGameEventWithInvalidEventId()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            GetGameRoomEventsResponse gameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEvents.GameEvents.Count);
            Assert.IsTrue(gameEvents.GameEvents.Any(it => it.EventId == eventResponse.EventId));
            
            var exception = Assert.Throws<RpcException>(() => client.UpdateGameEvent(new UpdateGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventId = "14141414",
                    EventData = "NewEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            }));
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.InvalidArgument);
        }

        [Test]
        public void PlayerCannotUpdateAGameEventThatHasAlreadyOccurred()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 2,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            // Submitting player can see their own events
            GetGameRoomEventsResponse gameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEvents.GameEvents.Count);
            Assert.IsTrue(gameEvents.GameEvents.Any(it => it.EventId == eventResponse.EventId));
            
            Thread.Sleep(3000);
            
            // Attempt to delete
            SubmitGameEventResponse updateResponse = client.UpdateGameEvent(new UpdateGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventId = eventResponse.EventId,
                    EventData = "NewEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(updateResponse != null);
            Assert.IsTrue(updateResponse.Success);
        }

        [Test]
        public void PlayerCannotUpdateAnotherPlayersEvent()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            GetGameRoomEventsResponse gameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEvents.GameEvents.Count);
            Assert.IsTrue(gameEvents.GameEvents.Any(it => it.EventId == eventResponse.EventId));

            authHelper.loginToAccount(playerTwoInGame);
            
            var exception = Assert.Throws<RpcException>(() => client.UpdateGameEvent(new UpdateGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventId = eventResponse.EventId,
                    EventData = "NewEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            }));
            Assert.AreEqual(exception.Status.StatusCode, StatusCode.Unauthenticated);
        }

        [Test]
        public void PlayersCanViewAnyEventThatHasAlreadyOccurred()
        {
            // Submit 3 close game events, and 2 far game events.
            SubmitGameEventRequest request = new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 5,
                },
                RoomId = gameId,
            };
            client.SubmitGameEvent(request);
            client.SubmitGameEvent(request);
            client.SubmitGameEvent(request);

            request.EventData.OccursAtTick = 10467;
            
            client.SubmitGameEvent(request);
            client.SubmitGameEvent(request);
            
            Thread.Sleep(5000);
            
            // Submitting player can see their own events
            GetGameRoomEventsResponse gameEventsForSubmitter = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(5, gameEventsForSubmitter.GameEvents.Count);

            authHelper.loginToAccount(playerTwoInGame);
            // Other player can only see events that have passed.
            GetGameRoomEventsResponse gameEventsForOther = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(3, gameEventsForOther.GameEvents.Count);
        }
        
        [Test]
        public void PlayerCanViewTheirOwnEventsThatOccurInTheFutureButOthersCannot()
        {
            SubmitGameEventResponse eventResponse = client.SubmitGameEvent(new SubmitGameEventRequest()
            {
                EventData = new GameEvent()
                {
                    EventData = "MyEventData",
                    IssuedBy = authHelper.getAccountId(playerOneInGame),
                    OccursAtTick = 42,
                },
                RoomId = gameId,
            });
            Assert.IsTrue(eventResponse.Success);
            Assert.IsTrue(eventResponse.EventId != null);
            
            // Submitting player can see their own events
            GetGameRoomEventsResponse gameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(1, gameEvents.GameEvents.Count);
            Assert.IsTrue(gameEvents.GameEvents.Any(it => it.EventId == eventResponse.EventId));

            authHelper.loginToAccount(playerTwoInGame);
            // Other player cannot see the first player's plans as they are in the future
            GetGameRoomEventsResponse playerTwoGameEvents = client.GetGameRoomEvents(new GetGameRoomEventsRequest()
            {
                RoomId = gameId
            });
            Assert.AreEqual(0, playerTwoGameEvents.GameEvents.Count);
        }
    }
}