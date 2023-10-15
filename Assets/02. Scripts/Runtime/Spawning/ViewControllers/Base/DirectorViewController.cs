using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Utilities;
using UnityEngine;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Utilities.ConfigSheet;
using UnityEngine.AI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;

namespace Runtime.Spawning
{
    public class DirectorOnSpawnEnemyUnregister : IUnRegister
    {
        private Action<GameObject, IDirectorViewController> onSpawnEnemy;

        private IDirectorViewController director;
		
        public DirectorOnSpawnEnemyUnregister(IDirectorViewController director, Action<GameObject, IDirectorViewController> onSpawnEnemy) {
            this.director = director;
            this.onSpawnEnemy = onSpawnEnemy;
        }

        public void UnRegister() {
            director.UnregisterOnSpawnEnemy(onSpawnEnemy);
            director = null;
        }
    }
    
    public interface IDirectorViewController : IEntityViewController {
        IDirectorEntity DirectorEntity => Entity as IDirectorEntity;

        public void SetLevelEntity(ILevelEntity levelEntity);

        public IUnRegister RegisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy);
        
        public void UnregisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy);
    }
    
    public abstract class DirectorViewController<T>  : AbstractBasicEntityViewController<T>, IDirectorViewController where T : class, IDirectorEntity, new() 
    {
        protected IDirectorModel directorModel;
        protected Lottery m_lottery;
        
        protected ILevelEntity LevelEntity;
        protected int levelNumber;
        protected Action<GameObject, IDirectorViewController> onSpawnEnemy;

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
            if (BoundEntity == null || String.IsNullOrEmpty(BoundEntity.UUID) || LevelEntity == null) { //not initialized yet
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

        public void SetLevelEntity(ILevelEntity levelEntity) {
            LevelEntity = levelEntity;
            levelNumber = levelEntity.GetCurrentLevelCount();
            IEntity ent = OnBuildNewEntity(levelNumber);
            InitWithID(ent.UUID);
            
            //for some reason we need to do this again
            LevelEntity = levelEntity;
            levelNumber = levelEntity.GetCurrentLevelCount();
        }

        public IUnRegister RegisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
            this.onSpawnEnemy += onSpawnEnemy;
            return new DirectorOnSpawnEnemyUnregister(this, onSpawnEnemy);
        }

        public void UnregisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
            this.onSpawnEnemy -= onSpawnEnemy;
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

                if (default(LevelSpawnCard).Equals(selectedCard)) {
                    return;
                }

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
            for (int i = card.MaxRarity; i <= card.MinRarity; i++)
            {
                float checkCost = card.GetRealSpawnCost(levelNumber, i);
                if (currentCredits > checkCost)
                {
                    cost = checkCost;
                    rarity = i;
                    break;
                }
            }
                        
            //raycast down from random point within min/max range
            int spawnAttempts = 20;
            while (spawnAttempts > 0 && currentCredits > cost)
            {
                //pick a spot
                Vector3 spawnPos = new Vector3(
                    transform.position.x + Random.Range(minSpawnRange, maxSpawnRange), 
                    transform.position.y + 500f,  
                    transform.position.z + Random.Range(minSpawnRange, maxSpawnRange));
                
                if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 600f, spawnMask))
                {
                     if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground")){//PhysicsUtility.IsInLayerMask(hit.collider.gameObject, LayerMask.NameToLayer("Ground"))) {
                        //Debug.Log("spawn success: " + card.EntityName + ", location: " + hit.point);
                        spawnPos = hit.point;
                        spawnPos.y += 3f;
                        NavMesh.SamplePosition(spawnPos, out NavMeshHit hitNavMesh, Mathf.Infinity, NavMesh.AllAreas);
                        spawnPos = hitNavMesh.position;
                        //TODO: spawn enemy at certain rarity
                        //Instantiate(card.Prefab, spawnPos, Quaternion.identity);
                        GameObject spawnedEnemy = EnemyVCFactory.Singleton.SpawnEnemyVC(card.Prefab, spawnPos, Quaternion.identity, null, rarity,
                            levelNumber, true, 5, 30);
                        IEnemyEntity enemyEntity = spawnedEnemy.GetComponent<IEnemyViewController>().EnemyEntity;
                        onSpawnEnemy?.Invoke(spawnedEnemy, this);
                        Debug.Log($"Spawn Success: {enemyEntity.EntityName} at {spawnPos} with rarity {rarity} and cost {cost}");
                    
                        currentCredits -= cost;
                        return true;
                     }
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


        public override void OnRecycled() {
            base.OnRecycled();
            onSpawnEnemy = null;
            m_lottery = null;
            LevelEntity = null;
        }
    }
}