using System;

namespace SubterfugeCore.Core.Timing
{
    /// <summary>
    /// GameTick class to more easily go to and from the current DateTime/TimeStamp to in-game ticks
    /// </summary>
    public class GameTick
    {
        public const int MinutesPerTick = 15;
        private DateTime _startTime;
        private int _tickNumber;

        /// <summary>
        /// GameTick constructor. Requires a DateTime and an integer tick number
        /// </summary>
        /// <param name="startTime">The DateTime that the tick starts</param>
        /// <param name="tickNumber">The integer number of the tick</param>
        public GameTick(DateTime startTime, int tickNumber) {
            this._startTime = startTime;
            this._tickNumber = tickNumber;
        }

        public GameTick(DateTime startTime, DateTime currentTime)
        {
            this._startTime = startTime;
            TimeSpan dateDelta = currentTime.Subtract(startTime);
            // Get minutes elapsed
            double minutesElapsed = dateDelta.TotalMinutes;
            // Determine the number of ticks past
            int ticksElapsed = (int)Math.Ceiling(minutesElapsed / GameTick.MinutesPerTick);

            // Return a new gametick relative to the start time.
            this._tickNumber = ticksElapsed;
        }

        /// <summary>
        /// Generic GameTick constructor. Sets tick 0 at the current time.
        /// </summary>
        public GameTick()
        {
            this._startTime = DateTime.Now;
            this._tickNumber = 0;
        }
        
        /// <summary>
        /// Static method to create a GameTick from a DateTime
        /// </summary>
        /// <param name="dateTime">The current DateTime</param>
        /// <returns>The current GameTick relative to the TimeMachine's start time</returns>
        
        public static GameTick FromDate(DateTime dateTime)
        {
            // Determine the delta between the game's start and the passed datetime
            TimeSpan dateDelta = dateTime.Subtract(Game.TimeMachine.GetState().GetStartTick().GetDate());
            // Get minutes elapsed
            double minutesElapsed = dateDelta.TotalMinutes;
            // Determine the number of ticks past
            int ticksElapsed = (int)Math.Ceiling(minutesElapsed / GameTick.MinutesPerTick);

            // Return a new gametick relative to the start time.
            return Game.TimeMachine.GetState().GetStartTick().Advance(ticksElapsed);
        }

        /// <summary>
        /// Static method to create a GameTick from a specific tick number.
        /// </summary>
        /// <param name="tickNumber">The tick to get a GameTick for</param>
        /// <returns>A GameTick relative to the start time of the game</returns>
        public static GameTick FromTickNumber(int tickNumber)
        {
            return Game.TimeMachine.GetState().GetStartTick().Advance(tickNumber);
        }

        /// <summary>
        /// Gets the next GameTick
        /// </summary>
        /// <returns>The next GameTick</returns>
        public GameTick GetNextTick()
        {
            return new GameTick(this._startTime.AddMinutes(MinutesPerTick), this._tickNumber + 1);
        }

        /// <summary>
        /// Gets the previous GameTick
        /// </summary>
        /// <returns>The previous GameTick</returns>
        public GameTick GetPreviousTick()
        {
            if (this._tickNumber > 0)
            {
                return new GameTick(this._startTime.AddMinutes(-MinutesPerTick), this._tickNumber - 1);
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
            return new GameTick(this._startTime.AddMinutes(ticks * MinutesPerTick), this._tickNumber + ticks);
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
                return new GameTick(this._startTime.AddMinutes(ticks * -MinutesPerTick), this._tickNumber - ticks);
            }
            return this.Rewind(this.GetTick());
        }

        /// <summary>
        /// Returns the DateTime that the tick started at
        /// </summary>
        /// <returns>the tick's start DateTime</returns>
        public DateTime GetDate()
        {
            return this._startTime;
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
