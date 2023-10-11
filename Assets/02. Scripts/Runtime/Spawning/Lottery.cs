using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.ViewControllers;
using UnityEngine;

namespace Runtime.Spawning
{
    public class Lottery
    {
        private Dictionary<Vector2, LevelSpawnCard> entityWeights;
        
        public void SetCards(List<LevelSpawnCard> spawnCards)
        {
            int totalWeight = 0;
            foreach (LevelSpawnCard entity in spawnCards)
            {
                totalWeight += entity.RealSpawnWeight;
            }
            
            float currentWeight = 0f;
            foreach (LevelSpawnCard entity in spawnCards)
            {
                entityWeights.Add(new Vector2(currentWeight, currentWeight + entity.RealSpawnWeight), entity);
                currentWeight += entity.RealSpawnWeight;
            }
        }

        public LevelSpawnCard PickNextCard()
        {
            float randomWeight = Random.Range(0f, 1f);
            foreach (KeyValuePair<Vector2, LevelSpawnCard> entityWeight in entityWeights)
            {
                if (entityWeight.Key.x <= randomWeight && entityWeight.Key.y >= randomWeight)
                {
                    return entityWeight.Value;
                }
            }

            return default;
        }
    }
}