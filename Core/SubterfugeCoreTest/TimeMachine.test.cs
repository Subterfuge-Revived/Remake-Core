using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.GameEvents.SpecialistEvents;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;
using PositionalGameEvent = Subterfuge.Remake.Core.GameEvents.Combat.PositionalGameEvent;

namespace Subterfuge.Remake.Test
{
    [TestClass]
    public class TimeMachineTest
    {
        TestUtils testUtils = new TestUtils();
        Player player = new Player(new SimpleUser() { Id = "1" });
        private Game _game;
        
        [TestInitialize]
        public void Setup()
        {
            _game = new Game(testUtils.GetDefaultGameConfiguration(new List<Player> { player }));

        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(_game.TimeMachine);
            Assert.IsNotNull(_game.TimeMachine.GetState());
        }

        [TestMethod]
        public void AddEvent()
        {
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player, _game.TimeMachine);
            outpost.GetComponent<DrillerCarrier>().AlterDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(10), 10, player, _game.TimeMachine);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(10));

            _game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
            Assert.AreEqual(arriveEvent, _game.TimeMachine.GetFutureEventsOf<CombatEvent>()[0]);
        }

        [TestMethod]
        public void CanAdvanceTime()
        {
            GameTick initialTick = _game.TimeMachine.GetCurrentTick();
            _game.TimeMachine.Advance(5);
            
            Assert.IsTrue(initialTick < _game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), _game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void CanRewindTime()
        {
            GameTick initialTick = _game.TimeMachine.GetCurrentTick();
            _game.TimeMachine.Advance(5);
            
            Assert.IsTrue(initialTick < _game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), _game.TimeMachine.GetCurrentTick().GetTick());

            GameTick advancedTick = _game.TimeMachine.GetCurrentTick();
            
            _game.TimeMachine.Rewind(1);
            
            
            Assert.IsTrue(advancedTick > _game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(advancedTick.Rewind(1).GetTick(), _game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void EventsSwitchQueuesWhenPassedForward()
        {
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player, _game.TimeMachine);
            outpost.GetComponent<DrillerCarrier>().AlterDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player, _game.TimeMachine);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            _game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
            Assert.AreEqual(arriveEvent, _game.TimeMachine.GetFutureEventsOf<CombatEvent>()[0]);
            
            // Go past the tick
            _game.TimeMachine.Advance(6);
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
        }

        [TestMethod]
        public void NaturalEventsAreRemovedWhenRewind()
        {
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player, _game.TimeMachine);
            outpost.GetComponent<DrillerCarrier>().AlterDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player, _game.TimeMachine);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            _game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
            Assert.AreEqual(arriveEvent, _game.TimeMachine.GetFutureEventsOf<CombatEvent>()[0]);
            
            // Go past the tick
            _game.TimeMachine.Advance(6);
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
            
            // Rewind back
            _game.TimeMachine.Rewind(6);
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
        }
        
        [TestMethod]
        public void PlayerEventsAreNotRemovedWhenRewind()
        {
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player, _game.TimeMachine);
            _game.TimeMachine.GetState().AddOutpost(outpost);
            var toggleShield = new ToggleShieldEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.ToggleShieldEventData,
                    OccursAtTick = 10,
                    SerializedEventData = JsonConvert.SerializeObject(new ToggleShieldEventData()
                    {
                        SourceId = outpost.GetComponent<IdentityManager>().GetId()
                    })
                },
                Id = "a",
                IssuedBy = new SimpleUser(),
                RoomId = "1",
                TimeIssued = DateTime.UtcNow,
            });

            _game.TimeMachine.AddEvent(toggleShield);
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<ToggleShieldEvent>().Count);
            Assert.AreEqual(toggleShield, _game.TimeMachine.GetFutureEventsOf<ToggleShieldEvent>()[0]);
            
            // Go past the tick
            _game.TimeMachine.GoTo(toggleShield.OccursAt);
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<ToggleShieldEvent>().Count);
            Assert.IsFalse(outpost.GetComponent<ShieldManager>().IsShieldActive());
            
            // Rewind back
            _game.TimeMachine.GoTo(new GameTick(0));
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<ToggleShieldEvent>().Count);
            Assert.IsTrue(outpost.GetComponent<ShieldManager>().IsShieldActive());
            
            // Go past the tick
            _game.TimeMachine.GoTo(toggleShield.OccursAt);
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<ToggleShieldEvent>().Count);
            Assert.IsFalse(outpost.GetComponent<ShieldManager>().IsShieldActive());
            
            // Rewind back
            _game.TimeMachine.GoTo(new GameTick(0));
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<ToggleShieldEvent>().Count);
            Assert.IsTrue(outpost.GetComponent<ShieldManager>().IsShieldActive());
        }
        
        [TestMethod]
        public void CanRemoveEvents()
        {
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player, _game.TimeMachine);
            outpost.GetComponent<DrillerCarrier>().AlterDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player, _game.TimeMachine);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            _game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
            Assert.AreEqual(arriveEvent, _game.TimeMachine.GetFutureEventsOf<CombatEvent>()[0]);
            
            _game.TimeMachine.RemoveEvent(arriveEvent);
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
        }
        
        [TestMethod]
        public void CanGoToAGameTick()
        {

            GameTick initialTick = _game.TimeMachine.GetCurrentTick();
            _game.TimeMachine.GoTo(new GameTick(5));
            
            Assert.IsTrue(initialTick < _game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), _game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void CanGoToAnEvent()
        {
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player, _game.TimeMachine);
            outpost.GetComponent<DrillerCarrier>().AlterDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player, _game.TimeMachine);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            _game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
            Assert.AreEqual(arriveEvent, _game.TimeMachine.GetFutureEventsOf<CombatEvent>()[0]);
            
            _game.TimeMachine.GoTo(arriveEvent);
            Assert.AreEqual(arriveEvent.OccursAt.GetTick(), _game.TimeMachine.GetCurrentTick().GetTick());
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
        }

        [TestMethod]
        public void CreatingTwoOfTheSameEventWillNotAddThemBoth()
        {
            // Simulate two events being published.
            var eventOne = new NoOpGameEvent(new GameTick(42));
            var eventTwo = new NoOpGameEvent(new GameTick(42));
            
            _game.TimeMachine.AddEvents(new List<GameEvent>() { eventOne, eventTwo });
            // Ensure only one event got added.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
        }

        [TestMethod]
        public void PausingTheGamePreventsActionsFromOccuringUntilTheGameIsUnpaused()
        {
            var initialEventTick = 42;
            // Simulate two events being published.
            var noopEvent = new NoOpGameEvent(new GameTick(initialEventTick));
            
            _game.TimeMachine.AddEvent(noopEvent);
            // Ensure only one event got added.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            
            // Add a Pause Event at Tick 10,
            // Add an unpause event at Tick 20.
            // Effectively adds 10 ticks to the NoOp Event.
            var startPause = 10;
            var endPause = 20;
            var pauseTen = CreatePauseAt(startPause);
            var unpauseTwenty = CreateUnpauseAt(endPause);
            _game.TimeMachine.AddEvent(pauseTen);
            _game.TimeMachine.AddEvent(unpauseTwenty);
            
            _game.TimeMachine.GoTo(new GameTick(9));
            // Ensure all events exist as normal.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>().Count);
            Assert.AreEqual(startPause, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>().Count);
            Assert.AreEqual(endPause, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>()[0].OccursAt.GetTick());
            
            // Advance to inside the pause event.
            _game.TimeMachine.GoTo(new GameTick(15));
            // Ensure that events other than the Unpause event got pushed.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick + 5, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>().Count);
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>().Count);
            Assert.AreEqual(endPause, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>()[0].OccursAt.GetTick());
            
            // Advance to the unpause event
            _game.TimeMachine.GoTo(new GameTick(20));
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick + 10, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>().Count);
            Assert.AreEqual(0, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>().Count);
            
            // Advance after the unpause to ensure the events are not getting pushed anymore.
            _game.TimeMachine.GoTo(new GameTick(25));
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick + 10, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
        }
        
        [TestMethod]
        public void CanReverseAPauseEvent()
        {
            var initialEventTick = 42;
            // Simulate two events being published.
            var noopEvent = new NoOpGameEvent(new GameTick(initialEventTick));
            
            _game.TimeMachine.AddEvent(noopEvent);
            // Ensure only one event got added.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            
            // Add a Pause Event at Tick 10,
            // Add an unpause event at Tick 20.
            // Effectively adds 10 ticks to the NoOp Event.
            var startPause = 10;
            var endPause = 20;
            var pauseTen = CreatePauseAt(startPause);
            var unpauseTwenty = CreateUnpauseAt(endPause);
            _game.TimeMachine.AddEvent(pauseTen);
            _game.TimeMachine.AddEvent(unpauseTwenty);
            
            // Ensure all events exist as normal.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>().Count);
            Assert.AreEqual(startPause, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>().Count);
            Assert.AreEqual(endPause, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>()[0].OccursAt.GetTick());
            
            // Advance to after the pause event.
            _game.TimeMachine.GoTo(new GameTick(45));
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick + 10, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            
            // Go back to before the unpause event and ensure events are not pushed.
            _game.TimeMachine.GoTo(new GameTick(0));
            
            // Ensure pushed events get pulled back
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>().Count);
            Assert.AreEqual(startPause, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>().Count);
            Assert.AreEqual(endPause, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>()[0].OccursAt.GetTick());
        }
        
        [TestMethod]
        public void OnTickDoesNotOccurWhilePaused()
        {
            var initialEventTick = 42;
            // Simulate two events being published.
            var noopEvent = new NoOpGameEvent(new GameTick(initialEventTick));
            
            _game.TimeMachine.AddEvent(noopEvent);
            // Ensure only one event got added.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            
            // Add a Pause Event at Tick 10,
            // Add an unpause event at Tick 20.
            // Effectively adds 10 ticks to the NoOp Event.
            var startPause = 10;
            var endPause = 20;
            var pauseTen = CreatePauseAt(startPause);
            var unpauseTwenty = CreateUnpauseAt(endPause);
            _game.TimeMachine.AddEvent(pauseTen);
            _game.TimeMachine.AddEvent(unpauseTwenty);

            var onTickCounter = 0;
            _game.TimeMachine.OnTick += (timeMachine, onTickArgs) =>
            {
                onTickCounter++;
            };
            
            // Ensure all events exist as normal.
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>().Count);
            Assert.AreEqual(startPause, _game.TimeMachine.GetFutureEventsOf<PauseGameEvent>()[0].OccursAt.GetTick());
            
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>().Count);
            Assert.AreEqual(endPause, _game.TimeMachine.GetFutureEventsOf<UnpauseGameEvent>()[0].OccursAt.GetTick());
            
            // Advance to after the pause event.
            var progressedTicks = 45;
            _game.TimeMachine.GoTo(new GameTick(progressedTicks));
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>().Count);
            Assert.AreEqual(initialEventTick + 10, _game.TimeMachine.GetFutureEventsOf<NoOpGameEvent>()[0].OccursAt.GetTick());
            
            // Ensure that 10 ticks were paused.
            Assert.AreEqual(progressedTicks - 10, onTickCounter);
        }
        
        private PauseGameEvent CreatePauseAt(int tick)
        {
            return new PauseGameEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.PauseGameEventData,
                    OccursAtTick = tick,
                    SerializedEventData = JsonConvert.SerializeObject(new PauseGameEventData()
                    {
                        TimePaused = DateTime.UtcNow
                    })
                }
            });
        }
        
        private UnpauseGameEvent CreateUnpauseAt(int tick)
        {
            return new UnpauseGameEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    EventDataType = EventDataType.UnpauseGameEventData,
                    OccursAtTick = tick,
                    SerializedEventData = JsonConvert.SerializeObject(new UnpauseGameEventData()
                    {
                        TimeUnpaused = DateTime.UtcNow
                    })
                }
            });
        }

    }
}
