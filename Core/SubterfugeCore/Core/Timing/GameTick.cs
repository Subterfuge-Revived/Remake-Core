using System;
using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Core.Timing
{
    /// <summary>
    /// GameTick class to more easily go to and from the current DateTime/TimeStamp to in-game ticks
    /// </summary>
    public class GameTick
    {
        public override int GetHashCode()
        {
            return _tickNumber;
        }

        protected bool Equals(GameTick other)
        {
            return _tickNumber == other._tickNumber;
        }

        public override bool Equals(object obj)
        {
            GameTick asTick = obj as GameTick;
            if (asTick == null)
                return false;
            
            return Equals((GameTick) obj);
        }

        public static double MinutesPerTick { get; set; }= 15.0;
        private readonly int _tickNumber;

        /// <summary>
        /// GameTick constructor. Requires a DateTime and an integer tick number
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
            int ticksElapsed = (int)Math.Ceiling(minutesElapsed / MinutesPerTick);

            // Return a new gametick relative to the start time.
            this._tickNumber = ticksElapsed;
        }

        /// <summary>
        /// Generic GameTick constructor. Sets tick 0 at the current time.
        /// </summary>
        public GameTick()
        {
            this._tickNumber = 0;
        }

        public static GameTick fromGameConfiguration(GameConfiguration config)
        {
            GameTick.MinutesPerTick = config.GameSettings.MinutesPerTick;
            return new GameTick(config.TimeStarted, DateTime.UtcNow);
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
            if (this._tickNumber >= ticks)
            {
                return new GameTick(this._tickNumber - ticks);
            }
            return this.Rewind(this.GetTick());
        }

        /// <summary>
        /// Returns the DateTime that the tick started at
        /// </summary>
        /// <returns>the tick's start DateTime</returns>
        public DateTime GetDate(DateTime startTime)
        {
            return startTime.AddMinutes(this._tickNumber * MinutesPerTick);
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
