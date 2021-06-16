using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Timing;
using System;
using System.Collections.Generic;
using System.Numerics;
using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;
using SubterfugeRemakeService;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class LaunchEventTest
    {
        private Game _game;


        [TestInitialize]
        public void Setup()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            
            GameConfiguration config = new GameConfiguration(players, DateTime.Now, new MapConfiguration(players));
            _game = new Game(config);
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = "0",
                    DrillerCount = 10,
                    SourceId = "0",
                }.ToByteString(),
                EventId = "123",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 10,
            }));
        }

        [TestMethod]
        public void CannotLaunchFromNonGeneratedOutposts()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetId(),
                }.ToByteString(),
                EventId = "a",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 1,
            });
            Assert.AreEqual(false, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
        }
        
        [TestMethod]
        public void CanLaunchSingleSub()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetId(),
                }.ToByteString(),
                EventId = "a",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 1,
            });

            Assert.AreEqual(true, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetId(),
                }.ToByteString(),
                EventId = "a",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 1,
            });
            Assert.AreEqual(true, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
            
            Assert.AreEqual(true, launch.BackwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            Assert.AreEqual(outpostOneInitial, outpost1.GetDrillerCount());
            Assert.AreEqual(outpostTwoInitial, outpost2.GetDrillerCount());
            Assert.AreEqual(0, _game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanGetTheLaunchedSub()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            outpost1.SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetId(),
                }.ToByteString(),
                EventId = "a",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 1,
            });
            Assert.AreEqual(true, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
            
            // Ensure can get the sub.
            Assert.IsNotNull(_game.TimeMachine.GetState().GetSubList()[0]);
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList()[0].GetDrillerCount());
            Assert.AreEqual(outpost1, _game.TimeMachine.GetState().GetSubList()[0].GetSource());
            Assert.AreEqual(outpost2, _game.TimeMachine.GetState().GetSubList()[0].GetDestination());
        }
        
        [TestMethod]
        public void SubLaunchCreatesCombatEvents()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            outpost1.SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetId(),
                }.ToByteString(),
                EventId = "a",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 1,
            });
            
            LaunchEvent launch2 = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost1.GetId(),
                    DrillerCount = 1,
                    SourceId = outpost2.GetId(),
                }.ToByteString(),
                EventId = "a",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 1,
            });
            
            Assert.AreEqual(true, launch1.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            Assert.AreEqual(true, launch2.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(2, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
            Assert.AreEqual(outpostTwoInitial - 1, outpost2.GetDrillerCount());
            
            // Ensure a combat event has been added that includes both subs.
            int subToSubBattles = 0;
            int subToOutpostBattles = 0;
            GameEvent arriveEvent = null;
            CombatEvent combatEvent = null;
            foreach(GameEvent gameEvent in _game.TimeMachine.GetQueuedEvents())
            {
                if (gameEvent is CombatEvent)
                {
                    combatEvent = (CombatEvent) gameEvent;
                    if (combatEvent.GetCombatants()[0] is Sub && combatEvent.GetCombatants()[1] is Sub)
                    {
                        subToSubBattles++;
                    } else
                    {
                        subToOutpostBattles++;
                        arriveEvent = gameEvent;
                    }
                }
            }
            // There should be 3 combats, one on each outpost, one on both subs.
            Assert.AreEqual(1, subToSubBattles);
            Assert.AreEqual(2, subToOutpostBattles);
        }
        
        [TestMethod]
        public void SubsArriveAfterLaunch()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(5);
            outpost1.SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetId(),
                    DrillerCount = 10,
                    SourceId = outpost1.GetId(),
                }.ToByteString(),
                EventId = "a",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 1,
            });
            Assert.AreEqual(true, launch1.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 10, outpost1.GetDrillerCount());
            
            // Ensure a combat event has been added.
            int combatEvents = 0;
            GameEvent combat = null;
            foreach (GameEvent gameEvent in _game.TimeMachine.GetQueuedEvents())
            {
                if (gameEvent is CombatEvent)
                {
                    combat = gameEvent;
                    combatEvents++;
                }
            }
            Assert.AreEqual(1, combatEvents);
            
            // Go to the event and ensure the arrival is successful
            _game.TimeMachine.GoTo(combat);
            _game.TimeMachine.Advance(1);
            
            Assert.AreEqual(true, combat.WasEventSuccessful());
            Assert.AreEqual(outpost1.GetOwner(), outpost2.GetOwner());
            Assert.AreEqual(Math.Abs(outpostTwoInitial - outpostOneInitial), outpost2.GetDrillerCount());
        }
    }
}
