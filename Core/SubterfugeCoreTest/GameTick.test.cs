using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using System;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class GameTickTest
    {
        DateTime _time;
        int _tickNumber;
        GameTick _tick;


        [TestInitialize]
        public void Setup()
        {
            _time = DateTime.Now;
            _tickNumber = 0;
            _tick = new GameTick(_tickNumber);
            Game server = new Game();

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

            Assert.AreEqual(_time.AddMinutes(GameTick.MINUTES_PER_TICK), nextTick.GetDate(_time));
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

            Assert.AreEqual(_time.AddMinutes(GameTick.MINUTES_PER_TICK * ticksToAdvance), tenMoreTicks.GetDate(_time));
            Assert.AreEqual(_tickNumber + ticksToAdvance, tenMoreTicks.GetTick());
        }

        [TestMethod]
        public void Rewind()
        {
            int ticksToReverse = 10;
            // Advance and then go back
            GameTick startingTick = _tick.Advance(ticksToReverse).Rewind(ticksToReverse);

            Assert.AreEqual(_time, startingTick.GetDate(_time));
            Assert.AreEqual(_tickNumber, startingTick.GetTick());
        }

        [TestMethod]
        public void FromTick()
        {
            int numberOfTicks = 4;
            double minutes = GameTick.MINUTES_PER_TICK * numberOfTicks;
            DateTime newDate = _time.AddMinutes(minutes);
            GameTick newTick = new GameTick(numberOfTicks);

            Assert.AreEqual(_tick.GetTick() + numberOfTicks, newTick.GetTick());
            Assert.AreEqual(newDate.ToLongTimeString(), newTick.GetDate(_time).ToLongTimeString());
            Assert.AreEqual(newDate.ToLongDateString(), newTick.GetDate(_time).ToLongDateString());
        }

        [TestMethod]
        public void FasterGameSpeed()
        {
            GameTick.MINUTES_PER_TICK = 0.1;
            DateTime start = _tick.GetDate(_time);
            GameTick forward = _tick.Advance(10);
            Assert.AreEqual(forward.GetDate(_time).ToLongTimeString(), start.AddMinutes(GameTick.MINUTES_PER_TICK * 10).ToLongTimeString());
            GameTick.MINUTES_PER_TICK = 15.0;
        }

    }
}
