using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core.Entities.Positions;
using System;
using System.Collections.Generic;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.combat;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class LaunchEventTest
    {
        private Game _game;
        private TestUtils testUtils = new TestUtils();
        private List<Player> players = new List<Player>
        {
            new Player("1"),
            new Player("2")
        };


        [TestInitialize]
        public void Setup()
        {
            GameConfiguration config = testUtils.GetDefaultGameConfiguration(players);
            _game = new Game(config);
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = "0",
                    DrillerCount = 10,
                    SourceId = "0",
                },
                Id = "123",
                OccursAtTick = 10,
            }));
        }
        
        [TestMethod]
        public void CanLaunchSingleSub()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                },
                Id = "a",
                OccursAtTick = 1,
            });
            Assert.AreEqual(true, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            int outpostTwoInitial = outpost2.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                },
                Id = "a",
                OccursAtTick = 1,
            });
            Assert.AreEqual(true, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            Assert.AreEqual(true, launch.BackwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            Assert.AreEqual(outpostOneInitial, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(outpostTwoInitial, outpost2.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(0, _game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanGetTheLaunchedSub()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                },
                Id = "a",
                OccursAtTick = 1,
            });
            Assert.AreEqual(true, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            // Ensure can get the sub.
            Assert.IsNotNull(_game.TimeMachine.GetState().GetSubList()[0]);
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList()[0].GetComponent<DrillerCarrier>().GetDrillerCount());
            // Assert.AreEqual(outpost1, _game.TimeMachine.GetState().GetSubList()[0].GetComponent<PositionManager>().GetSource());
            Assert.AreEqual(outpost2, _game.TimeMachine.GetState().GetSubList()[0].GetComponent<PositionManager>().GetDestination());
        }
        
        [TestMethod]
        public void SubLaunchCreatesCombatEvents()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            int outpostTwoInitial = outpost2.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                    DrillerCount = 1,
                    SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                },
                Id = "a",
                OccursAtTick = 1,
            });
            
            LaunchEvent launch2 = new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost1.GetComponent<IdentityManager>().GetId(),
                    DrillerCount = 1,
                    SourceId = outpost2.GetComponent<IdentityManager>().GetId(),
                },
                Id = "a",
                OccursAtTick = 1,
            });
            
            Assert.AreEqual(true, launch1.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            Assert.AreEqual(true, launch2.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(2, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(outpostTwoInitial - 1, outpost2.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            // Ensure a combat event has been added that includes both subs.
            int subToSubBattles = 0;
            int subToOutpostBattles = 0;
            foreach(GameEvent gameEvent in _game.TimeMachine.GetQueuedEvents())
            {
                if (gameEvent is CombatEvent)
                {
                    var combatEvent = (CombatEvent) gameEvent;
                    if (combatEvent.GetCombatants()[0] is Sub && combatEvent.GetCombatants()[1] is Sub)
                    {
                        subToSubBattles++;
                    } else
                    {
                        subToOutpostBattles++;
                    }
                }
            }
            // There should be 3 combats, one on each outpost, one on both subs.
            // TODO: Once subs on path is fixed, Update this. It should be 1.
            Assert.AreEqual(0, subToSubBattles);
            Assert.AreEqual(2, subToOutpostBattles);
        }
        
        [TestMethod]
        public void SubsArriveAfterLaunch()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0));
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(5);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            int outpostTwoInitial = outpost2.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                    DrillerCount = 10,
                    SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                },
                Id = "a",
                OccursAtTick = 1,
            });
            Assert.AreEqual(true, launch1.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 10, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
            
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
            
            Assert.AreEqual(true, combat != null && combat.WasEventSuccessful());
            Assert.AreEqual(outpost1.GetComponent<DrillerCarrier>().GetOwner(), outpost2.GetComponent<DrillerCarrier>().GetOwner());
            Assert.AreEqual(Math.Abs(outpostTwoInitial - outpostOneInitial), outpost2.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
    }
}