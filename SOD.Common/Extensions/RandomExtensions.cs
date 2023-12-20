using System;

namespace SOD.Common.Extensions
{
    public static class RandomExtensions
    {
        /// <summary>
        /// Returns a double between the min and max.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static double NextDouble(this Random random, double minValue, double maxValue, bool inclusive = false)
        {
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            double range = maxValue - minValue;

            if (inclusive)
            {
                // Include the maxValue in the range
                return minValue + random.NextDouble() * (range + double.Epsilon);
            }
            else
            {
                // Exclude the maxValue from the range
                return minValue + random.NextDouble() * range;
            }
        }

        /// <summary>
        /// Returns a double between the min and max(inclusive).
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue">inclusive</param>
        /// <param name="inclusive"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static float NextFloat(this Random random, float minValue, float maxValue, bool inclusive = false)
        {
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            float range = maxValue - minValue;

            if (inclusive)
            {
                // Include the maxValue in the range
                return minValue + (float)random.NextDouble() * (range + float.Epsilon);
            }
            else
            {
                // Exclude the maxValue from the range
                return minValue + (float)random.NextDouble() * range;
            }
        }

        /// <summary>
        /// Returns a float greater or equal than 0.0 and less than 1.0
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static float NextFloat(this Random random)
        {
            return (float)random.NextDouble();
        }
    }
}
