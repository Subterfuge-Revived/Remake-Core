﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class OutpostTest
    {
	    Rft _map;
        RftVector _outpostLocation;
        Outpost _outpost;
        Outpost _outpost2;
        private readonly TestUtils _testUtils = new TestUtils();

        [TestInitialize]
        public void Setup()
        {
            this._map = new Rft(3000, 3000);
            this._outpostLocation = new RftVector(_map, 0, 0);
            this._outpostLocation = new RftVector(_map, 15, 15);
            this._outpost = new Factory("0", _outpostLocation, new Player("1"));
            this._outpost2 = new Factory("1", _outpostLocation, new Player("1"));
        }
        
        [TestMethod]
        public void HasDrillerCarrier()
        {
            Assert.IsNotNull(_outpost.GetComponent<DrillerCarrier>());
        }
        
        [TestMethod]
        public void HasSpeedManager()
        {
            Assert.IsNotNull(_outpost.GetComponent<SpeedManager>());
        }
        
        [TestMethod]
        public void HasPositionManager()
        {
            Assert.IsNotNull(_outpost.GetComponent<PositionManager>());
        }
        
        [TestMethod]
        public void HasSpecialistManager()
        {
            Assert.IsNotNull(_outpost.GetComponent<SpecialistManager>());
        }
        
        [TestMethod]
        public void HasIdentityManager()
        {
            Assert.IsNotNull(_outpost.GetComponent<IdentityManager>());
        }
        
        [TestMethod]
        public void HasShieldManager()
        {
            Assert.IsNotNull(_outpost.GetComponent<ShieldManager>());
        }
        
        [TestMethod]
        public void HasSubLauncher()
        {
            Assert.IsNotNull(_outpost.GetComponent<SubLauncher>());
        }
        
        [TestMethod]
        public void HasVisionManager()
        {
            Assert.IsNotNull(_outpost.GetComponent<VisionManager>());
        }

        [TestMethod]
        public void GetPosition()
        {
            Assert.AreEqual(_outpostLocation.X, _outpost.GetComponent<PositionManager>().GetPositionAt(new GameTick(0)).X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetComponent<PositionManager>().GetPositionAt(new GameTick(0)).Y);
        }
        
        [TestMethod]
        public void GetTargetLocation()
        {
            // Outpost target location should always be the outpost's location
            Assert.AreEqual(_outpostLocation.X, _outpost.GetComponent<PositionManager>().GetInterceptionPosition(new RftVector(_map, 0, 0), 0.25f).X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetComponent<PositionManager>().GetInterceptionPosition(new RftVector(_map, 0, 0), 0.25f).Y);

            Assert.AreEqual(_outpostLocation.X, _outpost.GetComponent<PositionManager>().GetInterceptionPosition(new RftVector(_map, 100, 100), 1).X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetComponent<PositionManager>().GetInterceptionPosition(new RftVector(_map, 100, 100), 1).Y);

            Assert.AreEqual(_outpostLocation.X, _outpost.GetComponent<PositionManager>().GetInterceptionPosition(new RftVector(_map, 999, 999), 999).X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetComponent<PositionManager>().GetInterceptionPosition(new RftVector(_map, 999, 999), 999).Y);
        }

        [TestMethod]
        public void CanRemoveDrillers()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            int initialDrillers = outpost.GetComponent<DrillerCarrier>().GetDrillerCount();
            outpost.GetComponent<DrillerCarrier>().RemoveDrillers(5);
            Assert.AreEqual(initialDrillers - 5, outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanAddDrillers()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            int initialDrillers = outpost.GetComponent<DrillerCarrier>().GetDrillerCount();
            outpost.GetComponent<DrillerCarrier>().AddDrillers(40);
            Assert.AreEqual(initialDrillers + 40, outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanSetDrillerCount()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetComponent<DrillerCarrier>().SetDrillerCount(420);
            Assert.AreEqual(420, outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
        }
        
        [TestMethod]
        public void CanLaunchSubs()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            Game game = new Game(_testUtils.GetDefaultGameConfiguration(players));
            game.TimeMachine.GetState().GetOutposts().Add(_outpost);
            game.TimeMachine.GetState().GetOutposts().Add(_outpost2);

            int initialDrillers = _outpost.GetComponent<DrillerCarrier>().GetDrillerCount();
            _outpost.GetComponent<SubLauncher>().LaunchSub(game.TimeMachine.GetState(), new LaunchEvent(new GameEventData()
                {
                    EventData = new LaunchEventData()
                    {
                        DestinationId = _outpost.GetComponent<IdentityManager>().GetId(),
                        DrillerCount = 10,
                        SourceId = _outpost2.GetComponent<IdentityManager>().GetId(),
                    },
                    Id = "123",
                    RoomId = "",
                    OccursAtTick = 10,
                }));

            Assert.AreEqual(initialDrillers - 10, _outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(1, game.TimeMachine.GetState().GetSubList().Count);
        }
    
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            Game game = new Game(_testUtils.GetDefaultGameConfiguration(players));
            game.TimeMachine.GetState().GetOutposts().Add(_outpost);
            game.TimeMachine.GetState().GetOutposts().Add(_outpost2);

            var launchEvent = new LaunchEvent(new GameEventData()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = _outpost2.GetComponent<IdentityManager>().GetId(),
                    DrillerCount = 10,
                    SourceId = _outpost.GetComponent<IdentityManager>().GetId(),
                },
                Id = "123",
                OccursAtTick = 10,
            });
            
            int initialDrillers = _outpost.GetComponent<DrillerCarrier>().GetDrillerCount();
            launchEvent.ForwardAction(game.TimeMachine, game.TimeMachine.GetState());
            
            Assert.AreEqual(initialDrillers - 10, _outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(1, game.TimeMachine.GetState().GetSubList().Count);
            
            launchEvent.BackwardAction(game.TimeMachine, game.TimeMachine.GetState());
            
            Assert.AreEqual(initialDrillers, _outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
            Assert.AreEqual(0, game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanRemoveShields()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetComponent<ShieldManager>().SetShields(10);
            int initialShields = outpost.GetComponent<ShieldManager>().GetShields();
            outpost.GetComponent<ShieldManager>().RemoveShields(5);
            Assert.AreEqual(initialShields - 5, outpost.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanAddShields()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            int initialShield = outpost.GetComponent<ShieldManager>().GetShields();
            outpost.GetComponent<ShieldManager>().AddShield(1);
            Assert.AreEqual(initialShield + 1, outpost.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CanSetShields()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetComponent<ShieldManager>().SetShields(1);
            Assert.AreEqual(1, outpost.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void ShieldCapacityWorks()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetComponent<ShieldManager>().SetShieldCapacity(100);
            outpost.GetComponent<ShieldManager>().SetShields(5);
            outpost.GetComponent<ShieldManager>().AddShield(100);
            
            Assert.AreEqual(outpost.GetComponent<ShieldManager>().GetShieldCapacity(), outpost.GetComponent<ShieldManager>().GetShields());
            
            outpost.GetComponent<ShieldManager>().SetShields(105);
            Assert.AreEqual(outpost.GetComponent<ShieldManager>().GetShieldCapacity(), outpost.GetComponent<ShieldManager>().GetShields());
        }
        
        [TestMethod]
        public void CannotHaveNegativeShield()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetComponent<ShieldManager>().SetShields(10);
            outpost.GetComponent<ShieldManager>().RemoveShields(15);
            Assert.AreEqual(0, outpost.GetComponent<ShieldManager>().GetShields());
        }

        [TestMethod]
        public void CanToggleShields()
        {
            Outpost outpost = new Mine("0",new RftVector(_map, 0, 0), new Player("1"));
            bool initialState = outpost.GetComponent<ShieldManager>().IsShieldActive();
            outpost.GetComponent<ShieldManager>().ToggleShield();
            Assert.AreEqual(!initialState, outpost.GetComponent<ShieldManager>().IsShieldActive());
        }

        [TestMethod]
        public void CanSeeLocationInVision()
        {
            Outpost outpost = new Mine("0",new RftVector(_map, 0, 0), new Player("1"));
            Outpost outpost2 = new Mine("1",new RftVector(_map, Constants.BaseOutpostVisionRadius - 1, 0), new Player("2"));
            Assert.IsTrue(outpost.GetComponent<VisionManager>().IsInVisionRange(new GameTick(1), outpost2.GetComponent<PositionManager>()));
        }
        
        [TestMethod]
        public void CannotSeeLocationOutOfVision()
        {
            
            Outpost outpost = new Mine("0",new RftVector(_map, 0, 0), new Player("1"));
            Outpost outpost2 = new Mine("1",new RftVector(_map, Constants.BaseOutpostVisionRadius + 1, 0), new Player("2"));
            Assert.IsFalse(outpost.GetComponent<VisionManager>().IsInVisionRange(new GameTick(1), outpost2.GetComponent<PositionManager>()));
        }
    }
}
