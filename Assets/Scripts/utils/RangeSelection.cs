using System;

namespace utils
{
    public class RangeSelection
    {
        [Serializable]
        public class MinMaxInt
        {
            public int minimum;
            public int maximum;

            public MinMaxInt(int min, int max)
            {
                minimum = min;
                maximum = max;
            }

            public int Sample()
            {
                return Sample(SeededRandom.UNSEEDED);
            }

            public int Sample(SeededRandom seededRandom)
            {
                return seededRandom.GetInt(minimum, maximum + 1);
            }
        }

        [Serializable]
        public class MinMaxFloat
        {
            public float minimum;
            public float maximum;

            public MinMaxFloat(float min, float max)
            {
                minimum = min;
                maximum = max;
            }

            public float Sample()
            {
                return Sample(SeededRandom.UNSEEDED);
            }

            public float Sample(SeededRandom seededRandom)
            {
                return seededRandom.GetFloat(minimum, maximum);
            }
        }

        [Serializable]
        public class Gaussian
        {
            public float mean;
            public float stdev;

            public Gaussian(float mean = 0f, float stdev = 1f)
            {
                this.mean = mean;
                this.stdev = stdev;
            }

            public float Sample()
            {
                return Sample(SeededRandom.UNSEEDED);
            }

            public virtual float Sample(SeededRandom seededRandom)
            {
                return (float)seededRandom.GetGaussian(mean, stdev);
            }
        }

        [Serializable]
        public class MinMaxGaussian : Gaussian
        {
            public float minimum;
            public float maximum;

            public MinMaxGaussian(float minimum, float maximum, float mean, float stdev)
                : base(mean, stdev)
            {
                this.minimum = minimum;
                this.maximum = maximum;
            }

            public override float Sample(SeededRandom seededRandom)
            {
                return (float)seededRandom.GetBoundedGaussian(minimum, maximum, mean, stdev);
            }
        }
    }
}
