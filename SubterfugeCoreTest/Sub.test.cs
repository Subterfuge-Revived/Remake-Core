using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using SubterfugeCore;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;
using System;

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
            outpost = new Outpost(location, new Player(1));
            tick = new GameTick(new DateTime(), 10);
            sub = new Sub(outpost, outpost, tick, 0, new Player(1));
            GameServer server = new GameServer();
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
