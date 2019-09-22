using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Timing
{
    public class SeededRandom
    {
        private Random generator;
        private int currentRand;

        public SeededRandom(int seed)
        {
            this.generator = new Random(seed);
            this.currentRand = 0;
        }

        public int nextRand(int min, int max)
        {
            this.currentRand += 1;
            return this.generator.Next(min, max);
        }

        public double nextDouble()
        {
            this.currentRand += 1;
            return this.generator.NextDouble();
        }

        public int getRand(int index, int min, int max)
        {
            while(this.currentRand < index)
            {
                this.generator.NextDouble();
                this.currentRand++;
            }
            return this.generator.Next(min, max);
        }

        public double getDouble(int index)
        {
            while (this.currentRand < index)
            {
                this.generator.NextDouble();
                this.currentRand++;
            }
            return this.generator.NextDouble();
        }

    }
}
