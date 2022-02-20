using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Timing;
using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;
/*
namespace SubterfugeCoreTest
{
    [TestClass]
    public class TimeMachineTest
    {
        TestUtils testUtils = new TestUtils();
        Player player = new Player("1");
        private Game game;
        
        [TestInitialize]
        public void Setup()
        {
            game = new Game(testUtils.GetDefaultGameConfiguration(new List<Player> { player }));

        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(game.TimeMachine);
            Assert.IsNotNull(game.TimeMachine.GetState());
        }

        [TestMethod]
        public void AddEvent()
        {
            Player player1 = new Player("1");
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player1);
            outpost.AddDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick());

            game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(2, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
        }

        [TestMethod]
        public void CanAdvanceTime()
        {
            GameTick initialTick = game.TimeMachine.GetCurrentTick();
            game.TimeMachine.Advance(5);
            
            Assert.IsTrue(initialTick < game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void CanRewindTime()
        {
            GameTick initialTick = game.TimeMachine.GetCurrentTick();
            game.TimeMachine.Advance(5);
            
            Assert.IsTrue(initialTick < game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), game.TimeMachine.GetCurrentTick().GetTick());

            GameTick advancedTick = game.TimeMachine.GetCurrentTick();
            
            game.TimeMachine.Rewind(1);
            
            
            Assert.IsTrue(advancedTick > game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(advancedTick.Rewind(1).GetTick(), game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void EventsSwitchQueuesWhenPassedForward()
        {
            Player player1 = new Player("1");
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player1);
            outpost.AddDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(2, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
            
            // Go past the tick
            game.TimeMachine.Advance(6);
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
        }

        [TestMethod]
        public void EventsSwitchQueuesWhenRewind()
        {
            Player player1 = new Player("1");
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player1);
            outpost.AddDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(2, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
            
            // Go past the tick
            game.TimeMachine.Advance(6);
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
            
            // Rewind back
            game.TimeMachine.Rewind(6);
            Assert.AreEqual(2, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
        }
        
        [TestMethod]
        public void CanRemoveEvents()
        {
            Player player1 = new Player("1");
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player1);
            outpost.AddDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(2, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
            
            game.TimeMachine.RemoveEvent(arriveEvent);
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
        }
        
        [TestMethod]
        public void CanGoToAGameTick()
        {

            GameTick initialTick = game.TimeMachine.GetCurrentTick();
            game.TimeMachine.GoTo(new GameTick(5));
            
            Assert.IsTrue(initialTick < game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void CanGoToAnEvent()
        {
            Player player1 = new Player("1");
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Generator("0", new RftVector(map, 0, 0), player1);
            outpost.AddDrillers(10);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(5));

            game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(2, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
            
            game.TimeMachine.GoTo(arriveEvent);
            Assert.AreEqual(arriveEvent.GetOccursAt().GetTick(), game.TimeMachine.GetCurrentTick().GetTick());
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
        }

    }
}

*/