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
using SubterfugeCore.Core.GameEvents.Base;
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
            players.Add(new Player(1));;
            players.Add(new Player(2));
            
            GameConfiguration config = new GameConfiguration(players);
            config.seed = 1234;
            config.outpostsPerPlayer = 1;
            config.dormantsPerPlayer = 1;
            config.maxiumumOutpostDistance = 100;
            config.minimumOutpostDistance = 10;
            
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
        
        [TestMethod]
        public void SubLaunchCreatesCombatEvents()
        {
            Outpost outpost1 = Game.timeMachine.getState().getOutposts()[0];
            Outpost outpost2 = Game.timeMachine.getState().getOutposts()[2];
            int outpostOneInitial = outpost1.getDrillerCount();
            int outpostTwoInitial = outpost2.getDrillerCount();
            
            LaunchEvent launch1 = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            LaunchEvent launch2 = new LaunchEvent(new GameTick(), outpost2, 1, outpost1);
            Assert.AreEqual(true, launch1.forwardAction());
            Assert.AreEqual(true, launch2.forwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(2, Game.timeMachine.getState().getSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.getDrillerCount());
            Assert.AreEqual(outpostTwoInitial - 1, outpost2.getDrillerCount());
            
            // Ensure a combat event has been added that includes both subs.
            int subToSubBattles = 0;
            int subToOutpostBattes = 0;
            GameEvent arriveEvent = null;
            CombatEvent combatEvent = null;
            foreach(GameEvent gameEvent in Game.timeMachine.getQueuedEvents())
            {
                if (gameEvent is CombatEvent)
                {
                    combatEvent = (CombatEvent) gameEvent;
                    if (combatEvent.getCombatants()[0] is Sub && combatEvent.getCombatants()[1] is Sub)
                    {
                        subToSubBattles++;
                    } else
                    {
                        subToOutpostBattes++;
                        arriveEvent = gameEvent;
                    }
                }
            }
            // There should be 3 combats, one on each outpost, one on both subs.
            Assert.AreEqual(1, subToSubBattles);
            Assert.AreEqual(2, subToOutpostBattes);
        }
        
        [TestMethod]
        public void SubsArriveAfterLaunch()
        {
            Outpost outpost1 = Game.timeMachine.getState().getOutposts()[0];
            Outpost outpost2 = Game.timeMachine.getState().getOutposts()[1];
            int outpostOneInitial = outpost1.getDrillerCount();
            int outpostTwoInitial = outpost2.getDrillerCount();
            
            LaunchEvent launch1 = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch1.forwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.timeMachine.getState().getSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.getDrillerCount());
            
            // Ensure a combat event has been added.
            int combatEvents = 0;
            GameEvent combat = null;
            foreach (GameEvent gameEvent in Game.timeMachine.getQueuedEvents())
            {
                if (gameEvent is CombatEvent)
                {
                    combat = gameEvent;
                    combatEvents++;
                }
            }
            Assert.AreEqual(1, combatEvents);
            
            // Go to the event and ensure the arrival is successful
            Game.timeMachine.goTo(combat);
            Game.timeMachine.advance(1);
            
            Assert.AreEqual(true, combat.wasEventSuccessful());
            Assert.AreEqual(outpost1.getOwner(), outpost2.getOwner());
            Assert.AreEqual(1, outpost2.getDrillerCount());
        }
    }
}
