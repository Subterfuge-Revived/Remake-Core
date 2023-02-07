using Newtonsoft.Json;
using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
using SubterfugeRestApiClient.controllers.exception;
using SubterfugeServerConsole.Connections;

namespace SubterfugeRestApiServerTest.test.game;

public class GameEventControllerTest
{
    private AccountRegistrationResponse userOne;
    private AccountRegistrationResponse userTwo;
    private AccountRegistrationResponse playerNotInGame;

    private CreateRoomResponse gameRoom;

    [SetUp]
    public async Task Setup()
    {
        TestUtils.Mongo.FlushAll();
        TestUtils.GetClient().UserApi.Logout();
        
        playerNotInGame = await AccountUtils.AssertRegisterAccountAndAuthorized("PlayerNotInGame");
        userTwo = await AccountUtils.AssertRegisterAccountAndAuthorized("UserTwo");
        userOne = await AccountUtils.AssertRegisterAccountAndAuthorized("UserOne");

        gameRoom = await TestUtils.GetClient().LobbyClient.CreateNewRoom(getCreateRoomRequest());
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        await TestUtils.GetClient().LobbyClient.JoinRoom(new JoinRoomRequest(), gameRoom.GameConfiguration.Id);
        // Game has begun.
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
    }

    [Test]
    public async Task PlayerInAGameCanSubmitAnEvent()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));
    }

    [Test]
    public async Task CanSubmitToggleShieldEvent()
    {
        var serializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" });
        
        SubmitGameEventResponse toggleSheildResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = serializedEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, toggleSheildResponse.Status.IsSuccess);
        Assert.AreEqual(EventDataType.ToggleShieldEventData, toggleSheildResponse.GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, toggleSheildResponse.GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(toggleSheildResponse.EventId != null);
    }

    [Test]
    public async Task CanSubmitPlayerLeaveGameEvent()
    {
        var serializedEventData = JsonConvert.SerializeObject(new PlayerLeaveGameEventData() { Player = null });
        
        SubmitGameEventResponse playerLeaveResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.PlayerLeaveGameEventData,
                SerializedEventData = serializedEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, playerLeaveResponse.Status.IsSuccess);
        Assert.AreEqual(EventDataType.PlayerLeaveGameEventData, playerLeaveResponse.GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, playerLeaveResponse.GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(playerLeaveResponse.EventId != null);
    }

    [Test]
    public async Task CanSubmitDrillMineEvent()
    {
        var serializedEventData = JsonConvert.SerializeObject(new DrillMineEventData() { SourceId = "someOutpostId" });
        
        SubmitGameEventResponse drillMineResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.DrillMineEventData,
                SerializedEventData = serializedEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, drillMineResponse.Status.IsSuccess);
        Assert.AreEqual(EventDataType.DrillMineEventData, drillMineResponse.GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, drillMineResponse.GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(drillMineResponse.EventId != null);
    }

    [Test]
    public async Task CanSubmitLaunchEvent()
    {
        var serializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
        {
            SourceId = "someOutpostId",
            DestinationId = "SomeDestination",
            DrillerCount = 10,
            SpecialistIds = new List<string>(),
        });
        
        SubmitGameEventResponse launchEventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                SerializedEventData = serializedEventData,
                EventDataType = EventDataType.LaunchEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, launchEventResponse.Status.IsSuccess);
        Assert.AreEqual(EventDataType.LaunchEventData, launchEventResponse.GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, launchEventResponse.GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(launchEventResponse.EventId != null);
    }
    
    [Test]
    public async Task PlayerCanViewTheirOwnFutureEvents()
    {
        var serializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
        {
            SourceId = "someOutpostId",
            DestinationId = "SomeDestination",
            DrillerCount = 10,
            SpecialistIds = new List<string>(),
        });
        
        SubmitGameEventResponse launchEventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                SerializedEventData = serializedEventData,
                EventDataType = EventDataType.LaunchEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, launchEventResponse.Status.IsSuccess);
        Assert.AreEqual(EventDataType.LaunchEventData, launchEventResponse.GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, launchEventResponse.GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(launchEventResponse.EventId != null);
        
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
    }

    [Test]
    public async Task PlayerCannotSubmitEventsToAGameThatDoesNotExist()
    {
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                },
            }, "InvalidGameRoomId");
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCannotSubmitEventsToAGameTheyAreNotIn()
    {
        TestUtils.GetClient().UserApi.SetToken(playerNotInGame.Token);
        
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                },
            }, gameRoom.GameConfiguration.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCannotSubmitAnEventThatOccursInThePast()
    {
        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = -20,
                },
            }, gameRoom.GameConfiguration.Id);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCanDeleteAnEventThatTheySubmitted()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));

        var deleteResponse = await TestUtils.GetClient().GameEventClient.DeleteGameEvent(gameRoom.GameConfiguration.Id, eventResponse.EventId);
        Assert.AreEqual(true, deleteResponse.Status.IsSuccess);
        
        var gameEventsAfterDelete = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEventsAfterDelete.Status.IsSuccess);
        Assert.AreEqual(0, gameEventsAfterDelete.GameEvents.Count);
    }

    [Test]
    public async Task PlayerCannotDeleteAnotherPlayersEvent()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.DeleteGameEvent(
                gameRoom.GameConfiguration.Id,
                eventResponse.EventId);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCannotDeleteEventsThatHaveAlreadyHappened()
    {
        // The game is configured at 1 tick per second.
        // Event at tick two is two seconds into the game, and we can sleep for 2+ seconds then try to delete.
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 2,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));
        
        // Sleep and wait until the event has passed.
        Thread.Sleep(4000);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient
                .DeleteGameEvent(gameRoom.GameConfiguration.Id, eventResponse.EventId);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCanUpdateAGameEvent()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));

        var update = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            }
        }, gameRoom.GameConfiguration.Id, eventResponse.EventId);
        Assert.AreEqual(true, update.Status.IsSuccess);
        Assert.IsTrue(update.EventId != null);
    }

    [Test]
    public async Task PlayerCannotUpdateAGameEventWithInvalidEventId()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                }
            }, gameRoom.GameConfiguration.Id, "InvalidEventId");
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
    }
    
    [Test]
    public async Task PlayerCannotUpdateAGameEventWithInvalidRoomId()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                }
            }, "InvalidRoomId", eventResponse.EventId);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.NOT_FOUND, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayerCannotUpdateAGameEventThatHasAlreadyOccurred()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 2,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));
        
        Thread.Sleep(3000);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 2,
                }
            }, gameRoom.GameConfiguration.Id, eventResponse.EventId);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }
    
    [Test]
    public async Task PlayerCannotUpdateAGameToOccurInThePast()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));
        
        Thread.Sleep(3000);

        var update = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 2,
            }
        }, gameRoom.GameConfiguration.Id, eventResponse.EventId);
        Assert.AreEqual(true, update.Status.IsSuccess);
        Assert.IsTrue(update.EventId != null);
    }

    [Test]
    public async Task PlayerCannotUpdateAnotherPlayersEvent()
    {
        SubmitGameEventResponse eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.Status.IsSuccess);
        Assert.IsTrue(eventResponse.EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(1, gameEvents.GameEvents.Count);
        Assert.IsTrue(gameEvents.GameEvents.Any(it => it.Id == eventResponse.EventId));
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var exception = Assert.ThrowsAsync<SubterfugeClientException>(async () =>
        {
            await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 2,
                }
            }, gameRoom.GameConfiguration.Id, eventResponse.EventId);
        });
        Assert.IsFalse(exception.response.Status.IsSuccess);
        Assert.AreEqual(ResponseType.INVALID_REQUEST, exception.response.Status.ResponseType);
    }

    [Test]
    public async Task PlayersCanViewPastEventsButOnlyTheirOwnFutureEvents()
    {
        var request = new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 3,
            },
        };
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);

        request.GameEventData.OccursAtTick = 1323;
        
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        
            
        // Submitting player can see their own events in the future
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(5, gameEvents.GameEvents.Count);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        Thread.Sleep(5000);
        
        // Other user can see the ones in the past
        var playerTwoEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, playerTwoEvents.Status.IsSuccess);
        Assert.AreEqual(3, playerTwoEvents.GameEvents.Count);
    }

    [Test]
    public async Task AdminsCanSeeAllGameEvents()
    {
        var request = new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 3,
            },
        };


        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);

        request.GameEventData.OccursAtTick = 1323;
        
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        await TestUtils.GetClient().GameEventClient.SubmitGameEvent(request, gameRoom.GameConfiguration.Id);
        
        var roomResponse = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.IsTrue(roomResponse.Status.IsSuccess);
        Assert.AreEqual(5, roomResponse.GameEvents.Count);
        
        Thread.Sleep(5000);

        await TestUtils.CreateSuperUserAndLogin();
        var adminRoomResponse = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.IsTrue(adminRoomResponse.Status.IsSuccess);
        Assert.AreEqual(5, adminRoomResponse.GameEvents.Count);
    }

    private CreateRoomRequest getCreateRoomRequest()
    {
        return new CreateRoomRequest()
        {
            GameSettings = new GameSettings()
            {
                IsAnonymous = false,
                Goal = Goal.Domination,
                IsRanked = false,
                MaxPlayers = 2,
                MinutesPerTick = (1.0 / 60.0), // One second per tick
            },
            RoomName = "TestRoom",
            MapConfiguration = new MapConfiguration()
            {
                Seed = 123123,
                OutpostsPerPlayer = 3,
                MinimumOutpostDistance = 100,
                MaximumOutpostDistance = 1200,
                DormantsPerPlayer = 3,
                OutpostDistribution = new OutpostDistribution()
                {
                    FactoryWeight = 0.33f,
                    GeneratorWeight = 0.33f,
                    WatchtowerWeight = 0.33f,
                }
            }
        };
    }
}