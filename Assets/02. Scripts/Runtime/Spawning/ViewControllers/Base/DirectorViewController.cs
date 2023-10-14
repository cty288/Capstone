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
        [SerializeField] public float minSpawnRange;
        [SerializeField] public float maxSpawnRange;
        [SerializeField] public float addCreditsInterval = 1f;
        [SerializeField] public LayerMask spawnMask;
        
        [ReadOnly(true)] private float currentCredits;
        [ReadOnly(true)] private float creditTimer = 0f;
        [ReadOnly(true)] private float directorSpawnTimer = 0f;
        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
        }

        protected override void OnEntityStart()
        {
            m_lottery = new Lottery();
            currentCredits = BoundEntity.GetStartingCredits().RealValue;
            creditTimer = addCreditsInterval;
        }

        protected override IEntity OnBuildNewEntity() {
            return OnBuildNewEntity(1);
        }
        
        protected IEntity OnBuildNewEntity(int level) {
            DirectorBuilder<T> builder = directorModel.GetDirectorBuilder<T>();
            builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level);
            //TODO: set other property base values here
            //    .SetProperty(new PropertyNameInfo(PropertyName.spawn_timer), baseSpawnTimer)
              //  .SetProperty(new PropertyNameInfo(PropertyName.starting_credits), baseStartingCredits);
            
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
        
        public virtual void SetSpawnCards(List<LevelSpawnCard> spawnCards)
        {
            m_lottery.SetCards(spawnCards);
        }

        protected virtual void AttemptToSpawn()
        {
            //TODO: check if over max enemies

            //pick card, store the card
            m_lottery.SetCards(LevelEntity.GetAllCardsUnderCost(currentCredits));
            LevelSpawnCard selectedCard = m_lottery.PickNextCard();

            bool success = true;
            int maxPackSize = 10; // TODO: Temp to prevent lag
            while(success && maxPackSize > 0)
            {
                success = SpawnEnemy(selectedCard);
                maxPackSize--;
            }
        }

        protected virtual bool SpawnEnemy(LevelSpawnCard card)
        {
            //determine level and cost
            for (int i = card.MinRarity; i <= card.MaxRarity; i++)
            {
                float cost = card.GetRealSpawnCost(levelNumber, i);
                if (currentCredits >= cost)
                {
                    //pick a spot
                    Vector3 spawnPos = new Vector3(
                        Random.Range(minSpawnRange, maxSpawnRange), 
                        10f,  
                        Random.Range(minSpawnRange, maxSpawnRange));
                        
                    //raycast down from random point within min/max range
                    if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 30f, spawnMask))
                    {
                        //spawn
                        spawnPos = hit.point;
                        Instantiate(card.Prefab, spawnPos, Quaternion.identity);
                        return true;
                    }
                        
                    currentCredits -= cost;
                    break;
                }
            }
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