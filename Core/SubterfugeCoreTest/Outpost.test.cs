using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using GameEventModels;
using Google.Protobuf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;
using SubterfugeRemakeService;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class OutpostTest
    {
	    Rft _map;
        RftVector _outpostLocation;
        Outpost _outpost;
        Outpost _outpost2;

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
        public void GetPosition()
        {
            Assert.AreEqual(_outpostLocation.X, _outpost.GetCurrentPosition().X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetCurrentPosition().Y);
        }
        [TestMethod]
        public void GetTargetLocation()
        {
            // Outpost target location should always be the outpost's location
            Assert.AreEqual(_outpostLocation.X, _outpost.GetInterceptionPosition(new RftVector(_map, 0, 0), 0.25f).X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetInterceptionPosition(new RftVector(_map, 0, 0), 0.25f).Y);

            Assert.AreEqual(_outpostLocation.X, _outpost.GetInterceptionPosition(new RftVector(_map, 100, 100), 1).X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetInterceptionPosition(new RftVector(_map, 100, 100), 1).Y);

            Assert.AreEqual(_outpostLocation.X, _outpost.GetInterceptionPosition(new RftVector(_map, 999, 999), 999).X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetInterceptionPosition(new RftVector(_map, 999, 999), 999).Y);
        }

        [TestMethod]
        public void IsITargetable()
        {
            Assert.IsTrue(typeof(ITargetable).IsAssignableFrom(_outpost.GetType()));
        }

        [TestMethod]
        public void IsOwnable()
        {
            Assert.IsTrue(typeof(IOwnable).IsAssignableFrom(_outpost.GetType()));
        }
        
        [TestMethod]
        public void CanRemoveDrillers()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            int initialDrillers = outpost.GetDrillerCount();
            outpost.RemoveDrillers(40);
            Assert.AreEqual(initialDrillers - 40, outpost.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanAddDrillers()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            int initialDrillers = outpost.GetDrillerCount();
            outpost.AddDrillers(40);
            Assert.AreEqual(initialDrillers + 40, outpost.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanSetDrillerCount()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.SetDrillerCount(420);
            Assert.AreEqual(420, outpost.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanLaunchSubs()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players, DateTime.Now, new MapConfiguration(players));
            Game game = new Game(config);
            game.TimeMachine.GetState().GetOutposts().Add(_outpost);
            game.TimeMachine.GetState().GetOutposts().Add(_outpost2);

            int initialDrillers = _outpost.GetDrillerCount();
            _outpost.LaunchSub(game.TimeMachine.GetState(), new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = _outpost.GetId(),
                    DrillerCount = 10,
                    SourceId = _outpost2.GetId(),
                }.ToByteString(),
                EventId = "123",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 10,
            }));

            Assert.AreEqual(initialDrillers - 10, _outpost.GetDrillerCount());
            Assert.AreEqual(1, game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players, DateTime.Now, new MapConfiguration(players));
            Game game = new Game(config);
            game.TimeMachine.GetState().GetOutposts().Add(_outpost);
            game.TimeMachine.GetState().GetOutposts().Add(_outpost2);

            var launchEvent = new LaunchEvent(new GameEventModel()
            {
                EventData = new LaunchEventData()
                {
                    DestinationId = _outpost2.GetId(),
                    DrillerCount = 10,
                    SourceId = _outpost.GetId(),
                }.ToByteString(),
                EventId = "123",
                EventType = EventType.LaunchEvent,
                OccursAtTick = 10,
            });
            
            int initialDrillers = _outpost.GetDrillerCount();
            launchEvent.ForwardAction(game.TimeMachine, game.TimeMachine.GetState());
            
            Assert.AreEqual(initialDrillers - 10, _outpost.GetDrillerCount());
            Assert.AreEqual(1, game.TimeMachine.GetState().GetSubList().Count);
            
            launchEvent.BackwardAction(game.TimeMachine, game.TimeMachine.GetState());
            
            Assert.AreEqual(initialDrillers, _outpost.GetDrillerCount());
            Assert.AreEqual(0, game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanRemoveShields()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetShieldManager().SetShields(10);
            int initialShields = outpost.GetShieldManager().GetShields();
            outpost.GetShieldManager().RemoveShields(5);
            Assert.AreEqual(initialShields - 5, outpost.GetShieldManager().GetShields());
        }
        
        [TestMethod]
        public void CanAddShields()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            int initialShield = outpost.GetShieldManager().GetShields();
            outpost.GetShieldManager().AddShield(1);
            Assert.AreEqual(initialShield + 1, outpost.GetShieldManager().GetShields());
        }
        
        [TestMethod]
        public void CanSetShields()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetShieldManager().SetShields(1);
            Assert.AreEqual(1, outpost.GetShieldManager().GetShields());
        }
        
        [TestMethod]
        public void ShieldCapacityWorks()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetShieldManager().SetShieldCapacity(100);
            outpost.GetShieldManager().SetShields(5);
            outpost.GetShieldManager().AddShield(100);
            
            Assert.AreEqual(outpost.GetShieldManager().GetShieldCapacity(), outpost.GetShieldManager().GetShields());
            
            outpost.GetShieldManager().SetShields(105);
            Assert.AreEqual(outpost.GetShieldManager().GetShieldCapacity(), outpost.GetShieldManager().GetShields());
        }
        
        [TestMethod]
        public void CannotHaveNegativeShield()
        {
            Outpost outpost = new Mine("0", new RftVector(_map, 0, 0), new Player("1"));
            outpost.GetShieldManager().SetShields(10);
            int initialShields = outpost.GetShieldManager().GetShields();
            outpost.GetShieldManager().RemoveShields(15);
            Assert.AreEqual(0, outpost.GetShieldManager().GetShields());
        }

        [TestMethod]
        public void CanToggleSheilds()
        {
            Outpost outpost = new Mine("0",new RftVector(_map, 0, 0), new Player("1"));
            bool initialState = outpost.GetShieldManager().IsShieldActive();
            outpost.GetShieldManager().ToggleShield();
            Assert.AreEqual(!initialState, outpost.GetShieldManager().IsShieldActive());
        }

        [TestMethod]
        public void CanSeeLocationInVision()
        {
            Outpost outpost = new Mine("0",new RftVector(_map, 0, 0), new Player("1"));
            Outpost outpost2 = new Mine("1",new RftVector(_map, Constants.BASE_OUTPOST_VISION_RADIUS - 1, 0), new Player("2"));
            Assert.IsTrue(outpost.isInVisionRange(new GameTick(1), outpost2));
        }
        
        [TestMethod]
        public void CannotSeeLocationOutOfVision()
        {
            
            Outpost outpost = new Mine("0",new RftVector(_map, 0, 0), new Player("1"));
            Outpost outpost2 = new Mine("1",new RftVector(_map, Constants.BASE_OUTPOST_VISION_RADIUS + 1, 0), new Player("2"));
            Assert.IsFalse(outpost.isInVisionRange(new GameTick(1), outpost2));
        }
    }
}
