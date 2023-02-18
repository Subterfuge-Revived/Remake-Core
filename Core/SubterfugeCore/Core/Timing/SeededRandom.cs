using System;

namespace Subterfuge.Remake.Core.Timing
{
    /// <summary>
    /// Seeded random class to ensure that all randomization within the game is done from the same randomizer.
    /// This ensures that all randomly calculated values are calculated the same for all users.
    /// </summary>
    public class SeededRandom
    {
        private Random _generator;
        private int _currentRand;

        /// <summary>
        /// Seeded random constructor.
        /// </summary>
        /// <param name="seed">The seed to generate random numbers from</param>
        public SeededRandom(int seed)
        {
            this._generator = new Random(seed);
            this._currentRand = 0;
        }

        /// <summary>
        /// Gets a random number between min and max
        /// </summary>
        /// <param name="min">The minimum</param>
        /// <param name="max">The maximum</param>
        /// <returns>A random number between min and max</returns>
        public int NextRand(int min, int max)
        {
            this._currentRand += 1;
            return this._generator.Next(min, max);
        }
        
        /// <summary>
        /// Generates a double between 0.0 and 1.0
        /// </summary>
        /// <returns>A random double</returns>
        public double NextDouble()
        {
            this._currentRand += 1;
            return this._generator.NextDouble();
        }

        /// <summary>
        /// Gets the n'th random number between min and max
        /// </summary>
        /// <param name="index">The n'th number to generate</param>
        /// <param name="min">The minimum</param>
        /// <param name="max">The maximum</param>
        /// <returns>The n'th random number between min and max</returns>
        public int GetRand(int index, int min, int max)
        {
            // Generate random numbers until you have generated N
            while(this._currentRand < index)
            {
                this._generator.NextDouble();
                this._currentRand++;
            }
            // Return the n'th number
            return this._generator.Next(min, max);
        }

        /// <summary>
        /// Gets the N'th double
        /// </summary>
        /// <param name="index">The N'th number to return</param>
        /// <returns>The N'th double</returns>
        public double GetDouble(int index)
        {
            while (this._currentRand < index)
            {
                this._generator.NextDouble();
                this._currentRand++;
            }
            return this._generator.NextDouble();
        }

    }
}
