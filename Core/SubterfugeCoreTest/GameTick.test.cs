using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Test
{
    [TestClass]
    public class GameTickTest
    {
        DateTime _time;
        int _tickNumber;
        GameTick _tick;
        private TestUtils testUtils = new TestUtils();


        [TestInitialize]
        public void Setup()
        {
            _time = DateTime.UtcNow;
            _tickNumber = 0;
            _tick = new GameTick(_tickNumber);
            GameConfiguration config = testUtils.GetDefaultGameConfiguration(new List<Player>{ new Player("1") });
            new Game(config);
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.AreEqual(_tickNumber, _tick.GetTick());
            Assert.AreEqual(_time, _tick.GetDate(_time));
        }

        [TestMethod]
        public void GetNextTick()
        {
            GameTick nextTick = _tick.GetNextTick();

            Assert.AreEqual(_time.AddMinutes(GameTick.MinutesPerTick), nextTick.GetDate(_time));
            Assert.AreEqual(_tickNumber + 1, nextTick.GetTick());
        }

        [TestMethod]
        public void GetPreviousTick()
        {
            // Check cannot go back before the first tick
            GameTick previousTick = _tick.GetPreviousTick();

            Assert.AreEqual(_tick, previousTick);

            // Advance and then come back
            GameTick startingTick = _tick.GetNextTick().GetPreviousTick();

            Assert.AreEqual(_time, startingTick.GetDate(_time));
            Assert.AreEqual(_tickNumber, startingTick.GetTick());
        }

        [TestMethod]
        public void Advance()
        {
            // Check advancing N ticks
            int ticksToAdvance = 10;
            GameTick tenMoreTicks = _tick.Advance(ticksToAdvance);

            Assert.AreEqual(_time.AddMinutes(GameTick.MinutesPerTick * ticksToAdvance), tenMoreTicks.GetDate(_time));
            Assert.AreEqual(_tickNumber + ticksToAdvance, tenMoreTicks.GetTick());
        }

        [TestMethod]
        public void Rewind()
        {
            int ticksToReverse = 10;
            GameTick tenTicksBefore = _tick.Rewind(ticksToReverse);

            // Should not be able to reverse 
            Assert.AreEqual(true, tenTicksBefore == _tick);

            // Advance and then go back
            GameTick startingTick = _tick.Advance(ticksToReverse).Rewind(ticksToReverse);

            Assert.AreEqual(_time, startingTick.GetDate(_time));
            Assert.AreEqual(_tickNumber, startingTick.GetTick());

        }

        [TestMethod]
        public void FromTick()
        {
            int numberOfTicks = 4;
            double minutes = GameTick.MinutesPerTick * numberOfTicks;
            DateTime newDate = _time.AddMinutes(minutes);
            GameTick newTick = new GameTick(numberOfTicks);

            Assert.AreEqual(_tick.GetTick() + numberOfTicks, newTick.GetTick());
            Assert.AreEqual(newDate.ToLongTimeString(), newTick.GetDate(_time).ToLongTimeString());
            Assert.AreEqual(newDate.ToLongDateString(), newTick.GetDate(_time).ToLongDateString());
        }

        [TestMethod]
        public void FasterGameSpeed()
        {
            GameTick.MinutesPerTick = 0.1;
            DateTime start = _tick.GetDate(_time);
            GameTick forward = _tick.Advance(10);
            Assert.AreEqual(forward.GetDate(_time).ToLongTimeString(), start.AddMinutes(GameTick.MinutesPerTick * 10).ToLongTimeString());
        }

    }
}