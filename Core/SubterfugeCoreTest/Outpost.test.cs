using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class OutpostTest
    {
	    Rft _map;
        RftVector _outpostLocation;
        Outpost _outpost;

        [TestInitialize]
        public void Setup()
        {
	        this._map = new Rft(3000, 3000);
            this._outpostLocation = new RftVector(_map, 0, 0);
            this._outpost = new Outpost(_outpostLocation, new Player("1"), OutpostType.Generator);
        }

        [TestMethod]
        public void GetPosition()
        {
            Assert.AreEqual(_outpostLocation.X, _outpost.GetPosition().X);
            Assert.AreEqual(_outpostLocation.Y, _outpost.GetPosition().Y);
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
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            int initialDrillers = outpost.GetDrillerCount();
            outpost.RemoveDrillers(40);
            Assert.AreEqual(initialDrillers - 40, outpost.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanAddDrillers()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            int initialDrillers = outpost.GetDrillerCount();
            outpost.AddDrillers(40);
            Assert.AreEqual(initialDrillers + 40, outpost.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanSetDrillerCount()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            outpost.SetDrillerCount(420);
            Assert.AreEqual(420, outpost.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanLaunchSubs()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players);
            Game game = new Game(config);
            
            int initialDrillers = _outpost.GetDrillerCount();
            _outpost.LaunchSub(10, new Outpost(new RftVector(_map, 0,0), new Player("1"), OutpostType.Mine));
            
            Assert.AreEqual(initialDrillers - 10, _outpost.GetDrillerCount());
            Assert.AreEqual(1, Game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players);
            Game game = new Game(config);
            
            int initialDrillers = _outpost.GetDrillerCount();
            _outpost.LaunchSub(10, new Outpost(new RftVector(_map, 0,0), new Player("1"), OutpostType.Mine));
            
            Assert.AreEqual(initialDrillers - 10, _outpost.GetDrillerCount());
            Assert.AreEqual(1, Game.TimeMachine.GetState().GetSubList().Count);
            
            _outpost.UndoLaunch(Game.TimeMachine.GetState().GetSubList()[0]);
            
            Assert.AreEqual(initialDrillers, _outpost.GetDrillerCount());
            Assert.AreEqual(0, Game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanRemoveShields()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            outpost.SetShields(10);
            int initialShields = outpost.GetShields();
            outpost.RemoveShields(5);
            Assert.AreEqual(initialShields - 5, outpost.GetShields());
        }
        
        [TestMethod]
        public void CanAddShields()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            int initialShield = outpost.GetShields();
            outpost.AddShield(1);
            Assert.AreEqual(initialShield + 1, outpost.GetShields());
        }
        
        [TestMethod]
        public void CanSetShields()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            outpost.SetShields(1);
            Assert.AreEqual(1, outpost.GetShields());
        }
        
        [TestMethod]
        public void ShieldCapacityWorks()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            outpost.SetShieldCapacity(100);
            outpost.SetShields(5);
            outpost.AddShield(100);
            
            Assert.AreEqual(outpost.GetShieldCapacity(), outpost.GetShields());
            
            outpost.SetShields(105);
            Assert.AreEqual(outpost.GetShieldCapacity(), outpost.GetShields());
        }
        
        [TestMethod]
        public void CannotHaveNegativeShield()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            outpost.SetShields(10);
            int initialShields = outpost.GetShields();
            outpost.RemoveShields(15);
            Assert.AreEqual(0, outpost.GetShields());
        }

        [TestMethod]
        public void CanToggleSheilds()
        {
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            bool initialState = outpost.IsShieldActive();
            outpost.ToggleShield();
            Assert.AreEqual(!initialState, outpost.IsShieldActive());
        }

        [TestMethod]
        public void CanSeeLocationInVision()
        {
            
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            Outpost outpost2 = new Outpost(new RftVector(_map, Constants.BASE_OUTPOST_VISION_RADIUS - 1, 0), new Player("2"), OutpostType.Mine);
            Assert.IsTrue(outpost.isInVisionRange(outpost2));
        }
        
        [TestMethod]
        public void CannotSeeLocationOutOfVision()
        {
            
            Outpost outpost = new Outpost(new RftVector(_map, 0, 0), new Player("1"), OutpostType.Mine);
            Outpost outpost2 = new Outpost(new RftVector(_map, Constants.BASE_OUTPOST_VISION_RADIUS + 1, 0), new Player("2"), OutpostType.Mine);
            Assert.IsFalse(outpost.isInVisionRange(outpost2));
        }
    }
}
