using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Architecture;
using UnityEngine;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.ConfigSheet;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;

namespace Runtime.Spawning
{
    public interface IDirectorViewController : IEntityViewController {
        IDirectorEntity DirectorEntity => Entity as IDirectorEntity;

        public void SetLevelEntity(ILevelEntity levelEntity);
    }
    
    public abstract class DirectorViewController<T>  : AbstractBasicEntityViewController<T>, IDirectorViewController where T : class, IDirectorEntity, new() 
    {
        protected IDirectorModel directorModel;
        protected Lottery m_lottery;
        
        protected ILevelEntity LevelEntity;
        protected int levelNumber;
        
        [Header("Director Info")]
        [SerializeField] public float baseSpawnTimer;
        [SerializeField] public float baseStartingCredits;
        [SerializeField] public float baseCreditsPerSecond;
        
        
        [SerializeField] public float minSpawnRange;
        [SerializeField] public float maxSpawnRange;
        [SerializeField] public float addCreditsInterval = 1f;
        [SerializeField] public LayerMask spawnMask;
        
        [Header("Director Don't Edit")]
        [SerializeField] private float currentCredits;
        [SerializeField] private float creditTimer = 0f;
        [SerializeField] private float directorSpawnTimer = 0f;
        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
        }

        protected override void OnEntityStart()
        {
            Debug.Log("director start");
            m_lottery = new Lottery();
            // currentCredits = BoundEntity.GetStartingCredits().RealValue;
            creditTimer = addCreditsInterval;
            currentCredits = baseStartingCredits;
        }

        protected override IEntity OnBuildNewEntity() {
            return OnBuildNewEntity(1);
        }
        
        protected IEntity OnBuildNewEntity(int level) {
            DirectorBuilder<T> builder = directorModel.GetDirectorBuilder<T>();
            builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level)
            //TODO: set other property base values here
            .SetProperty(new PropertyNameInfo(PropertyName.spawn_timer), baseSpawnTimer)
            .SetProperty(new PropertyNameInfo(PropertyName.credits_per_second), baseCreditsPerSecond)
            .SetProperty(new PropertyNameInfo(PropertyName.starting_credits), baseStartingCredits);
            
            return OnInitDirectorEntity(builder);
        }
        
        protected abstract IEntity OnInitDirectorEntity(DirectorBuilder<T> builder);
        
        protected override void Update() {
            if (BoundEntity == null || String.IsNullOrEmpty(BoundEntity.UUID)) { //not initialized yet
                return;
            }
            base.Update();
            DecrementTimers();
            
            if (creditTimer <= 0f)
            {
                AddCredits();
                creditTimer = addCreditsInterval;
            }

            if(directorSpawnTimer <= 0f)
            {
                AttemptToSpawn();
                directorSpawnTimer = BoundEntity.GetSpawnTimer().RealValue;
            }
        }

        public void SetLevelEntity(ILevelEntity levelEntity)
        {
            LevelEntity = levelEntity;
            levelNumber = levelEntity.GetCurrentLevelCount();
            IEntity ent = OnBuildNewEntity(levelNumber);
            InitWithID(ent.UUID);
        }
        
        private void DecrementTimers()
        {
            creditTimer -= Time.deltaTime;
            directorSpawnTimer -= Time.deltaTime;
        }
        
        protected virtual void AttemptToSpawn()
        {
            //TODO: check if over max enemies

            //pick card, store the card
            List<LevelSpawnCard> cards = LevelEntity.GetAllNormalEnemiesUnderCost(currentCredits);
            Debug.Log("attempt to spawn, under cost cards: " + cards.Count);

            if (cards.Count > 0)
            {
                Debug.Log("cards size: " + cards.Count);
                Debug.Log("lottery: " + m_lottery);
                m_lottery.SetCards(cards);
                LevelSpawnCard selectedCard = m_lottery.PickNextCard();

                bool success = true;
                int maxPackSize = 10; // TODO: Temp to prevent lag
                Debug.Log("spawn success: " + success + "pack size: " + (maxPackSize > 0));
                while (success && maxPackSize > 0)
                {
                    Debug.Log("pack size: " + maxPackSize);
                    success = SpawnEnemy(selectedCard);
                    maxPackSize--;
                }
            }
        }

        protected virtual bool SpawnEnemy(LevelSpawnCard card)
        {
            //determine level and cost
            int rarity = card.MinRarity;
            float cost = card.GetRealSpawnCost(levelNumber, card.MinRarity);
            for (int i = card.MinRarity; i <= card.MaxRarity; i++)
            {
                float checkCost = card.GetRealSpawnCost(levelNumber, i);
                if (currentCredits > checkCost)
                {
                    cost = checkCost;
                    rarity = i;
                    break;
                }
            }
            Debug.Log("start spawn, cost: " + cost + "rarity: " + rarity);
            
            //pick a spot
            Vector3 spawnPos = new Vector3(
                Random.Range(minSpawnRange, maxSpawnRange), 
                10f,  
                Random.Range(minSpawnRange, maxSpawnRange));
                        
            //raycast down from random point within min/max range
            int spawnAttempts = 20;
            while (spawnAttempts > 0)
            {
                if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 30f, spawnMask))
                {
                    Debug.Log("spawn success");
                    spawnPos = hit.point;
                    //TODO: spawn enemy at certain rarity
                    Instantiate(card.Prefab, spawnPos, Quaternion.identity);
                    
                    currentCredits -= cost;
                    return true;
                }
                spawnAttempts--;
            }
            Debug.Log("spawn fail");
            return false;
        }

        protected virtual void AddCredits()
        {
            currentCredits += BoundEntity.GetCreditsPerSecond().RealValue;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minSpawnRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxSpawnRange);
        }
    }
}