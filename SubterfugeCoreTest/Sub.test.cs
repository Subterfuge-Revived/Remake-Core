using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Positions;
using System;
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

        [TestInitialize]
        public void Setup()
        {
	        _map = new Rft(3000, 3000);
            _location = new RftVector(_map, 0, 0);
            _outpost = new Outpost(_location, new Player(1), OutpostType.Generator);
            _tick = new GameTick(new DateTime(), 10);
            _sub = new Sub(_outpost, _outpost, _tick, 0, new Player(1));
            Game server = new Game();
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
    }
}
