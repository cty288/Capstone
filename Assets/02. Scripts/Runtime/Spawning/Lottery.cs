using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.ViewControllers;
using UnityEngine;

namespace Runtime.Spawning
{
    public class Lottery
    {
        private Dictionary<LevelSpawnCard, Vector2> entityWeights;
        
        public Lottery()
        {
            entityWeights = new Dictionary<LevelSpawnCard, Vector2>();
        }
        
        public void SetCards(List<LevelSpawnCard> spawnCards)
        {
            entityWeights.Clear();
            int totalWeight = 0;
            foreach (LevelSpawnCard entity in spawnCards)
            {
                totalWeight += entity.RealSpawnWeight;
            }
            
            float currentWeight = 0f;
            foreach (LevelSpawnCard entity in spawnCards)
            {
                entityWeights.Add(entity, new Vector2(currentWeight, currentWeight + entity.RealSpawnWeight));
                currentWeight += entity.RealSpawnWeight;
            }
        }

        public LevelSpawnCard PickNextCard()
        {
            float randomWeight = Random.Range(0f, 1f);
            foreach (KeyValuePair<LevelSpawnCard, Vector2> entityWeight in entityWeights)
            {
                if (entityWeight.Value.x <= randomWeight && entityWeight.Value.y >= randomWeight)
                {
                    return entityWeight.Key;
                }
            }

            return default;
        }
    }
}