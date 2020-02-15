using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;
using System;
using System.Numerics;

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
            state = new GameState();
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
