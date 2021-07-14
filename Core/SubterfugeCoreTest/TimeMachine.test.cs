﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeRemakeService;
using GameEventModels;
using Google.Protobuf;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class TimeMachineTest
    {
        private Game game;
        
        [TestInitialize]
        public void Setup()
        {
            game = new Game();
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(game);
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
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
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
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
            
            // Go past the tick
            game.TimeMachine.Advance(6);
            Assert.AreEqual(0, game.TimeMachine.GetQueuedEvents().Count);
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
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
            
            // Go past the tick
            game.TimeMachine.Advance(6);
            Assert.AreEqual(0, game.TimeMachine.GetQueuedEvents().Count);
            
            // Rewind back
            game.TimeMachine.Rewind(6);
            // no longer true due to vision events
            // Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
            Assert.IsTrue(game.TimeMachine.GetQueuedEvents().Contains(arriveEvent));
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
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(arriveEvent, game.TimeMachine.GetQueuedEvents()[0]);
            
            game.TimeMachine.RemoveEvent(arriveEvent);
            Assert.AreEqual(0, game.TimeMachine.GetQueuedEvents().Count);
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
            outpost.AddDrillers(50);
            Sub sub = new Sub("1", outpost, outpost, new GameTick(), 10, player1);
            DrillMineEvent drillEvent = new DrillMineEvent(new GameEventModel()
            {
                EventData = new DrillMineEventData()
                {
                    SourceId = outpost.GetId()
                }.ToByteString(),
                EventId = Guid.NewGuid().ToString(),
                EventType = EventType.DrillMineEvent,
                OccursAtTick = 5
            });

            game.TimeMachine.AddEvent(drillEvent);
            Assert.AreEqual(1, game.TimeMachine.GetQueuedEvents().Count);
            Assert.AreEqual(drillEvent, game.TimeMachine.GetQueuedEvents()[0]);

            game.TimeMachine.GoTo(drillEvent);
            Assert.AreEqual(drillEvent.GetOccursAt().GetTick(), game.TimeMachine.GetCurrentTick().GetTick());
            Assert.AreEqual(0, game.TimeMachine.GetQueuedEvents().Count);
        }

    }
}
