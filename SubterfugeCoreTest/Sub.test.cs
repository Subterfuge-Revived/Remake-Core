using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Locations;
using System;
using System.Numerics;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class SubTest
    {
        Vector2 location;
        Outpost outpost;
        GameTick tick;
        Sub sub;

        [TestInitialize]
        public void setup()
        {
            location = new Vector2(0, 0);
            outpost = new Outpost(location, new Player(1), OutpostType.GENERATOR);
            tick = new GameTick(new DateTime(), 10);
            sub = new Sub(outpost, outpost, tick, 0, new Player(1));
            Game server = new Game();
        }

        [TestMethod]
        public void constructor()
        {
            Assert.AreEqual(location.X, sub.getInitialPosition().X);
            Assert.AreEqual(location.Y, sub.getInitialPosition().Y);
            Assert.AreEqual(location.X, sub.getDestinationLocation().X);
            Assert.AreEqual(location.Y, sub.getDestinationLocation().Y);
            Assert.AreEqual(tick, sub.getLaunchTick());
            Assert.AreEqual(0, sub.getDrillerCount());
        }

        [TestMethod]
        public void isIOwnable()
        {
            Assert.IsTrue(typeof(IOwnable).IsAssignableFrom(sub.GetType()));
        }

        [TestMethod]
        public void isITargetable()
        {
            Assert.IsTrue(typeof(ITargetable).IsAssignableFrom(sub.GetType()));
        }
    }
}
