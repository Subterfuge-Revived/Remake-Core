using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeRemake.Shared.Content.Game.Core.Timing
{
    class GameTick
    {
        private const int MINUTES_PER_TICK = 15;
        private DateTime startTime;
        private int tickNumber;

        public GameTick(DateTime startTime, int tickNumber) {
            this.startTime = startTime;
            this.tickNumber = tickNumber;
        }

        public GameTick getNextTick()
        {
            return new GameTick(this.startTime.AddMinutes(MINUTES_PER_TICK), this.tickNumber + 1);
        }

        public GameTick getPreviousTick()
        {
            return new GameTick(this.startTime.AddMinutes(-MINUTES_PER_TICK), this.tickNumber - 1);
        }

        public DateTime getTickStartTime()
        {
            return this.startTime;
        }

        public int getTickNumber()
        {
            return this.tickNumber;
        }

        public static bool operator > (GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTickNumber() > secondArg.getTickNumber();
        }

        public static bool operator <(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTickNumber() < secondArg.getTickNumber();
        }

    }
}
