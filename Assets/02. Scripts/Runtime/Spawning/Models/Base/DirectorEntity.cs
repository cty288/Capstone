using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;

namespace Runtime.Spawning
{
    public interface IDirectorEntity : IEntity, IHaveCustomProperties, IHaveTags
    {
        public float GetRealMinSpawnRange();
        public float GetRealMaxSpawnRange();
        public int GetRealStartingCredits();
        public int GetRealCurrentCredits();
        public int GetRealCreditsPerSecond();
        public float GetRealSpawnTimer();
        public float GetRealPackSpawnTimer();
        public ILevelEntity GetLevelEntity();
        public void SetLevelEntity(ILevelEntity levelEntity);
    }

    public abstract class DirectorEntity<T> : AbstractBasicEntity, IDirectorEntity where T : DirectorEntity<T>, new()
    {
        [field: ES3Serializable] 
        private float minSpawnRange;
        
        [field: ES3Serializable] 
        private float maxSpawnRange;
        
        [field: ES3Serializable] 
        private int startingCredits;
        
        [field: ES3Serializable] 
        private int currentCredits;
        
        [field: ES3Serializable] 
        private int creditsPerSecond;
        
        [field: ES3Serializable] 
        private float spawnTimer;
        
        [field: ES3Serializable] 
        private float packSpawnTimer;
        
        [field: ES3Serializable] 
        private ILevelEntity LevelEntity;
        
        public override void OnDoRecycle() {
            SafeObjectPool<T>.Singleton.Recycle(this as T);
        }
        
        public float GetMinSpawnRange() {
            return minSpawnRange;
        }

        public float GetRealMinSpawnRange()
        {
            return minSpawnRange;
        }

        public float GetRealMaxSpawnRange()
        {
            return maxSpawnRange;
        }

        public int GetRealStartingCredits()
        {
            return startingCredits;
        }

        public int GetRealCurrentCredits()
        {
            return currentCredits;
        }

        public int GetRealCreditsPerSecond()
        {
            return creditsPerSecond;
        }

        public float GetRealSpawnTimer()
        {
            return spawnTimer;
        }

        public float GetRealPackSpawnTimer()
        {
            return packSpawnTimer;
        }

        public ILevelEntity GetLevelEntity()
        {
            return LevelEntity;
        }

        public void SetLevelEntity(ILevelEntity levelEntity)
        {
            LevelEntity = levelEntity;
        }
    }
}