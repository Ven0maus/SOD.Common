using System;
using System.Collections.Generic;
using System.Linq;

namespace SOD.StockMarket.Core
{
    internal class Helpers
    {
        private static bool hasSpareRandom;
        private static double spareRandom;
        public static Random Random { get; private set; }

        public static void Init(int seed)
        {
            Random = new Random(seed);
        }

        private static double NextGaussian()
        {
            if (hasSpareRandom)
            {
                hasSpareRandom = false;
                return spareRandom;
            }

            double u, v, s;
            do
            {
                u = 2.0 * Random.NextDouble() - 1.0;
                v = 2.0 * Random.NextDouble() - 1.0;
                s = u * u + v * v;
            } while (s >= 1.0 || s == 0.0);

            s = Math.Sqrt(-2.0 * Math.Log(s) / s);
            spareRandom = v * s;
            hasSpareRandom = true;

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
