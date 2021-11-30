namespace utils
{
    public class SeededRandom
    {
        public static SeededRandom UNSEEDED = new SeededRandom();

        private System.Random seededRandom;

        private SeededRandom()
        {
            seededRandom = new System.Random();
        }

        public SeededRandom(int seed)
        {
            seededRandom = new System.Random(seed);
        }

        public virtual bool GetBool()
        {
            return seededRandom.NextDouble() > 0.5;
        }

        public virtual int GetInt(int minInclusive, int maxExclusive)
        {
            return seededRandom.Next(minInclusive, maxExclusive);
        }

        public virtual float GetFloat()
        {
            return (float)seededRandom.NextDouble();
        }

        public virtual float GetFloat(float minInclusive, float maxInclusive)
        {
            float delta = maxInclusive - minInclusive;
            return GetFloat() * delta + minInclusive;
        }

        public virtual double GetDouble()
        {
            return seededRandom.NextDouble();
        }

        public virtual double GetGaussian(double mean = 0d, double stdev = 1d)
        {
            // https://stackoverflow.com/questions/218060/random-gaussian-variables

            double u1 = 1.0 - seededRandom.NextDouble(); // uniform(0,1] random doubles
            double u2 = 1.0 - seededRandom.NextDouble();
            double randStdNormal = System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Sin(2.0 * System.Math.PI * u2); // random normal(0,1)
            return randStdNormal + mean + stdev * randStdNormal;
        }

        public virtual double GetBoundedGaussian(double minInclusive, double maxInclusive, double mean = 0d, double stdev = 1d)
        {
            if (minInclusive == maxInclusive)
            {
                return minInclusive;
            }
            else if (minInclusive > maxInclusive)
            {
                throw new System.InvalidOperationException("minInclusive must be < maxInclusive: " + minInclusive + ", " + maxInclusive);
            }

            while (true)
            {
                double value = GetGaussian(mean, stdev);
                if ((value >= minInclusive) && (value <= maxInclusive))
                {
                    return value;
                }
            }
        }
    }
}
