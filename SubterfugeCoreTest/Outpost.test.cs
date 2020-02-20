using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class OutpostTest
    {
        Vector2 outpostLocation;
        Outpost outpost;

        [TestInitialize]
        public void setup()
        {
            this.outpostLocation = new Vector2(0, 0);
            this.outpost = new Outpost(outpostLocation, new Player(1), OutpostType.GENERATOR);
        }

        [TestMethod]
        public void getPosition()
        {
            Assert.AreEqual(outpostLocation.X, outpost.getPosition().X);
            Assert.AreEqual(outpostLocation.Y, outpost.getPosition().Y);
        }
        [TestMethod]
        public void getTargetLocation()
        {
            // Outpost target location should always be the outpost's location
            Assert.AreEqual(outpostLocation.X, outpost.getTargetLocation(new Vector2(0, 0), 0.25).X);
            Assert.AreEqual(outpostLocation.Y, outpost.getTargetLocation(new Vector2(0, 0), 0.25).Y);

            Assert.AreEqual(outpostLocation.X, outpost.getTargetLocation(new Vector2(100, 100), 1).X);
            Assert.AreEqual(outpostLocation.Y, outpost.getTargetLocation(new Vector2(100, 100), 1).Y);

            Assert.AreEqual(outpostLocation.X, outpost.getTargetLocation(new Vector2(999, 999), 999).X);
            Assert.AreEqual(outpostLocation.Y, outpost.getTargetLocation(new Vector2(999, 999), 999).Y);
        }

        [TestMethod]
        public void isITargetable()
        {
            Assert.IsTrue(typeof(ITargetable).IsAssignableFrom(outpost.GetType()));
        }

        [TestMethod]
        public void isOwnable()
        {
            Assert.IsTrue(typeof(IOwnable).IsAssignableFrom(outpost.GetType()));
        }
    }
}
