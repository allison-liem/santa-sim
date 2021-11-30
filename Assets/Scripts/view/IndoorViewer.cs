using System.Collections.Generic;
using UnityEngine;

namespace view
{
    public class IndoorViewer : MonoBehaviour
    {
        [field: SerializeField]
        private Transform indoorCreaturesParent;
        [field: SerializeField]
        private Vector2 prepperPositionMin;
        [field: SerializeField]
        private Vector2 prepperPositionMax;
        [field: SerializeField]
        private Vector2 moverPositionMin;
        [field: SerializeField]
        private Vector2 moverPositionMax;

        private logic.creature.CreatureInfoBehavior creatureInfoBehavior;
        private Dictionary<logic.creature.Creature, GameObject> preppers;
        private Dictionary<logic.creature.Creature, GameObject> movers;

        private game.GameState gameState;

        void Start()
        {
            creatureInfoBehavior = FindObjectOfType<logic.creature.CreatureInfoBehavior>();
            preppers = new Dictionary<logic.creature.Creature, GameObject>();
            movers = new Dictionary<logic.creature.Creature, GameObject>();

            gameState = FindObjectOfType<game.GameStateBehavior>().gameState;
            ResetCreatures();
            gameState.gameStateChangedListeners += GameStateChanged;
        }

        void Update()
        {
        }

        private void GameStateChanged()
        {
            // Do we need to spawn new creatures?
            foreach (logic.creature.Creature mover in gameState.movers)
            {
                if (movers.ContainsKey(mover))
                {
                    continue;
                }

                GameObject prefab = creatureInfoBehavior.GetCreatureInfo(mover.type).creatureIndoorPrefab;
                GameObject moverGameObject = Instantiate(prefab, indoorCreaturesParent);
                moverGameObject.GetComponent<IndoorCreatureBehavior>().SetCreature(mover, true);
                moverGameObject.transform.position = GetRandomPosition(true);

                movers.Add(mover, moverGameObject);
            }
            foreach (logic.creature.Creature prepper in gameState.preppers)
            {
                if (preppers.ContainsKey(prepper))
                {
                    continue;
                }

                GameObject prefab = creatureInfoBehavior.GetCreatureInfo(prepper.type).creatureIndoorPrefab;
                GameObject prepperGameObject = Instantiate(prefab, indoorCreaturesParent);
                prepperGameObject.GetComponent<IndoorCreatureBehavior>().SetCreature(prepper, false);
                prepperGameObject.transform.position = GetRandomPosition(false);

                preppers.Add(prepper, prepperGameObject);
            }
        }

        public void ResetCreatures()
        {
            for (int i = 0; i < indoorCreaturesParent.childCount; i++)
            {
                Destroy(indoorCreaturesParent.GetChild(i).gameObject);
            }
            movers.Clear();
            preppers.Clear();
            GameStateChanged();
        }

        public Vector2 GetRandomPosition(bool isMover)
        {
            if (isMover)
            {
                return new Vector2(gameState.indoorCreatureRandom.GetFloat(moverPositionMin.x, moverPositionMax.x), gameState.indoorCreatureRandom.GetFloat(moverPositionMin.y, moverPositionMax.y));
            }
            else
            {
                return new Vector2(gameState.indoorCreatureRandom.GetFloat(prepperPositionMin.x, prepperPositionMax.x), gameState.indoorCreatureRandom.GetFloat(prepperPositionMin.y, prepperPositionMax.y));
            }
        }
    }
}
