using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

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
        
        [TestMethod]
        public void canRemoveDrillers()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            int initialDrillers = outpost.getDrillerCount();
            outpost.removeDrillers(40);
            Assert.AreEqual(initialDrillers - 40, outpost.getDrillerCount());
        }
        
        [TestMethod]
        public void canAddDrillers()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            int initialDrillers = outpost.getDrillerCount();
            outpost.addDrillers(40);
            Assert.AreEqual(initialDrillers + 40, outpost.getDrillerCount());
        }
        
        [TestMethod]
        public void canSetDrillerCount()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            outpost.setDrillerCount(420);
            Assert.AreEqual(420, outpost.getDrillerCount());
        }
        
        [TestMethod]
        public void canLaunchSubs()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Game game = new Game(config);
            
            int initialDrillers = outpost.getDrillerCount();
            outpost.launchSub(10, new Outpost(new Vector2(0,0), new Player(1), OutpostType.MINE));
            
            Assert.AreEqual(initialDrillers - 10, outpost.getDrillerCount());
            Assert.AreEqual(1, Game.timeMachine.getState().getSubList().Count);
        }
        
        [TestMethod]
        public void canUndoSubLaunch()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Game game = new Game(config);
            
            int initialDrillers = outpost.getDrillerCount();
            outpost.launchSub(10, new Outpost(new Vector2(0,0), new Player(1), OutpostType.MINE));
            
            Assert.AreEqual(initialDrillers - 10, outpost.getDrillerCount());
            Assert.AreEqual(1, Game.timeMachine.getState().getSubList().Count);
            
            outpost.undoLaunch(Game.timeMachine.getState().getSubList()[0]);
            
            Assert.AreEqual(initialDrillers, outpost.getDrillerCount());
            Assert.AreEqual(0, Game.timeMachine.getState().getSubList().Count);
        }
        
        [TestMethod]
        public void canRemoveShields()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            outpost.setShields(10);
            int initialShields = outpost.getShields();
            outpost.removeShields(5);
            Assert.AreEqual(initialShields - 5, outpost.getShields());
        }
        
        [TestMethod]
        public void canAddShields()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            int initialShield = outpost.getShields();
            outpost.addShield(1);
            Assert.AreEqual(initialShield + 1, outpost.getShields());
        }
        
        [TestMethod]
        public void canSetShields()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            outpost.setShields(1);
            Assert.AreEqual(1, outpost.getShields());
        }
        
        [TestMethod]
        public void shieldCapacityWorks()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            outpost.setShieldCapacity(100);
            outpost.setShields(5);
            outpost.addShield(100);
            
            Assert.AreEqual(outpost.getShieldCapacity(), outpost.getShields());
            
            outpost.setShields(105);
            Assert.AreEqual(outpost.getShieldCapacity(), outpost.getShields());
        }
        
        [TestMethod]
        public void cannotHaveNegativeShield()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            outpost.setShields(10);
            int initialShields = outpost.getShields();
            outpost.removeShields(15);
            Assert.AreEqual(0, outpost.getShields());
        }

        public void canToggleSheilds()
        {
            Outpost outpost = new Outpost(new Vector2(0, 0), new Player(1), OutpostType.MINE);
            bool initialState = outpost.isShieldActive();
            outpost.toggleShield();
            Assert.AreEqual(!initialState, outpost.isShieldActive());
        }
    }
}
