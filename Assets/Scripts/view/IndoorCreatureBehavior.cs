using UnityEngine;

namespace view
{
    public class IndoorCreatureBehavior : MonoBehaviour
    {
        public static readonly float CLOSE_DISTANCE = 1e-2f;

        [field: Header("Creature")]
        [field: SerializeField]
        private GameObject creatureAnimation;
        [field: SerializeField]
        private float faceRightSign = -1;

        [field: Header("States")]
        [field: SerializeField]
        private utils.RangeSelection.MinMaxFloat idleTime = new utils.RangeSelection.MinMaxFloat(1, 3);
        [field: SerializeField]
        private utils.RangeSelection.MinMaxFloat maxWanderDistance = new utils.RangeSelection.MinMaxFloat(2, 5);
        [field: SerializeField]
        private float speedMultiplier = 1;
        [field: SerializeField]
        private float idleMultiplier = 1;

        [System.Serializable]
        public enum CreatureState
        {
            Idle,
            Wander,
        }

        private Animator animator;
        private game.GameState gameState;
        private IndoorViewer indoorViewer;

        private bool isMover;
        private logic.creature.Creature creature;
        private CreatureState creatureState;
        private float idleTimeRemaining;
        private Vector2 wanderTarget;

        void Start()
        {
            animator = creatureAnimation.GetComponent<Animator>();
            gameState = FindObjectOfType<game.GameStateBehavior>().gameState;
            indoorViewer = FindObjectOfType<IndoorViewer>();

            creatureState = CreatureState.Idle;
            wanderTarget = new Vector2(0, 0);
            AnimateIdle();
        }

        void Update()
        {
            switch (creatureState)
            {
                case CreatureState.Idle:
                    idleTimeRemaining -= Time.deltaTime;
                    if (idleTimeRemaining <= 0)
                    {
                        wanderTarget = ChooseAndFaceWanderTarget();
                        creatureState = CreatureState.Wander;
                        AnimateWalk();
                    }
                    break;
                case CreatureState.Wander:
                    if (MoveTowardsWanderTarget())
                    {
                        idleTimeRemaining = SampleIdleTime();
                        creatureState = CreatureState.Idle;
                        AnimateIdle();
                    }
                    break;
            }
        }

        private float SampleIdleTime()
        {
            return idleTime.Sample(gameState.indoorCreatureRandom) / (creature.overallStats.speed * idleMultiplier);
        }

        public void SetCreature(logic.creature.Creature creature, bool isMover)
        {
            gameState = FindObjectOfType<game.GameStateBehavior>().gameState;
            this.creature = creature;
            this.isMover = isMover;
            idleTimeRemaining = 0;
        }

        public void AnimateWalk()
        {
            animator.SetInteger("State", 2);
        }

        public void AnimateIdle()
        {
            animator.SetInteger("State", 0);
        }

        private Vector2 ChooseAndFaceWanderTarget()
        {
            Vector2 wanderTargetRaw = indoorViewer.GetRandomPosition(isMover);
            float wanderDistance = maxWanderDistance.Sample(gameState.indoorCreatureRandom);
            Vector2 wanderTarget = Vector2.MoveTowards(transform.position, wanderTargetRaw, wanderDistance);

            Vector3 scale = creatureAnimation.transform.localScale;
            if (wanderTarget.x - transform.position.x > 0)
            {
                scale.x = faceRightSign * Mathf.Abs(scale.x);
            }
            else
            {
                scale.x = -faceRightSign * Mathf.Abs(scale.x);
            }
            creatureAnimation.transform.localScale = scale;

            return wanderTarget;
        }

        private bool MoveTowardsWanderTarget()
        {
            float agentSpeed = creature.overallStats.speed * speedMultiplier * Time.deltaTime;
            Vector2 newPosition;
            bool arrived = false;

            if ((wanderTarget - (Vector2)transform.position).magnitude <= agentSpeed)
            {
                newPosition = wanderTarget;
                arrived = true;
            }
            else
            {
                newPosition = Vector2.MoveTowards(transform.position, wanderTarget, agentSpeed);
            }
            transform.position = newPosition;

            return arrived;
        }

    }
}
