using System;

namespace SubterfugeCore.Core.Timing
{
    /// <summary>
    /// GameTick class to more easily go to and from the current DateTime/TimeStamp to in-game ticks
    /// </summary>
    public class GameTick
    {
        public const int MINUTES_PER_TICK = 15;
        private DateTime startTime;
        private int tickNumber;

        /// <summary>
        /// GameTick constructor. Requires a DateTime and an integer tick number
        /// </summary>
        /// <param name="startTime">The DateTime that the tick starts</param>
        /// <param name="tickNumber">The integer number of the tick</param>
        public GameTick(DateTime startTime, int tickNumber) {
            this.startTime = startTime;
            this.tickNumber = tickNumber;
        }

        /// <summary>
        /// Generic GameTick constructor. Sets tick 0 at the current time.
        /// </summary>
        public GameTick()
        {
            this.startTime = new DateTime();
            this.tickNumber = 0;
        }
        
        /// <summary>
        /// Static method to create a GameTick from a DateTime
        /// </summary>
        /// <param name="dateTime">The current DateTime</param>
        /// <returns>The current GameTick relative to the TimeMachine's start time</returns>
        
        public static GameTick fromDate(DateTime dateTime)
        {
            // Determine the delta between the game's start and the passed datetime
            TimeSpan dateDelta = dateTime.Subtract(Game.timeMachine.getState().getStartTick().getDate());
            // Get minutes elapsed
            double minutesElapsed = dateDelta.TotalMinutes;
            // Determine the number of ticks past
            int ticksElapsed = (int)Math.Ceiling(minutesElapsed / GameTick.MINUTES_PER_TICK);

            // Return a new gametick relative to the start time.
            return Game.timeMachine.getState().getStartTick().advance(ticksElapsed);
        }

        /// <summary>
        /// Static method to create a GameTick from a specific tick number.
        /// </summary>
        /// <param name="tickNumber">The tick to get a GameTick for</param>
        /// <returns>A GameTick relative to the start time of the game</returns>
        public static GameTick fromTickNumber(int tickNumber)
        {
            return Game.timeMachine.getState().getStartTick().advance(tickNumber);
        }

        /// <summary>
        /// Gets the next GameTick
        /// </summary>
        /// <returns>The next GameTick</returns>
        public GameTick getNextTick()
        {
            return new GameTick(this.startTime.AddMinutes(MINUTES_PER_TICK), this.tickNumber + 1);
        }

        /// <summary>
        /// Gets the previous GameTick
        /// </summary>
        /// <returns>The previous GameTick</returns>
        public GameTick getPreviousTick()
        {
            if (this.tickNumber > 0)
            {
                return new GameTick(this.startTime.AddMinutes(-MINUTES_PER_TICK), this.tickNumber - 1);
            }
            return this;
        }

        /// <summary>
        /// Returns a gametick that is a set number of ticks in the future
        /// </summary>
        /// <param name="ticks">The number of ticks in the future</param>
        /// <returns>A GameTick in the future</returns>
        public GameTick advance(int ticks)
        {
            return new GameTick(this.startTime.AddMinutes(ticks * MINUTES_PER_TICK), this.tickNumber + ticks);
        }

        /// <summary>
        /// Returns a gametick that is a set number of ticks in the past
        /// </summary>
        /// <param name="ticks">The number of ticks in the past</param>
        /// <returns>A GameTick in the past</returns>
        public GameTick rewind(int ticks)
        {
            if (this.tickNumber >= ticks)
            {
                return new GameTick(this.startTime.AddMinutes(ticks * -MINUTES_PER_TICK), this.tickNumber - ticks);
            }
            return this.rewind(this.getTick());
        }

        /// <summary>
        /// Returns the DateTime that the tick started at
        /// </summary>
        /// <returns>the tick's start DateTime</returns>
        public DateTime getDate()
        {
            return this.startTime;
        }

        /// <summary>
        /// Returns the tick number
        /// </summary>
        /// <returns>Tick number</returns>
        public int getTick()
        {
            return this.tickNumber;
        }
        
        

        /*
         * Operator overloads.
         * Allows performing things like:
         * GameTick1 > GameTick2
         * etc.
         */
        public static bool operator > (GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() > secondArg.getTick();
        }

        public static bool operator >=(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() >= secondArg.getTick();
        }

        public static bool operator <(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() < secondArg.getTick();
        }

        public static bool operator <=(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() <= secondArg.getTick();
        }
        
        public static bool operator ==(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() == secondArg.getTick();
        }
        
        public static bool operator !=(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() != secondArg.getTick();
        }

        public static int operator -(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() - secondArg.getTick();
        }

        public static int operator +(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.getTick() + secondArg.getTick();
        }

    }
}
