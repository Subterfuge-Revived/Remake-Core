using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Timing;
using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class LaunchEventTest
    {
        private Game game;


        [TestInitialize]
        public void setup()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            config.seed = 1234;
            config.outpostsPerPlayer = 1;
            config.dormantsPerPlayer = 1;
            
            game = new Game(config);
        }

        [TestMethod]
        public void constructor()
        {
            Assert.IsNotNull(new LaunchEvent(new GameTick(), new Outpost(new Vector2(0, 0)), 1, new Outpost(new Vector2(0, 0))));
        }

        [TestMethod]
        public void CannotLaunchFromNonGeneratedOutposts()
        {
            Outpost outpost1 = new Outpost(new Vector2(0, 0));
            Outpost outpost2 = new Outpost(new Vector2(0, 0));
            int outpostOneInitial = outpost1.getDrillerCount();
            int outpostTwoInitial = outpost2.getDrillerCount();
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(false, launch.forwardAction());
        }
        
        [TestMethod]
        public void CanLaunchSingleSub()
        {
            Outpost outpost1 = Game.timeMachine.getState().getOutposts()[0];
            Outpost outpost2 = Game.timeMachine.getState().getOutposts()[1];
            int outpostOneInitial = outpost1.getDrillerCount();
            int outpostTwoInitial = outpost2.getDrillerCount();
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch.forwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.timeMachine.getState().getSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.getDrillerCount());
        }
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            Outpost outpost1 = Game.timeMachine.getState().getOutposts()[0];
            Outpost outpost2 = Game.timeMachine.getState().getOutposts()[1];
            int outpostOneInitial = outpost1.getDrillerCount();
            int outpostTwoInitial = outpost2.getDrillerCount();
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch.forwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.timeMachine.getState().getSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.getDrillerCount());
            
            Assert.AreEqual(true, launch.backwardAction());
            Assert.AreEqual(outpostOneInitial, outpost1.getDrillerCount());
            Assert.AreEqual(outpostTwoInitial, outpost2.getDrillerCount());
            Assert.AreEqual(0, Game.timeMachine.getState().getSubList().Count);
        }
        
        [TestMethod]
        public void CanGetTheLaunchedSub()
        {
            Outpost outpost1 = Game.timeMachine.getState().getOutposts()[0];
            Outpost outpost2 = Game.timeMachine.getState().getOutposts()[1];
            int outpostOneInitial = outpost1.getDrillerCount();
            int outpostTwoInitial = outpost2.getDrillerCount();
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch.forwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.timeMachine.getState().getSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.getDrillerCount());
            
            // Ensure can get the sub.
            Assert.IsNotNull(Game.timeMachine.getState().getSubList()[0]);
            Assert.AreEqual(1, Game.timeMachine.getState().getSubList()[0].getDrillerCount());
            Assert.AreEqual(outpost1, Game.timeMachine.getState().getSubList()[0].getSource());
            Assert.AreEqual(outpost2, Game.timeMachine.getState().getSubList()[0].getDestination());
        }

    }
}
