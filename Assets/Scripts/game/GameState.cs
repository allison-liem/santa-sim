using System.Collections.Generic;
using UnityEngine;

namespace game
{
    [System.Serializable]
    public class Currency
    {
        [field: SerializeField]
        public int gold { get; private set; }

        [field: SerializeField]
        public int hearts { get; private set; }

        [field: SerializeField]
        public logic.sim.Duration duration { get; private set; }

        public Currency(int gold, int hearts, logic.sim.Duration duration)
        {
            this.gold = gold;
            this.hearts = hearts;
            this.duration = duration;
        }

        public Currency(Currency other, float multiplier, bool roundToMinutes)
        {
            gold = Mathf.RoundToInt(other.gold * multiplier);
            hearts = Mathf.RoundToInt(other.hearts);

            long milliseconds = Mathf.RoundToInt(other.duration.milliseconds * multiplier);
            if (roundToMinutes)
            {
                milliseconds = Mathf.RoundToInt(milliseconds / 60000f) * 60000;
            }
            duration = new logic.sim.Duration(milliseconds);
        }

        public override string ToString()
        {
            string str = "";
            if (gold > 0)
            {
                str += gold + " gold";
            }
            if (hearts > 0)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    str += ", ";
                }
                str += hearts + " heart" + (hearts > 1 ? "s" : "");
            }
            if (logic.sim.TimeUtils.Longer(duration, logic.sim.TimeUtils.ZERO_DURATION))
            {
                int minutes = (int)(duration.milliseconds / 60000);
                int hours = minutes / 60;
                minutes = minutes % 60;
                if (!string.IsNullOrEmpty(str))
                {
                    str += " and ";
                }
                str += hours + ":" + (minutes < 10 ? "0" : "") + minutes;
            }

            return str;
        }
    }

    [System.Serializable]
    public class GameState
    {
        // December 25th, 12am
        public static readonly logic.sim.Time END_TIME = new logic.sim.Time(1640419200000L);
        // 24 hours
        public static readonly logic.sim.Duration ONE_DAY = new logic.sim.Duration(24L * 60L * 60L * 1000L);

        public static readonly float COMPOUND_INTEREST = 0.5f;
        public static readonly float EXTRA_INCOME = 1f;

        public delegate void GameStateChanged();
        public event GameStateChanged gameStateChangedListeners;

        public delegate void NewDay();
        public event NewDay newDayListeners;

        public delegate void ResetGame();
        public event ResetGame resetGameListeners;

        [field: SerializeField]
        public string overallRandomSeed { get; private set; }

        [field: SerializeField]
        public int numTasks { get; private set; }

        [field: SerializeField]
        public logic.sim.Time santaArrivalTime { get; private set; }

        [field: SerializeField]
        public logic.sim.Duration simulationDuration { get; private set; }

        [field: SerializeField]
        public logic.sim.Time dayEndTime { get; private set; }

        [field: SerializeField]
        public logic.sim.Time workDayEndTime { get; private set; }

        [field: SerializeField]
        public logic.sim.Time dayStartTime { get; private set; }

        [field: SerializeField]
        public logic.sim.Time currentTime { get; private set; }

        [field: SerializeField]
        public int dailyGold { get; private set; }

        [field: SerializeField]
        public int gold { get; private set; }

        [field: SerializeField]
        public int hearts { get; private set; }

        [field: SerializeField]
        public List<logic.creature.Creature> preppers { get; private set; }

        [field: SerializeField]
        public List<logic.creature.Creature> movers { get; private set; }

        // We use a list instead of a hashset so it can be serialized if need be
        [field: SerializeField]
        public List<logic.upgrade.UpgradeScriptableObject> upgrades { get; private set; }

        [field: SerializeField]
        private int simulationGenerationSeed;
        [field:SerializeField]
        public utils.SeededRandom simulationGenerationRandom { get; private set; }

        // Each day should have its own random seed
        [field: SerializeField]
        private List<int> dailyRandomSeeds;

        // The simulation should use a constant random seed
        int simulationRandomSeed;

        // Every day, there should be a random generator for:
        // hiring pool, simulation, indoor creatures wandering
        [field: SerializeField]
        public utils.SeededRandom hiringRandom { get; private set; }
        [field: SerializeField]
        public utils.SeededRandom simulationRandom { get; private set; }
        [field:SerializeField]
        public utils.SeededRandom indoorCreatureRandom { get; private set; }

        // Keep track of the daily metrics
        public List<logic.sim.Metrics> metrics { get; private set; }

        // How many games have been played?
        public int numGames { get; private set; }


        public GameState(string overallRandomSeed, int numTasks, logic.sim.Time santaArrivalTime, logic.sim.Time dayStartTime, int dailyGold, int gold, int hearts)
        {
            ResetState(overallRandomSeed, numTasks, santaArrivalTime, dayStartTime, dailyGold, gold, hearts);
            numGames = 0;
        }

        public void ResetState(string overallRandomSeed, int numTasks, logic.sim.Time santaArrivalTime, logic.sim.Time dayStartTime, int dailyGold, int gold, int hearts)
        {
            numGames++;

            this.overallRandomSeed = overallRandomSeed;

            this.numTasks = numTasks;
            this.santaArrivalTime = santaArrivalTime;
            simulationDuration = logic.sim.TimeUtils.Elapsed(santaArrivalTime, END_TIME);

            this.dayStartTime = dayStartTime;
            this.dailyGold = dailyGold;
            this.gold = gold;
            this.hearts = hearts;

            preppers = new List<logic.creature.Creature>();
            movers = new List<logic.creature.Creature>();
            upgrades = new List<logic.upgrade.UpgradeScriptableObject>();
            metrics = new List<logic.sim.Metrics>();

            // Generate random seed for simulation generation, and one for every day
            (simulationGenerationSeed, dailyRandomSeeds) = GenerateRandomSeeds();

            ComputeDayEndTime();
            StartDay();
            resetGameListeners?.Invoke();
            gameStateChangedListeners?.Invoke();
        }

        public void AddPrepper(logic.creature.Creature prepper)
        {
            preppers.Add(prepper);
            gameStateChangedListeners?.Invoke();
        }

        public void AddMover(logic.creature.Creature mover)
        {
            movers.Add(mover);
            gameStateChangedListeners?.Invoke();
        }

        public void AddUpgrade(logic.upgrade.UpgradeScriptableObject upgrade)
        {
            upgrades.Add(upgrade);
            gameStateChangedListeners?.Invoke();
        }

        public void AddMetrics(logic.sim.Metrics metrics)
        {
            this.metrics.Add(metrics);
        }

        private (int, List<int>) GenerateRandomSeeds()
        {
            utils.SeededRandom overallRandom = new utils.SeededRandom(overallRandomSeed.GetHashCode());

            int simulationGenerationSeed = GenerateRandomSeed(overallRandom);

            // How many days are there?
            logic.sim.Duration durationRemaining = logic.sim.TimeUtils.Elapsed(dayStartTime, END_TIME);
            int numDays = Mathf.CeilToInt(durationRemaining.milliseconds / 1000f / 60f / 60f / 24f);

            List<int> randomSeeds = new List<int>();
            for (int i = 0; i < numDays; i++)
            {
                randomSeeds.Add(GenerateRandomSeed(overallRandom));
            }

            simulationRandomSeed = GenerateRandomSeed(overallRandom);

            return (simulationGenerationSeed, randomSeeds);
        }

        private int GenerateRandomSeed(utils.SeededRandom seededRandom)
        {
            return seededRandom.GetInt(0, int.MaxValue);
        }

        public void StartDay()
        {
            if (dailyRandomSeeds.Count <= 0)
            {
                throw new System.InvalidOperationException("Ran out of daily random seeds");
            }
            currentTime = dayStartTime;

            simulationGenerationRandom = new utils.SeededRandom(simulationGenerationSeed);

            utils.SeededRandom randomToday = new utils.SeededRandom(dailyRandomSeeds[0]);
            dailyRandomSeeds.RemoveAt(0);

            int hiringRandomSeed = GenerateRandomSeed(randomToday);
            int indoorCreatureRandomSeed = GenerateRandomSeed(randomToday);

            hiringRandom = new utils.SeededRandom(hiringRandomSeed);
            simulationRandom = new utils.SeededRandom(simulationRandomSeed);
            indoorCreatureRandom = new utils.SeededRandom(indoorCreatureRandomSeed);

            gold += ComputeDailyGold();

            gameStateChangedListeners?.Invoke();
            newDayListeners?.Invoke();
        }

        public void EndDay()
        {
            currentTime = dayEndTime;
            dayStartTime = logic.sim.TimeUtils.Add(dayStartTime, ONE_DAY);
            ComputeDayEndTime();
            gameStateChangedListeners?.Invoke();
        }

        private void ComputeDayEndTime()
        {
            if (logic.sim.TimeUtils.After(END_TIME, dayStartTime))
            {
                dayEndTime = END_TIME;
                while (logic.sim.TimeUtils.Longer(logic.sim.TimeUtils.Elapsed(dayStartTime, dayEndTime), ONE_DAY))
                {
                    dayEndTime = new logic.sim.Time(dayEndTime.millisecondsSinceEpoch - ONE_DAY.milliseconds);
                }
                workDayEndTime = new logic.sim.Time(dayEndTime.millisecondsSinceEpoch - simulationDuration.milliseconds);
            }
            else
            {
                dayStartTime = END_TIME;
                dayEndTime = END_TIME;
                workDayEndTime = END_TIME;
            }
        }

        private int ComputeDailyGold()
        {
            int dailyGold = this.dailyGold;

            // Check for compound interest
            foreach (var upgrade in upgrades)
            {
                foreach (var special in upgrade.effect.specials)
                {
                    if (special is logic.creature.TraitScriptableObject.Special.CompoundInterest)
                    {
                        dailyGold += Mathf.RoundToInt(gold * COMPOUND_INTEREST);
                    }
                    if (special is logic.creature.TraitScriptableObject.Special.ExtraIncome)
                    {
                        dailyGold += Mathf.RoundToInt(this.dailyGold * EXTRA_INCOME);
                    }
                }
            }
            return dailyGold;
        }

        public bool HaveEnoughCurrency(Currency currency)
        {
            if (gold < currency.gold)
            {
                return false;
            }
            if (hearts < currency.hearts)
            {
                return false;
            }
            var durationTillWorkDayEnds = logic.sim.TimeUtils.Elapsed(currentTime, workDayEndTime);
            if (logic.sim.TimeUtils.Longer(currency.duration, durationTillWorkDayEnds))
            {
                return false;
            }
            return true;
        }

        public void SpendCurrency(Currency currency)
        {
            if (!HaveEnoughCurrency(currency))
            {
                throw new System.InvalidOperationException("Insufficient currency: " + currency);
            }

            gold -= currency.gold;
            hearts -= currency.hearts;
            currentTime = logic.sim.TimeUtils.Add(currentTime, currency.duration);

            gameStateChangedListeners?.Invoke();
        }

        public void AddHearts(int hearts)
        {
            this.hearts += hearts;
            gameStateChangedListeners?.Invoke();
        }
    }
}
