using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using SubterfugeCore;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class SubTest
    {
        Vector2 source;
        Outpost destination;
        GameTick tick;
        Sub sub;

        [TestInitialize]
        public void setup()
        {
            source = new Vector2(0, 0);
            destination = new Outpost(source);
            tick = new GameTick(new DateTime(), 10);
            sub = new Sub(source, destination, tick, 0);
        }

        [TestMethod]
        public void constructor()
        {
            Assert.AreEqual(source.X, sub.getInitialPosition().X);
            Assert.AreEqual(source.Y, sub.getInitialPosition().Y);
            Assert.AreEqual(destination.getPosition().X, sub.getDestination().X);
            Assert.AreEqual(destination.getPosition().Y, sub.getDestination().Y);
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
