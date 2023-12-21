using System;

namespace SOD.Common.Shadows.Implementations
{
    /// <summary>
    /// A Mersenne Twister Random Generator implementation, allows to export and import the state of the random.
    /// </summary>
    public sealed class MersenneTwisterRandom
    {
        /// <summary>
        /// N
        /// </summary>
        private static readonly int N = 624;

        /// <summary>
        /// M
        /// </summary>
        private static readonly int M = 397;

        /// <summary>
        /// Constant vector a
        /// </summary>
        private readonly uint MATRIX_A = 0x9908b0df;

        /// <summary>
        /// most significant w-r bits
        /// </summary>
        private readonly uint UPPER_MASK = 0x80000000;

        /// <summary>
        /// least significant r bits
        /// </summary>
        private readonly uint LOWER_MASK = 0x7fffffff;

        /// <summary>
        /// Tempering mask B
        /// </summary>
        private readonly uint TEMPERING_MASK_B = 0x9d2c5680;

        /// <summary>
        /// Tempering mask C
        /// </summary>
        private readonly uint TEMPERING_MASK_C = 0xefc60000;

        /// <summary>
        /// Last constant used for generation
        /// </summary>
        private readonly double FINAL_CONSTANT = 2.3283064365386963e-10;

        /// <summary>
        /// Generates a new random with seed: DateTime.Now.Ticks
        /// </summary>
        public MersenneTwisterRandom()
        {
            Generate((uint)DateTime.Now.Ticks);
        }

        /// <summary>
        /// Generates a new random based on a seed.
        /// </summary>
        /// <param name="seed"></param>
        public MersenneTwisterRandom(int seed)
        {
            Generate((uint)seed);
        }

        /// <summary>
        /// Initializes a new random based on a previous state.
        /// </summary>
        /// <param name="state"></param>
        public MersenneTwisterRandom((int mti, uint[] mt) state)
        {
            mt = state.mt;
            mti = state.mti;
        }

        private static uint TemperingShiftU(uint y)
        {
            return y >> 11;
        }

        private static uint TemperingShiftS(uint y)
        {
            return y << 7;
        }

        private static uint TemperingShiftT(uint y)
        {
            return y << 15;
        }

        private static uint TemperingShiftL(uint y)
        {
            return y >> 18;
        }

        private readonly uint[] mt = new uint[625];
        private int mti = N + 1;

        private void Generate(uint seed)
        {
            mt[0] = seed & 0xffffffff;
            for (mti = 1; mti < N; mti++)
                mt[mti] = (69069 * mt[mti - 1]) & 0xffffffff;
        }

        private double Generate()
        {
            uint y;
            uint[] mag01 = new uint[2] { 0x0, MATRIX_A };

            if (mti >= N)
            {
                int kk;
                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];

                mti = 0;
            }

            y = mt[mti++];
            y ^= TemperingShiftU(y);
            y ^= TemperingShiftS(y) & TEMPERING_MASK_B;
            y ^= TemperingShiftT(y) & TEMPERING_MASK_C;
            y ^= TemperingShiftL(y);

            return (y * FINAL_CONSTANT);
        }

        /// <summary>
        /// Generate a random number between 0 and 1
        /// </summary>
        /// <returns></returns>
        public double NextDouble()
        {
            return Generate();
        }

        /// <summary>
        /// Generate an int between two bounds
        /// </summary>
        /// <param name="minValue">The lower bound (inclusive)</param>
        /// <param name="maxValue">The higher bound (inclusive)</param>
        /// <returns></returns>
        public int Next(int minValue, int maxValue)
        {
            if (maxValue < minValue)
                throw new ArgumentException("MaxValue cannot be smaller than minValue.");
            return Convert.ToInt32(Math.Floor(NextDouble(minValue * 1.0d, maxValue * 1.0d)));
        }

        /// <summary>
        /// Generate a double between two bounds
        /// </summary>
        /// <param name="minValue">The lower bound (inclusive)</param>
        /// <param name="maxValue">The higher bound (inclusive)</param>
        /// <returns>The random num or NaN if higherbound is lower than lowerbound</returns>
        public double NextDouble(double minValue, double maxValue)
        {
            if (maxValue < minValue)
                throw new ArgumentException("MaxValue cannot be smaller than minValue.");
            return (NextDouble() * (maxValue - minValue + 1)) + minValue;
        }

        public (int mti, uint[] mt) SaveState()
        {
            return (mti, mt);
        }
    }
}
