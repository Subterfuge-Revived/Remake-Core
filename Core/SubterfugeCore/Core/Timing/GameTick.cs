using System;

namespace SubterfugeCore.Core.Timing
{
    /// <summary>
    /// GameTick class to more easily go to and from the current DateTime/TimeStamp to in-game ticks
    /// </summary>
    public class GameTick
    {
        protected bool Equals(GameTick other)
        {
            return _tickNumber == other._tickNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GameTick) obj);
        }

        public static double MINUTES_PER_TICK = 15.0;
        private int _tickNumber;

        /// <summary>
        /// GameTick constructor. 
        /// </summary>
        /// <param name="tickNumber">The integer number of the tick</param>
        public GameTick(int tickNumber) {
            this._tickNumber = tickNumber;
        }

        public GameTick(DateTime startTime, DateTime currentTime)
        {
            TimeSpan dateDelta = currentTime.Subtract(startTime);
            // Get minutes elapsed
            double minutesElapsed = dateDelta.TotalMinutes;
            // Determine the number of ticks past
            int ticksElapsed = (int)Math.Ceiling(minutesElapsed / MINUTES_PER_TICK);

            // Return a new gametick relative to the start time.
            this._tickNumber = ticksElapsed;
        }

        /// <summary>
        /// Generic GameTick constructor. Sets the GameTick to 0.
        /// </summary>
        public GameTick()
        {
            this._tickNumber = 0;
        }

        /// <summary>
        /// Gets the next GameTick
        /// </summary>
        /// <returns>The next GameTick</returns>
        public GameTick GetNextTick()
        {
            return new GameTick(this._tickNumber + 1);
        }

        /// <summary>
        /// Gets the previous GameTick
        /// </summary>
        /// <returns>The previous GameTick</returns>
        public GameTick GetPreviousTick()
        {
            if (this._tickNumber > 0)
            {
                return new GameTick(this._tickNumber - 1);
            }
            return this;
        }

        /// <summary>
        /// Returns a gametick that is a set number of ticks in the future
        /// </summary>
        /// <param name="ticks">The number of ticks in the future</param>
        /// <returns>A GameTick in the future</returns>
        public GameTick Advance(int ticks)
        {
            return new GameTick(this._tickNumber + ticks);
        }

        /// <summary>
        /// Returns a gametick that is a set number of ticks in the past
        /// </summary>
        /// <param name="ticks">The number of ticks in the past</param>
        /// <returns>A GameTick in the past</returns>
        public GameTick Rewind(int ticks)
        {
            return new GameTick(this._tickNumber - ticks);
        }

        /// <summary>
        /// Returns the DateTime that the tick started at
        /// </summary>
        /// <returns>the tick's start DateTime</returns>
        public DateTime GetDate(DateTime startTime)
        {
            return startTime.AddMinutes(this._tickNumber * MINUTES_PER_TICK);
        }

        /// <summary>
        /// Returns the tick number
        /// </summary>
        /// <returns>Tick number</returns>
        public int GetTick()
        {
            return this._tickNumber;
        }
        

        /*
         * Operator overloads.
         * Allows performing things like:
         * GameTick1 > GameTick2
         * etc.
         */
        public static bool operator > (GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() > secondArg.GetTick();
        }

        public static bool operator >=(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() >= secondArg.GetTick();
        }

        public static bool operator <(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() < secondArg.GetTick();
        }

        public static bool operator <=(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() <= secondArg.GetTick();
        }
        
        public static bool operator ==(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() == secondArg.GetTick();
        }
        
        public static bool operator !=(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() != secondArg.GetTick();
        }

        public static int operator -(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() - secondArg.GetTick();
        }

        public static int operator +(GameTick firstArg, GameTick secondArg)
        {
            return firstArg.GetTick() + secondArg.GetTick();
        }

    }
}
