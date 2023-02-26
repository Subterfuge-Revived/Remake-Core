using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Test
{
    [TestClass]
    public class LaunchEventTest
    {
        private Game _game;
        private TestUtils testUtils = new TestUtils();
        private List<Player> players = new List<Player>
        {
            new Player(new SimpleUser() { Id = "1" }),
            new Player(new SimpleUser() { Id = "2" })
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
            Assert.IsNotNull(new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 10,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
                    {
                        DestinationId = "0",
                        DrillerCount = 10,
                        SourceId = "0",
                    }),
                },
                Id = "123",
            }));
        }
        
        [TestMethod]
        public void CanLaunchSingleSub()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
                    {
                        DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                        DrillerCount = 1,
                        SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                    }),
                },
                Id = "a",
            });
            Assert.AreEqual(true, launch.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            int outpostTwoInitial = outpost2.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
                    {
                        DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                        DrillerCount = 1,
                        SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                    }),
                },
                Id = "a",
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
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
                    {
                        DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                        DrillerCount = 1,
                        SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                    }),
                },
                Id = "a",
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
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            int outpostTwoInitial = outpost2.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
                    {
                        DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                        DrillerCount = 1,
                        SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                    }),
                },
                Id = "a",
            });
            
            LaunchEvent launch2 = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
                    {
                        DestinationId = outpost1.GetComponent<IdentityManager>().GetId(),
                        DrillerCount = 1,
                        SourceId = outpost2.GetComponent<IdentityManager>().GetId(),
                    }),
                },
                Id = "a",
            });
            
            Assert.AreEqual(true, launch1.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            Assert.AreEqual(true, launch2.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(2, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(outpostTwoInitial - 1, outpost2.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void SubsArriveAfterLaunch()
        {
            Outpost outpost1 = new Generator("0",new RftVector(new Rft(300, 300), 0, 0), _game.TimeMachine);
            Outpost outpost2 = new Generator("1",new RftVector(new Rft(300, 300), 0, 10), _game.TimeMachine);
            outpost1.GetComponent<DrillerCarrier>().SetDrillerCount(10);
            outpost2.GetComponent<DrillerCarrier>().SetDrillerCount(5);
            outpost1.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.GetComponent<DrillerCarrier>().SetOwner(_game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetComponent<DrillerCarrier>().GetDrillerCount();
            int outpostTwoInitial = outpost2.GetComponent<DrillerCarrier>().GetDrillerCount();
            
            _game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            _game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameRoomEvent()
            {
                GameEventData = new GameEventData()
                {
                    OccursAtTick = 1,
                    EventDataType = EventDataType.LaunchEventData,
                    SerializedEventData = JsonConvert.SerializeObject(new LaunchEventData()
                    {
                        DestinationId = outpost2.GetComponent<IdentityManager>().GetId(),
                        DrillerCount = 10,
                        SourceId = outpost1.GetComponent<IdentityManager>().GetId(),
                    }),
                },
                Id = "a",
                IssuedBy = _game.TimeMachine.GetState().GetPlayers()[0].PlayerInstance
            });
            Assert.AreEqual(true, launch1.ForwardAction(_game.TimeMachine, _game.TimeMachine.GetState()));
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 10, outpost1.GetComponent<DrillerCarrier>().GetDrillerCount());
            
            // Go to the combat.
            var combatHappened = false;
            _game.TimeMachine.GetState().GetSubList()[0].GetComponent<PositionManager>().OnPostCombat += (PositionManager, postCombatEvent) =>
            {
                combatHappened = true;
            };

            while (!combatHappened)
            {
                _game.TimeMachine.Advance(1);
            }

            Assert.AreEqual(0, _game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpost1.GetComponent<DrillerCarrier>().GetOwner(), outpost2.GetComponent<DrillerCarrier>().GetOwner());
            
            // Awkwarddd the outpost generates some shields in the process of the sub travelling.
            // Delta of 1 for the 1 shield that got generated.
            var shieldsGenerated = 1;
            Assert.AreEqual(Math.Abs((outpostTwoInitial + shieldsGenerated) - outpostOneInitial), outpost2.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
    }
}