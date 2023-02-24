using Newtonsoft.Json;
using NUnit.Framework;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Server.Test.util;
using Subterfuge.Remake.Server.Test.util.account;

namespace Subterfuge.Remake.Server.Test.test.game;

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

        gameRoom = (await TestUtils.GetClient().LobbyClient.CreateNewRoom(getCreateRoomRequest())).GetOrThrow();
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        var joinResponse = await TestUtils.GetClient().LobbyClient.JoinRoom(TestUtils.CreateJoinRequest(), gameRoom.GameConfiguration.Id);
        Assert.IsTrue(joinResponse.IsSuccess());
        // Game has begun.
        TestUtils.GetClient().UserApi.SetToken(userOne.Token);
    }

    [Test]
    public async Task PlayerInAGameCanSubmitAnEvent()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));
    }

    [Test]
    public async Task CanSubmitToggleShieldEvent()
    {
        var serializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" });
        
        var toggleSheildResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = serializedEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, toggleSheildResponse.IsSuccess());
        Assert.AreEqual(EventDataType.ToggleShieldEventData, toggleSheildResponse.GetOrThrow().GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, toggleSheildResponse.GetOrThrow().GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(toggleSheildResponse.GetOrThrow().EventId != null);
    }

    [Test]
    public async Task CanSubmitPlayerLeaveGameEvent()
    {
        var serializedEventData = JsonConvert.SerializeObject(new PlayerLeaveGameEventData() { Player = null });
        
        var playerLeaveResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.PlayerLeaveGameEventData,
                SerializedEventData = serializedEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, playerLeaveResponse.IsSuccess());
        Assert.AreEqual(EventDataType.PlayerLeaveGameEventData, playerLeaveResponse.GetOrThrow().GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, playerLeaveResponse.GetOrThrow().GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(playerLeaveResponse.GetOrThrow().EventId != null);
    }

    [Test]
    public async Task CanSubmitDrillMineEvent()
    {
        var serializedEventData = JsonConvert.SerializeObject(new DrillMineEventData() { SourceId = "someOutpostId" });
        
        var drillMineResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.DrillMineEventData,
                SerializedEventData = serializedEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, drillMineResponse.IsSuccess());
        Assert.AreEqual(EventDataType.DrillMineEventData, drillMineResponse.GetOrThrow().GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, drillMineResponse.GetOrThrow().GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(drillMineResponse.GetOrThrow().EventId != null);
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
        
        var launchEventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                SerializedEventData = serializedEventData,
                EventDataType = EventDataType.LaunchEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, launchEventResponse.IsSuccess());
        Assert.AreEqual(EventDataType.LaunchEventData, launchEventResponse.GetOrThrow().GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, launchEventResponse.GetOrThrow().GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(launchEventResponse.GetOrThrow().EventId != null);
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
        
        var launchEventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                SerializedEventData = serializedEventData,
                EventDataType = EventDataType.LaunchEventData,
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, launchEventResponse.IsSuccess());
        Assert.AreEqual(EventDataType.LaunchEventData, launchEventResponse.GetOrThrow().GameRoomEvent.GameEventData.EventDataType);
        Assert.AreEqual(serializedEventData, launchEventResponse.GetOrThrow().GameRoomEvent.GameEventData.SerializedEventData);
        Assert.IsTrue(launchEventResponse.GetOrThrow().EventId != null);
        
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
    }

    [Test]
    public async Task PlayerCannotSubmitEventsToAGameThatDoesNotExist()
    {
        var error = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                },
            }, "InvalidGameRoomId");
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, error.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCannotSubmitEventsToAGameTheyAreNotIn()
    {
        TestUtils.GetClient().UserApi.SetToken(playerNotInGame.Token);
        
        var error = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                },
            }, gameRoom.GameConfiguration.Id);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, error.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCannotSubmitAnEventThatOccursInThePast()
    {
        var error = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = -20,
                },
            }, gameRoom.GameConfiguration.Id);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.VALIDATION_ERROR, error.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCanDeleteAnEventThatTheySubmitted()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));

        var deleteResponse = await TestUtils.GetClient().GameEventClient.DeleteGameEvent(gameRoom.GameConfiguration.Id, eventResponse.GetOrThrow().EventId);
        Assert.AreEqual(true, deleteResponse.ResponseDetail.IsSuccess);
        
        var gameEventsAfterDelete = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEventsAfterDelete.ResponseDetail.IsSuccess);
        Assert.AreEqual(0, gameEventsAfterDelete.GetOrThrow().GameEvents.Count);
    }

    [Test]
    public async Task PlayerCannotDeleteAnotherPlayersEvent()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);

        var error = await TestUtils.GetClient().GameEventClient.DeleteGameEvent(
                gameRoom.GameConfiguration.Id,
                eventResponse.GetOrThrow().EventId);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, error.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCannotDeleteEventsThatHaveAlreadyHappened()
    {
        // The game is configured at 1 tick per second.
        // Event at tick two is two seconds into the game, and we can sleep for 2+ seconds then try to delete.
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 2,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));
        
        // Sleep and wait until the event has passed.
        Thread.Sleep(4000);

        var error = await TestUtils.GetClient().GameEventClient
            .DeleteGameEvent(gameRoom.GameConfiguration.Id, eventResponse.GetOrThrow().EventId);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.VALIDATION_ERROR, error.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCanUpdateAGameEvent()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));

        var update = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            }
        }, gameRoom.GameConfiguration.Id, eventResponse.GetOrThrow().EventId);
        Assert.AreEqual(true, update.ResponseDetail.IsSuccess);
        Assert.IsTrue(update.GetOrThrow().EventId != null);
    }

    [Test]
    public async Task PlayerCannotUpdateAGameEventWithInvalidEventId()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));

        var error = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                }
            }, gameRoom.GameConfiguration.Id, "InvalidEventId");
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, error.ResponseDetail.ResponseType);
    }
    
    [Test]
    public async Task PlayerCannotUpdateAGameEventWithInvalidRoomId()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));

        var error = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 42,
                }
            }, "InvalidRoomId", eventResponse.GetOrThrow().EventId);
        Assert.IsFalse(error.IsSuccess());
        Assert.AreEqual(ResponseType.NOT_FOUND, error.ResponseDetail.ResponseType);
    }

    [Test]
    public async Task PlayerCannotUpdateAGameEventThatHasAlreadyOccurred()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 2,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));
        
        Thread.Sleep(3000);

        var error = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                    OccursAtTick = 2,
                }
            }, gameRoom.GameConfiguration.Id, eventResponse.GetOrThrow().EventId);
        Assert.IsFalse(error.ResponseDetail.IsSuccess);
        Assert.AreEqual(ResponseType.VALIDATION_ERROR, error.ResponseDetail.ResponseType);
    }
    
    [Test]
    public async Task PlayerCannotUpdateAGameToOccurInThePast()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));
        
        Thread.Sleep(3000);

        var update = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 2,
            }
        }, gameRoom.GameConfiguration.Id, eventResponse.GetOrThrow().EventId);
        Assert.AreEqual(true, update.ResponseDetail.IsSuccess);
        Assert.IsTrue(update.GetOrThrow().EventId != null);
    }

    [Test]
    public async Task PlayerCannotUpdateAnotherPlayersEvent()
    {
        var eventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, eventResponse.IsSuccess());
        Assert.IsTrue(eventResponse.GetOrThrow().EventId != null);
            
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(1, gameEvents.GetOrThrow().GameEvents.Count);
        Assert.IsTrue(gameEvents.GetOrThrow().GameEvents.Any(it => it.Id == eventResponse.GetOrThrow().EventId));
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        var response = await TestUtils.GetClient().GameEventClient.UpdateGameEvent(new UpdateGameEventRequest()
        {
            GameEventData = new GameEventData()
            {
                EventDataType = EventDataType.ToggleShieldEventData,
                SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData() { SourceId = "someOutpostId" }),
                OccursAtTick = 2,
            }
        }, gameRoom.GameConfiguration.Id, eventResponse.GetOrThrow().EventId);;
        Assert.IsFalse(response.IsSuccess());
        Assert.AreEqual(ResponseType.PERMISSION_DENIED, response.ResponseDetail.ResponseType);
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
        Assert.AreEqual(true, gameEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(5, gameEvents.GetOrThrow().GameEvents.Count);
        
        TestUtils.GetClient().UserApi.SetToken(userTwo.Token);
        
        Thread.Sleep(5000);
        
        // Other user can see the ones in the past
        var playerTwoEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, playerTwoEvents.ResponseDetail.IsSuccess);
        Assert.AreEqual(3, playerTwoEvents.GetOrThrow().GameEvents.Count);
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
        Assert.IsTrue(roomResponse.ResponseDetail.IsSuccess);
        Assert.AreEqual(5, roomResponse.GetOrThrow().GameEvents.Count);
        
        Thread.Sleep(5000);

        await TestUtils.CreateSuperUserAndLogin();
        var adminRoomResponse = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.IsTrue(adminRoomResponse.ResponseDetail.IsSuccess);
        Assert.AreEqual(5, adminRoomResponse.GetOrThrow().GameEvents.Count);
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
            },
            CreatorSpecialistDeck = new List<SpecialistIds>()
            {
                SpecialistIds.Advisor,
                SpecialistIds.Amnesiac,
                SpecialistIds.Assasin,
                SpecialistIds.Automation,
                SpecialistIds.Dispatcher,
                SpecialistIds.Economist,
                SpecialistIds.Enforcer,
                SpecialistIds.Engineer,
                SpecialistIds.Escort,
                SpecialistIds.Foreman,
                SpecialistIds.Helmsman,
                SpecialistIds.Hypnotist,
                SpecialistIds.Icicle,
                SpecialistIds.Industrialist,
                SpecialistIds.Infiltrator
            }
        };
    }
}