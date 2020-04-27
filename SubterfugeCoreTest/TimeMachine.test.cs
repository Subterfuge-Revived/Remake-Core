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

namespace SubterfugeCoreTest
{
    [TestClass]
    public class TimeMachineTest
    {
        [TestInitialize]
        public void Setup()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Game game = new Game(config);

        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(Game.TimeMachine);
            Assert.IsNotNull(Game.TimeMachine.GetState());
        }

        [TestMethod]
        public void AddEvent()
        {
            Player player1 = new Player(1);
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Outpost(new RftVector(map, 0, 0), player1, OutpostType.Generator);
            outpost.AddDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(), outpost.GetInterceptionPoint(sub.GetCurrentPosition(), sub.GetSpeed()));

            Game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, Game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.TimeMachine.GetQueuedEvents()[0]);
        }

        [TestMethod]
        public void CanAdvanceTime()
        {
            GameTick initialTick = Game.TimeMachine.GetCurrentTick();
            Game.TimeMachine.Advance(5);
            
            Assert.IsTrue(initialTick < Game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), Game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void CanRewindTime()
        {
            GameTick initialTick = Game.TimeMachine.GetCurrentTick();
            Game.TimeMachine.Advance(5);
            
            Assert.IsTrue(initialTick < Game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), Game.TimeMachine.GetCurrentTick().GetTick());

            GameTick advancedTick = Game.TimeMachine.GetCurrentTick();
            
            Game.TimeMachine.Rewind(1);
            
            
            Assert.IsTrue(advancedTick > Game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(advancedTick.Rewind(1).GetTick(), Game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void EventsSwitchQueuesWhenPassedForward()
        {
            Player player1 = new Player(1);
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Outpost(new RftVector(map, 0, 0), player1, OutpostType.Generator);
            outpost.AddDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.FromTickNumber(5), outpost.GetInterceptionPoint(sub.GetCurrentPosition(), sub.GetSpeed()));

            Game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, Game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.TimeMachine.GetQueuedEvents()[0]);
            
            // Go past the tick
            Game.TimeMachine.Advance(6);
            Assert.AreEqual(0, Game.TimeMachine.GetQueuedEvents().Count);
        }

        [TestMethod]
        public void EventsSwitchQueuesWhenRewind()
        {
            Player player1 = new Player(1);
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Outpost(new RftVector(map, 0, 0), player1, OutpostType.Generator);
            outpost.AddDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.FromTickNumber(5), outpost.GetInterceptionPoint(sub.GetCurrentPosition(), sub.GetSpeed()));

            Game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, Game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.TimeMachine.GetQueuedEvents()[0]);
            
            // Go past the tick
            Game.TimeMachine.Advance(6);
            Assert.AreEqual(0, Game.TimeMachine.GetQueuedEvents().Count);
            
            // Rewind back
            Game.TimeMachine.Rewind(6);
            Assert.AreEqual(1, Game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.TimeMachine.GetQueuedEvents()[0]);
        }
        
        [TestMethod]
        public void CanRemoveEvents()
        {
            Player player1 = new Player(1);
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Outpost(new RftVector(map, 0, 0), player1, OutpostType.Generator);
            outpost.AddDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.FromTickNumber(5), outpost.GetInterceptionPoint(sub.GetCurrentPosition(), sub.GetSpeed()));

            Game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, Game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.TimeMachine.GetQueuedEvents()[0]);
            
            Game.TimeMachine.RemoveEvent(arriveEvent);
            Assert.AreEqual(0, Game.TimeMachine.GetQueuedEvents().Count);
        }
        
        [TestMethod]
        public void CanGoToAGameTick()
        {

            GameTick initialTick = Game.TimeMachine.GetCurrentTick();
            Game.TimeMachine.GoTo(GameTick.FromTickNumber(5));
            
            Assert.IsTrue(initialTick < Game.TimeMachine.GetCurrentTick());
            Assert.AreEqual(initialTick.Advance(5).GetTick(), Game.TimeMachine.GetCurrentTick().GetTick());
        }
        
        [TestMethod]
        public void CanGoToAnEvent()
        {
            Player player1 = new Player(1);
            Rft map = new Rft(3000, 3000);
            Outpost outpost = new Outpost(new RftVector(map, 0, 0), player1, OutpostType.Generator);
            outpost.AddDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.FromTickNumber(5), outpost.GetInterceptionPoint(sub.GetCurrentPosition(), sub.GetSpeed()));

            Game.TimeMachine.AddEvent(arriveEvent);
            Assert.AreEqual(1, Game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.TimeMachine.GetQueuedEvents()[0]);
            
            Game.TimeMachine.GoTo(arriveEvent);
            Assert.AreEqual(arriveEvent.GetTick().GetTick(), Game.TimeMachine.GetCurrentTick().GetTick());
            Assert.AreEqual(0, Game.TimeMachine.GetQueuedEvents().Count);
        }

    }
}
