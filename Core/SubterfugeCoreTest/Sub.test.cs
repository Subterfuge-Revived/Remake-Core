using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Positions;
using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class SubTest
    {
	    Rft _map;
        RftVector _location;
        Outpost _outpost;
        GameTick _tick;
        Sub _sub;
        TestUtils testUtils = new TestUtils();
        Player player = new Player("1");
        private Game game;

        [TestInitialize]
        public void Setup()
        {
	        _map = new Rft(3000, 3000);
            _location = new RftVector(_map, 0, 0);
            _outpost = new Generator("0", _location, new Player("1"));
            _tick = new GameTick(10);
            _sub = new Sub("0", _outpost, _outpost, _tick, 0, new Player("1"));
            Game server = new Game(testUtils.GetDefaultGameConfiguration(new List<Player> { player }));
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.AreEqual(_location.X, _sub.GetInitialPosition().X);
            Assert.AreEqual(_location.Y, _sub.GetInitialPosition().Y);
            Assert.AreEqual(_location.X, _sub.GetDestinationLocation().X);
            Assert.AreEqual(_location.Y, _sub.GetDestinationLocation().Y);
            Assert.AreEqual(_tick, _sub.GetLaunchTick());
            Assert.AreEqual(0, _sub.GetDrillerCount());
        }

        [TestMethod]
        public void IsIOwnable()
        {
            Assert.IsTrue(typeof(IOwnable).IsAssignableFrom(_sub.GetType()));
        }

        [TestMethod]
        public void IsITargetable()
        {
            Assert.IsTrue(typeof(ITargetable).IsAssignableFrom(_sub.GetType()));
        }

        [TestMethod]
        public void CanSeeLocationInVisionRange()
        {
            RftVector currentLocation = _sub.GetCurrentPosition(new GameTick(1));
            RftVector insideVisionRange = new RftVector(currentLocation.X + _sub.GetVisionRange() - 1, currentLocation.Y);
            Outpost insideRange = new Generator("0", insideVisionRange, new Player("1"));
            Assert.IsTrue(_sub.isInVisionRange(new GameTick(1), insideRange));
        }
        
        [TestMethod]
        public void CanNotSeeLocationOutsideVisionRange()
        {
            RftVector currentLocation = _sub.GetCurrentPosition(new GameTick(1));
            RftVector outsideVisionRange = new RftVector(currentLocation.X + _sub.GetVisionRange() + 1, currentLocation.Y);
            Outpost outsideRange = new Generator("0", outsideVisionRange, new Player("1"));
            Assert.IsFalse(_sub.isInVisionRange(new GameTick(1), outsideRange));
        }
    }
}
