using SOD.Common.Shadows.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Implementation
{
    internal static class MathHelper
    {
        internal static MersenneTwisterRandom Random { get; private set; }

        internal static void Init(int seed)
        {
            Random = new MersenneTwisterRandom(seed);
        }

        internal static void Init(MersenneTwisterRandom random)
        {
            Random = random;
        }

        private static double NextGaussian()
        {
            double u, v, s;
            do
            {
                u = 2.0 * Random.NextDouble() - 1.0;
                v = 2.0 * Random.NextDouble() - 1.0;
                s = u * u + v * v;
            } while (s >= 1.0 || s == 0.0);

            s = Math.Sqrt(-2.0 * Math.Log(s) / s);

            return u * s;
        }

        public static double NextGaussian(double mean, double stdDev)
        {
            return mean + stdDev * NextGaussian();
        }

        public static double CalculateStandardDeviation(List<double> values)
        {
            double mean = values.Average();
            double sumSquaredDifferences = values.Sum(value => Math.Pow(value - mean, 2));
            double variance = sumSquaredDifferences / values.Count;
            return Math.Sqrt(variance);
        }
    }
}
