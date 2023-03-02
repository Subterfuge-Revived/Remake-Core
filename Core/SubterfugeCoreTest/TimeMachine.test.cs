using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
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
        public void EventsSwitchQueuesWhenRewind()
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
            Assert.AreEqual(1, _game.TimeMachine.GetFutureEventsOf<CombatEvent>().Count);
            Assert.AreEqual(arriveEvent, _game.TimeMachine.GetFutureEventsOf<CombatEvent>()[0]);
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

    }
}
