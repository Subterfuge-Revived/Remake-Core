using NUnit.Framework;
using SubterfugeCore.Models.GameEvents;
using SubterfugeRestApiClient;
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
            GameEventRequest = new GameEventRequest()
            {
                EventData = new ToggleShieldEventData() { SourceId = "someOutpostId" },
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
    public async Task CanSubmitEachTypeOfGameEvent()
    {
        SubmitGameEventResponse toggleSheildResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventRequest = new GameEventRequest()
            {
                EventData = new ToggleShieldEventData() { SourceId = "someOutpostId" },
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, toggleSheildResponse.Status.IsSuccess);
        Assert.IsTrue(toggleSheildResponse.EventId != null);
        
        SubmitGameEventResponse playerLeaveResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventRequest = new GameEventRequest()
            {
                EventData = new PlayerLeaveGameEventData() { Player = null },
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, playerLeaveResponse.Status.IsSuccess);
        Assert.IsTrue(playerLeaveResponse.EventId != null);
        
        SubmitGameEventResponse drillMineResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventRequest = new GameEventRequest()
            {
                EventData = new DrillMineEventData() { SourceId = "someOutpostId" },
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, drillMineResponse.Status.IsSuccess);
        Assert.IsTrue(drillMineResponse.EventId != null);
        
        SubmitGameEventResponse launchEventResponse = await TestUtils.GetClient().GameEventClient.SubmitGameEvent(new SubmitGameEventRequest()
        {
            GameEventRequest = new GameEventRequest()
            {
                EventData = new LaunchEventData()
                {
                    SourceId = "someOutpostId",
                    DestinationId = "SomeDestination",
                    DrillerCount = 10,
                    SpecialistIds = new List<string>(),
                },
                OccursAtTick = 42,
            },
        }, gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, launchEventResponse.Status.IsSuccess);
        Assert.IsTrue(launchEventResponse.EventId != null);
        
        // Submitting player can see their own events
        var gameEvents = await TestUtils.GetClient().GameEventClient.GetGameRoomEvents(gameRoom.GameConfiguration.Id);
        Assert.AreEqual(true, gameEvents.Status.IsSuccess);
        Assert.AreEqual(4, gameEvents.GameEvents.Count);
    }

    [Test]
    public void PlayerCannotSubmitEventsToAGameThatDoesNotExist()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSubmitEventsToAGameTheyAreNotIn()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotSubmitAnEventThatOccursInThePast()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanDeleteAnEventThatTheySubmitted()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotDeleteAnotherPlayersEvent()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotDeleteEventsThatHaveAlreadyHappened()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanUpdateAGameEvent()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotUpdateAGameEventWithInvalidEventId()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotUpdateAGameEventThatHasAlreadyOccurred()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCannotUpdateAnotherPlayersEvent()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayersCanViewAnyEventThatHasAlreadyOccurred()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void PlayerCanViewTheirOwnEventsThatOccurInTheFutureButOthersCannot()
    {
        throw new NotImplementedException();
    }

    [Test]
    public void AdminsCanSeeAllGameEvents()
    {
        throw new NotImplementedException();
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