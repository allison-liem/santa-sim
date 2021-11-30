using System;
using System.Collections.Generic;

namespace utils
{
    public class WeightedSelection
    {
        public interface WeightedInterface
        {
            float GetWeight();
        }

        [Serializable]
        public class Weighted : WeightedInterface
        {
            public float weight;

            public float GetWeight()
            {
                return weight;
            }
        }

        [Serializable]
        public class WeightedOption<T> : Weighted
        {
            public T option;

            public WeightedOption(T option, float weight = 1f)
            {
                this.option = option;
                this.weight = weight;
            }
        }

        public delegate bool accept<T>(T option);

        public static T SelectWeighted<T>(SeededRandom seededRandom, T[] options) where T : WeightedInterface
        {
            return SelectWeighted(seededRandom, new List<T>(options));
        }

        public static T SelectWeighted<T>(SeededRandom seededRandom, T[] options, accept<T> accepter) where T : WeightedInterface
        {
            return SelectWeighted(seededRandom, new List<T>(options), accepter);
        }

        public static T SelectWeighted<T>(SeededRandom seededRandom, List<T> options) where T : WeightedInterface
        {
            return SelectWeighted(seededRandom, options, foo => true);
        }

        public static T SelectWeighted<T>(SeededRandom seededRandom, List<T> options, accept<T> accepter) where T : WeightedInterface
        {
            T randomSelection = options[seededRandom.GetInt(0, options.Count)];
            T selected = default(T);
            while ((selected == null) && (options.Count > 0))
            {
                float weight = 0;
                foreach (T option in options)
                {
                    weight += option.GetWeight();
                }
                float value = seededRandom.GetFloat(0, weight);
                bool tried = false;
                for (int i = 0; i < options.Count; i++)
                {
                    T option = options[i];
                    if (value <= option.GetWeight())
                    {
                        options.Remove(option);
                        tried = true;

                        if (accepter(option))
                        {
                            selected = option;
                            break;
                        }
                    }
                    else
                    {
                        value -= option.GetWeight();
                    }
                }
                if (!tried)
                {
                    // We shouldn't get here, if we did, remove one random option
                    options.RemoveAt(seededRandom.GetInt(0, options.Count));
                }
            }
            // If we haven't chosen, use the random selection
            if (selected == null)
            {
                selected = randomSelection;
            }
            return selected;
        }
    }
}
