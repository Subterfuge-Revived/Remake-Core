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
    public class TimeMachineTest
    {
        TimeMachine timeMachine;
        GameState state;


        [TestInitialize]
        public void setup()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            state = new GameState(config);
            timeMachine = new TimeMachine(state);

        }

        [TestMethod]
        public void constructor()
        {
            Assert.AreEqual(state, timeMachine.getState());
        }

        [TestMethod]
        public void addEvent()
        {
            Player player1 = new Player(1);
            Outpost outpost = new Outpost(new Vector2(0, 0), player1, OutpostType.GENERATOR);
            outpost.addDrillers(10);
            Sub sub = new Sub(outpost, outpost, new GameTick(), 10, player1);
            CombatEvent arriveEvent = new CombatEvent(sub, outpost, new GameTick(), outpost.getTargetLocation(sub.getCurrentLocation(), sub.getSpeed()));

            timeMachine.addEvent(arriveEvent);
        }

    }
}
