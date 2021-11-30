using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace analytics
{
    public class SendAnalytics : MonoBehaviour
    {
        [field: SerializeField]
        public string buildTag { get; private set; } = "test_version";

        private game.GameState gameState;

        private string gameStartedEventKey;
        private string gameOverEventKey;
        private string gameOverMetricsEventKey;
        private string gameProgressEventKey;
        private string gameProgressMetricsEventKey;
        private string gameDestroyedEventKey;
        private string gameDestroyedMetricsEventKey;

        private Dictionary<string, object> lastGameProgress;
        private Dictionary<string, object> lastGameMetrics;

        public bool sendAnalytics = true;
        public bool sendAnalyticsInDebug = false;

        void Start()
        {
            gameState = FindObjectOfType<game.GameStateBehavior>().gameState;

            gameStartedEventKey = "GameStarted_" + buildTag;
            gameOverEventKey = "GameOver_" + buildTag;
            gameOverMetricsEventKey = "GameOverMetrics_" + buildTag;
            gameProgressEventKey = "GameProgress_" + buildTag;
            gameProgressMetricsEventKey = "GameProgressMetrics_" + buildTag;
            gameDestroyedEventKey = "GameDestroyed_" + buildTag;
            gameDestroyedMetricsEventKey = "GameDestroyedMetrics_" + buildTag;

            Analytics.EnableCustomEvent(gameStartedEventKey, true);
            Analytics.EnableCustomEvent(gameOverEventKey, true);
            Analytics.EnableCustomEvent(gameOverMetricsEventKey, true);
            Analytics.EnableCustomEvent(gameProgressEventKey, true);
            Analytics.EnableCustomEvent(gameProgressMetricsEventKey, true);
            Analytics.EnableCustomEvent(gameDestroyedEventKey, true);
            Analytics.EnableCustomEvent(gameDestroyedMetricsEventKey, true);

            FindObjectOfType<map.HybridTransform>().viewSwitchedListeners += ViewSwitched;

#if UNITY_EDITOR
            if (!sendAnalyticsInDebug)
            {
                sendAnalytics = false;
            }
#endif
        }

        void Update()
        {
            Analytics.FlushEvents();
        }

        private void OnDestroy()
        {
            GameDestroyed();
        }

        public void GameStarted()
        {
            if (!sendAnalytics)
            {
                return;
            }

            lastGameProgress = GetGameProgress();
            AnalyticsResult aa = Analytics.CustomEvent(gameStartedEventKey, lastGameProgress);
            Debug.Log("Analytics Result (GameStarted): " + aa);
        }

        public void GameOver()
        {
            if (!sendAnalytics)
            {
                return;
            }

            lastGameProgress = GetGameProgress();
            AnalyticsResult aa = Analytics.CustomEvent(gameOverEventKey, lastGameProgress);
            Debug.Log("Analytics Result (GameOver): " + aa);

            lastGameMetrics = GetMetrics();
            if (lastGameMetrics != null)
            {
                aa = Analytics.CustomEvent(gameOverMetricsEventKey, lastGameMetrics);
                Debug.Log("Analytics Result (GameOverMetrics): " + aa);
            }
        }

        public void GameProgress()
        {
            if (!sendAnalytics)
            {
                return;
            }

            lastGameProgress = GetGameProgress();
            AnalyticsResult aa = Analytics.CustomEvent(gameProgressEventKey, lastGameProgress);
            Debug.Log("Analytics Result (GameProgress): " + aa);

            lastGameMetrics = GetMetrics();
            if (lastGameMetrics != null)
            {
                aa = Analytics.CustomEvent(gameProgressMetricsEventKey, lastGameMetrics);
                Debug.Log("Analytics Result (GameProgressMetrics): " + aa);
            }
        }

        private void GameDestroyed()
        {
            if (!sendAnalytics)
            {
                return;
            }

            if (lastGameProgress != null)
            {
                AnalyticsResult aa = Analytics.CustomEvent(gameDestroyedEventKey, lastGameProgress);
                Analytics.FlushEvents();
                Debug.Log("Analytics Result (GameDestroyed): " + aa);
            }

            if (lastGameMetrics != null)
            {
                AnalyticsResult aa = Analytics.CustomEvent(gameDestroyedMetricsEventKey, lastGameMetrics);
                Analytics.FlushEvents();
                Debug.Log("Analytics Result (GameDestroyedMetrics): " + aa);
            }
        }

        private Dictionary<string, object> GetGameProgress()
        {
            tutorial.TutorialBehavior tutorialBehavior = FindObjectOfType<tutorial.TutorialBehavior>();
            bool tutorialRunning = (tutorialBehavior != null) && tutorialBehavior.isActiveAndEnabled;

            int numMoverLevels = 0;
            int numPrepperLevels = 0;
            foreach (var creature in gameState.movers)
            {
                numMoverLevels += creature.numLevels;
            }
            foreach (var creature in gameState.movers)
            {
                numPrepperLevels += creature.numLevels;
            }

            var result = new Dictionary<string, object>
            {
                { "NumGames", gameState.numGames },
                { "Tutorial", tutorialRunning },
                { "OverallSeed", gameState.overallRandomSeed },
                { "DaysRemaining", GetDaysRemaining() },
                { "NumMovers", gameState.movers.Count },
                { "NumPreppers", gameState.preppers.Count },
                { "NumUpgrades", gameState.upgrades.Count },
                { "NumMoverLevels", numMoverLevels },
                { "NumPrepperLevels", numPrepperLevels },
                { "NumTotalLevels", numMoverLevels + numPrepperLevels }
            };

            return result;
        }

        private Dictionary<string, object> GetMetrics()
        {
            Dictionary<string, object> result = null;
            if (gameState.metrics.Count > 0)
            {
                logic.sim.Metrics metrics = gameState.metrics[gameState.metrics.Count - 1];
                result = new Dictionary<string, object>()
                {
                    { "NumTasksCompleted", metrics.overallMetrics.numTasksCompleted },
                    { "TaskTotalDurationSeconds", metrics.overallMetrics.taskTotalDurationSeconds.mean / 60 },
                    { "TaskOnRouteDurationSeconds", metrics.overallMetrics.taskOnRouteDurationSeconds.mean / 60 },
                    { "MoverNumTasksCompleted", metrics.overallMetrics.moverNumTasksCompleted.mean },
                    { "MoverUtilization", metrics.overallMetrics.moverUtilization.mean * 100 },
                    { "MoverDistanceTraveledMiles", metrics.overallMetrics.moverDistanceTraveledMeters.mean / menu.MetricsDisplay.MILE_TO_METERS },
                    { "PrepperNumTasksPrepared", metrics.overallMetrics.prepperNumTasksPrepared.mean },
                    { "PrepperUtilization", metrics.overallMetrics.prepperUtilization.mean * 100 },
                };
            }
            return result;
        }

        private float GetDaysRemaining()
        {
            return (float)logic.sim.TimeUtils.Elapsed(gameState.currentTime, game.GameState.END_TIME).milliseconds / game.GameState.ONE_DAY.milliseconds;
        }

        private void ViewSwitched(map.HybridTransform.ViewType from, map.HybridTransform.ViewType to)
        {
            if (from == map.HybridTransform.ViewType.MainMenu)
            {
                if (to != map.HybridTransform.ViewType.MainMenu)
                {
                    GameStarted();
                }
            }
            else if (to == map.HybridTransform.ViewType.MainMenu)
            {
                GameOver();
            }
            else if (to == map.HybridTransform.ViewType.PlaybackMap)
            {
                GameProgress();
            }
        }
    }
}
