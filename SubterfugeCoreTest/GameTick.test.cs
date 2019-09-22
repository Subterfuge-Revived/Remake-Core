using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class GameTickTest
    {
        DateTime time;
        int tickNumber;
        GameTick tick;


        [TestInitialize]
        public void setup()
        {
            time = new DateTime();
            tickNumber = 0;
            tick = new GameTick(time, tickNumber);

        }

        [TestMethod]
        public void constructor()
        {
            Assert.AreEqual(tickNumber, tick.getTick());
            Assert.AreEqual(time, tick.getDate());
        }

        [TestMethod]
        public void getNextTick()
        {
            GameTick nextTick = tick.getNextTick();

            Assert.AreEqual(time.AddMinutes(GameTick.MINUTES_PER_TICK), nextTick.getDate());
            Assert.AreEqual(tickNumber + 1, nextTick.getTick());
        }

        [TestMethod]
        public void getPreviousTick()
        {
            // Check cannot go back before the first tick
            GameTick previousTick = tick.getPreviousTick();

            Assert.AreEqual(null, previousTick);

            // Advance and then come back
            GameTick startingTick = tick.getNextTick().getPreviousTick();

            Assert.AreEqual(time, startingTick.getDate());
            Assert.AreEqual(tickNumber, startingTick.getTick());
        }

        [TestMethod]
        public void advance()
        {
            // Check advancing N ticks
            int ticksToAdvance = 10;
            GameTick TenMoreTicks = tick.advance(ticksToAdvance);

            Assert.AreEqual(time.AddMinutes(GameTick.MINUTES_PER_TICK * ticksToAdvance), TenMoreTicks.getDate());
            Assert.AreEqual(tickNumber + ticksToAdvance, TenMoreTicks.getTick());
        }

        [TestMethod]
        public void rewind()
        {
            int ticksToReverse = 10;
            GameTick TenTicksBefore = tick.rewind(ticksToReverse);

            // Should not be able to reverse 
            Assert.AreEqual(null, TenTicksBefore);

            // Advance and then go back
            GameTick startingTick = tick.advance(ticksToReverse).rewind(ticksToReverse);

            Assert.AreEqual(time, startingTick.getDate());
            Assert.AreEqual(tickNumber, startingTick.getTick());

        }

        [TestMethod]
        public void fromDate()
        {
            int numberOfTicks = 4;
            int minutes = GameTick.MINUTES_PER_TICK * numberOfTicks;
            DateTime newDate = time.AddMinutes(minutes);
            GameTick newTick = GameTick.fromDate(newDate);

            Assert.AreEqual(tick.getTick() + numberOfTicks, newTick.getTick());
            Assert.AreEqual(newDate, newTick.getDate());
        }

        [TestMethod]
        public void fromTick()
        {
            int numberOfTicks = 4;
            int minutes = GameTick.MINUTES_PER_TICK * numberOfTicks;
            DateTime newDate = time.AddMinutes(minutes);
            GameTick newTick = GameTick.fromTickNumber(numberOfTicks);

            Assert.AreEqual(tick.getTick() + numberOfTicks, newTick.getTick());
            Assert.AreEqual(newDate, newTick.getDate());

        }

    }
}
