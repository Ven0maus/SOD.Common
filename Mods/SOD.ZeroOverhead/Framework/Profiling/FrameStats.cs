using System;

namespace SOD.ZeroOverhead.Framework.Profiling
{
    internal sealed class FrameStats
    {
        private readonly double[] _samples;
        private int _index;
        private int _count;

        public FrameStats(int window)
        {
            _samples = new double[window];
        }

        public void Add(double value)
        {
            _samples[_index] = value;
            _index = (_index + 1) % _samples.Length;
            if (_count < _samples.Length)
                _count++;
        }

        public double Average
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < _count; i++)
                    sum += _samples[i];
                return _count > 0 ? sum / _count : 0;
            }
        }

        public double Min
        {
            get
            {
                if (_count == 0) return 0;
                double min = double.MaxValue;
                for (int i = 0; i < _count; i++)
                    min = Math.Min(min, _samples[i]);
                return min;
            }
        }

        public double Max
        {
            get
            {
                double max = 0;
                for (int i = 0; i < _count; i++)
                    max = Math.Max(max, _samples[i]);
                return max;
            }
        }
    }
}
