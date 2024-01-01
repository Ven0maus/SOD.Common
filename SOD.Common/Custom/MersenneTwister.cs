using System;

namespace SOD.Common.Custom
{
    /// <summary>
    /// A Mersenne Twister Random Generator implementation, allows to export and import the state of the random.
    /// </summary>
    public sealed class MersenneTwister
    {
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908b0dfU;
        private const uint UPPER_MASK = 0x80000000U;
        private const uint LOWER_MASK = 0x7fffffffU;
        private readonly uint[] mag01 = { 0x0U, MATRIX_A };
        private readonly uint[] mt = new uint[N];
        private int mti = N + 1;

        public const int ArraySize = N;

        public MersenneTwister()
        {
            InitGenRand((uint)DateTime.Now.Millisecond);
        }

        public MersenneTwister(int seed)
        {
            InitGenRand((uint)seed);
        }

        public MersenneTwister((int index, uint[] mt) state)
        {
            mt = state.mt;
            mti = state.index;
        }

        /// <summary>
        /// Returns the full state of the MersenneTwister
        /// </summary>
        /// <returns></returns>
        public (int index, uint[] mt) SaveState()
        {
            return (mti, (uint[])mt.Clone());
        }

        public MersenneTwister(int[] init)
        {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i)
                initArray[i] = (uint)init[i];
            InitByArray(initArray, (uint)initArray.Length);
        }

        public static int MaxRandomInt => 0x7fffffff;
        /// <summary>
        /// Generates a random integer value
        /// </summary>
        /// <returns></returns>
        public int Next() => GenRandInt31();
        /// <summary>
        /// Gives a random integer which can be 0 or maxValue or any number in between. (max inclusive)
        /// </summary>
        /// <param name="maxValue">(inclusive)</param>
        /// <returns></returns>
        public int Next(int maxValue) => Next(0, maxValue);
        /// <summary>
        /// Gives a random integer which can be minValue or maxValue or any number in between. (max inclusive)
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue">(inclusive)</param>
        /// <returns></returns>
        public int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                (minValue, maxValue) = (maxValue, minValue);
            }
            return (int)(Math.Floor((maxValue - minValue + 1) * GenRandReal1() +
            minValue));
        }

        public float NextFloat() => (float)GenRandReal2();
        public float NextFloat(bool includeOne)
        {
            if (includeOne)
            {
                return (float)GenRandReal1();
            }
            return (float)GenRandReal2();
        }
        public float NextFloat(float min, float max)
        {
            return NextFloat() * (max - min) + min;
        }

        public float NextFloatPositive() => (float)GenRandReal3();
        public double NextDouble() => GenRandReal2();
        public double NextDouble(bool includeOne)
        {
            if (includeOne)
            {
                return GenRandReal1();
            }
            return GenRandReal2();
        }
        public double NextDouble(double min, double max)
        {
            return NextDouble() * (max - min) + min;
        }

        public double NextDoublePositive() => GenRandReal3();
        public double Next53BitRes() => GenRandRes53();

        private void InitGenRand(uint s)
        {
            mt[0] = s & 0xffffffffU;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (uint)(1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                mt[mti] &= 0xffffffffU;
            }
        }

        private void InitByArray(uint[] init_key, uint key_length)
        {
            int i, j, k;
            InitGenRand(19650218U);
            i = 1; j = 0;
            k = (int)(N > key_length ? N : key_length);
            for (; k > 0; k--)
            {
                mt[i] = (uint)((mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525U)) + init_key[j] + j);
                mt[i] &= 0xffffffffU;
                i++; j++;
                if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
                if (j >= key_length) j = 0;
            }
            for (k = N - 1; k > 0; k--)
            {
                mt[i] = (uint)((mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) *
                1566083941U)) - i);
                mt[i] &= 0xffffffffU;
                i++;
                if (i >= N) { mt[0] = mt[N - 1]; i = 1; }
            }
            mt[0] = 0x80000000U;
        }

        private uint GenRandInt32()
        {
            uint y;
            if (mti >= N)
            {
                int kk;
                if (mti == N + 1)
                    InitGenRand(5489U);
                for (kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
                }
                for (; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];
                mti = 0;
            }
            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);
            return y;
        }

        private int GenRandInt31() => (int)(GenRandInt32() >> 1);
        private double GenRandReal1() => GenRandInt32() * (1.0 / 4294967295.0);
        private double GenRandReal2() => GenRandInt32() * (1.0 / 4294967296.0);
        private double GenRandReal3() => (GenRandInt32() + 0.5) * (1.0 / 4294967296.0);

        private double GenRandRes53()
        {
            uint a = GenRandInt32() >> 5, b = GenRandInt32() >> 6;
            return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
        }
    }
}
