using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Timing;
using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class TimeMachineTest
    {
        [TestInitialize]
        public void setup()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Game game = new Game(config);

        }

        [TestMethod]
        public void constructor()
        {
            Assert.IsNotNull(Game.timeMachine);
            Assert.IsNotNull(Game.timeMachine.getState());
        }

        [TestMethod]
        public void addEvent()
        {
            Player player1 = new Player(1);
            Outpost outpost = new Outpost(new Vector2(0, 0), player1, OutpostType.GENERATOR);
            outpost.addDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(), outpost.getTargetLocation(sub.getCurrentLocation(), sub.getSpeed()));

            Game.timeMachine.addEvent(arriveEvent);
            Assert.AreEqual(1, Game.timeMachine.getQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.timeMachine.getQueuedEvents()[0]);
        }

        [TestMethod]
        public void canAdvanceTime()
        {
            GameTick initialTick = Game.timeMachine.getCurrentTick();
            Game.timeMachine.advance(5);
            
            Assert.IsTrue(initialTick < Game.timeMachine.getCurrentTick());
            Assert.AreEqual(initialTick.advance(5).getTick(), Game.timeMachine.getCurrentTick().getTick());
        }
        
        [TestMethod]
        public void canRewindTime()
        {
            GameTick initialTick = Game.timeMachine.getCurrentTick();
            Game.timeMachine.advance(5);
            
            Assert.IsTrue(initialTick < Game.timeMachine.getCurrentTick());
            Assert.AreEqual(initialTick.advance(5).getTick(), Game.timeMachine.getCurrentTick().getTick());

            GameTick advancedTick = Game.timeMachine.getCurrentTick();
            
            Game.timeMachine.rewind(1);
            
            
            Assert.IsTrue(advancedTick > Game.timeMachine.getCurrentTick());
            Assert.AreEqual(advancedTick.rewind(1).getTick(), Game.timeMachine.getCurrentTick().getTick());
        }
        
        [TestMethod]
        public void EventsSwitchQueuesWhenPassedForward()
        {
            Player player1 = new Player(1);
            Outpost outpost = new Outpost(new Vector2(0, 0), player1, OutpostType.GENERATOR);
            outpost.addDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.fromTickNumber(5), outpost.getTargetLocation(sub.getCurrentLocation(), sub.getSpeed()));

            Game.timeMachine.addEvent(arriveEvent);
            Assert.AreEqual(1, Game.timeMachine.getQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.timeMachine.getQueuedEvents()[0]);
            
            // Go past the tick
            Game.timeMachine.advance(6);
            Assert.AreEqual(0, Game.timeMachine.getQueuedEvents().Count);
        }

        [TestMethod]
        public void EventsSwitchQueuesWhenRewind()
        {
            Player player1 = new Player(1);
            Outpost outpost = new Outpost(new Vector2(0, 0), player1, OutpostType.GENERATOR);
            outpost.addDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.fromTickNumber(5), outpost.getTargetLocation(sub.getCurrentLocation(), sub.getSpeed()));

            Game.timeMachine.addEvent(arriveEvent);
            Assert.AreEqual(1, Game.timeMachine.getQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.timeMachine.getQueuedEvents()[0]);
            
            // Go past the tick
            Game.timeMachine.advance(6);
            Assert.AreEqual(0, Game.timeMachine.getQueuedEvents().Count);
            
            // Rewind back
            Game.timeMachine.rewind(6);
            Assert.AreEqual(1, Game.timeMachine.getQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.timeMachine.getQueuedEvents()[0]);
        }
        
        [TestMethod]
        public void canRemoveEvents()
        {
            Player player1 = new Player(1);
            Outpost outpost = new Outpost(new Vector2(0, 0), player1, OutpostType.GENERATOR);
            outpost.addDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.fromTickNumber(5), outpost.getTargetLocation(sub.getCurrentLocation(), sub.getSpeed()));

            Game.timeMachine.addEvent(arriveEvent);
            Assert.AreEqual(1, Game.timeMachine.getQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.timeMachine.getQueuedEvents()[0]);
            
            Game.timeMachine.removeEvent(arriveEvent);
            Assert.AreEqual(0, Game.timeMachine.getQueuedEvents().Count);
        }
        
        [TestMethod]
        public void canGoToAGameTick()
        {

            GameTick initialTick = Game.timeMachine.getCurrentTick();
            Game.timeMachine.goTo(GameTick.fromTickNumber(5));
            
            Assert.IsTrue(initialTick < Game.timeMachine.getCurrentTick());
            Assert.AreEqual(initialTick.advance(5).getTick(), Game.timeMachine.getCurrentTick().getTick());
        }
        
        [TestMethod]
        public void canGoToAnEvent()
        {
            Player player1 = new Player(1);
            Outpost outpost = new Outpost(new Vector2(0, 0), player1, OutpostType.GENERATOR);
            outpost.addDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, GameTick.fromTickNumber(5), outpost.getTargetLocation(sub.getCurrentLocation(), sub.getSpeed()));

            Game.timeMachine.addEvent(arriveEvent);
            Assert.AreEqual(1, Game.timeMachine.getQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, Game.timeMachine.getQueuedEvents()[0]);
            
            Game.timeMachine.goTo(arriveEvent);
            Assert.AreEqual(arriveEvent.getTick().getTick(), Game.timeMachine.getCurrentTick().getTick());
            Assert.AreEqual(0, Game.timeMachine.getQueuedEvents().Count);
        }

    }
}
