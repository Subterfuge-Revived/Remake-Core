using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Positions;
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
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class LaunchEventTest
    {
        private Game _game;


        [TestInitialize]
        public void Setup()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            
            GameConfiguration config = new GameConfiguration(players);
            _game = new Game(config);
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.IsNotNull(new LaunchEvent(new GameTick(), new Outpost(new RftVector(new Rft(300, 300), 0, 0)),
	            1, new Outpost(new RftVector(new Rft(300, 300), 0, 0))));
        }

        [TestMethod]
        public void CannotLaunchFromNonGeneratedOutposts()
        {
            Outpost outpost1 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            Game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            Game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(false, launch.ForwardAction());
        }
        
        [TestMethod]
        public void CanLaunchSingleSub()
        {
            Outpost outpost1 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            Game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            Game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch.ForwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
        }
        
        [TestMethod]
        public void CanUndoSubLaunch()
        {
            Outpost outpost1 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            Game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            Game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch.ForwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
            
            Assert.AreEqual(true, launch.BackwardAction());
            Assert.AreEqual(outpostOneInitial, outpost1.GetDrillerCount());
            Assert.AreEqual(outpostTwoInitial, outpost2.GetDrillerCount());
            Assert.AreEqual(0, Game.TimeMachine.GetState().GetSubList().Count);
        }
        
        [TestMethod]
        public void CanGetTheLaunchedSub()
        {
            Outpost outpost1 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            Game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            Game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch.ForwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
            
            // Ensure can get the sub.
            Assert.IsNotNull(Game.TimeMachine.GetState().GetSubList()[0]);
            Assert.AreEqual(1, Game.TimeMachine.GetState().GetSubList()[0].GetDrillerCount());
            Assert.AreEqual(outpost1, Game.TimeMachine.GetState().GetSubList()[0].GetSource());
            Assert.AreEqual(outpost2, Game.TimeMachine.GetState().GetSubList()[0].GetDestination());
        }
        
        [TestMethod]
        public void SubLaunchCreatesCombatEvents()
        {
            Outpost outpost1 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Outpost(new RftVector(new Rft(300, 300), 40, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            outpost1.SetOwner(Game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.SetOwner(Game.TimeMachine.GetState().GetPlayers()[1]);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            Game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            Game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            LaunchEvent launch2 = new LaunchEvent(new GameTick(), outpost2, 1, outpost1);
            Assert.AreEqual(true, launch1.ForwardAction());
            Assert.AreEqual(true, launch2.ForwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(2, Game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
            Assert.AreEqual(outpostTwoInitial - 1, outpost2.GetDrillerCount());
            
            // Ensure a combat event has been added that includes both subs.
            int subToSubBattles = 0;
            int subToOutpostBattles = 0;
            GameEvent arriveEvent = null;
            CombatEvent combatEvent = null;
            foreach(GameEvent gameEvent in Game.TimeMachine.GetQueuedEvents())
            {
                if (gameEvent is CombatEvent)
                {
                    combatEvent = (CombatEvent) gameEvent;
                    if (combatEvent.GetCombatants()[0] is Sub && combatEvent.GetCombatants()[1] is Sub)
                    {
                        subToSubBattles++;
                    } else
                    {
                        subToOutpostBattles++;
                        arriveEvent = gameEvent;
                    }
                }
            }
            // There should be 3 combats, one on each outpost, one on both subs.
            Assert.AreEqual(1, subToSubBattles);
            Assert.AreEqual(2, subToOutpostBattles);
        }
        
        [TestMethod]
        public void SubsArriveAfterLaunch()
        {
            Outpost outpost1 = new Outpost(new RftVector(new Rft(300, 300), 0, 0));
            Outpost outpost2 = new Outpost(new RftVector(new Rft(300, 300), 40, 0));
            outpost1.SetDrillerCount(10);
            outpost2.SetDrillerCount(10);
            outpost1.SetOwner(Game.TimeMachine.GetState().GetPlayers()[0]);
            outpost2.SetOwner(Game.TimeMachine.GetState().GetPlayers()[0]);
            int outpostOneInitial = outpost1.GetDrillerCount();
            int outpostTwoInitial = outpost2.GetDrillerCount();
            
            Game.TimeMachine.GetState().GetOutposts().Add(outpost1);
            Game.TimeMachine.GetState().GetOutposts().Add(outpost2);
            
            LaunchEvent launch1 = new LaunchEvent(new GameTick(), outpost1, 1, outpost2);
            Assert.AreEqual(true, launch1.ForwardAction());
            
            // Ensure the sub was launched, outpost lost drillers, etc.
            Assert.AreEqual(1, Game.TimeMachine.GetState().GetSubList().Count);
            Assert.AreEqual(outpostOneInitial - 1, outpost1.GetDrillerCount());
            
            // Ensure a combat event has been added.
            int combatEvents = 0;
            GameEvent combat = null;
            foreach (GameEvent gameEvent in Game.TimeMachine.GetQueuedEvents())
            {
                if (gameEvent is CombatEvent)
                {
                    combat = gameEvent;
                    combatEvents++;
                }
            }
            Assert.AreEqual(1, combatEvents);
            
            // Go to the event and ensure the arrival is successful
            Game.TimeMachine.GoTo(combat);
            Game.TimeMachine.Advance(1);
            
            Assert.AreEqual(true, combat.WasEventSuccessful());
            Assert.AreEqual(outpost1.GetOwner(), outpost2.GetOwner());
            Assert.AreEqual(outpostTwoInitial + 1, outpost2.GetDrillerCount());
        }
    }
}
