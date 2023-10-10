using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Spawning
{
    public class Lottery<T>
    {
        private Dictionary<Vector2, T> entityWeights;
        
        public void SetCards(List<T> spawnCards)
        {
            int totalWeight = 0;
            foreach (T entity in spawnCards)
            {
                totalWeight += entity.GetRealWeight();
            }
            
            float currentWeight = 0f;
            foreach (T entity in spawnCards)
            {
                entityWeights.Add(new Vector2(currentWeight, currentWeight + entity.GetRealWeight()), entity);
                currentWeight += entity.GetRealWeight();
            }
        }

        public T PickNextCard()
        {
            float randomWeight = Random.Range(0f, 1f);
            foreach (KeyValuePair<Vector2, T> entityWeight in entityWeights)
            {
                if (entityWeight.Key.x <= randomWeight && entityWeight.Key.y >= randomWeight)
                {
                    return entityWeight.Value;
                }
            }
        }
    }
}