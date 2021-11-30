using UnityEngine;

namespace game
{
    public class GameStateBehavior : MonoBehaviour
    {
        [field: SerializeField]
        private logic.sim.Time startTime = new logic.sim.Time(1640041200000);
        [field: SerializeField]
        private logic.sim.Time santaArrivalTime = new logic.sim.Time(1640415600000);

        [field: SerializeField]
        public string overallRandomSeed = "Claus";

        [field: SerializeField]
        private int numTasks = 200;

        [field: SerializeField]
        private int startingGold = 1000;

        [field: SerializeField]
        private int dailyGold = 1000;

        [field: SerializeField]
        public GameState gameState { get; private set; }

        void Awake()
        {
            gameState = new GameState(overallRandomSeed, numTasks, santaArrivalTime, startTime, dailyGold, startingGold, 0);
        }

        void Update()
        {
        }

        public void StartNewGame()
        {
            gameState.ResetState(overallRandomSeed, numTasks, santaArrivalTime, startTime, dailyGold, startingGold, 0);
        }
    }
}
