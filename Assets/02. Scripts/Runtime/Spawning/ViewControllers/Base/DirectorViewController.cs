using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Utilities;
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

        protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
        protected Vector3[] insideArenaCheckPoints;

        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
        }

        protected override void OnEntityStart() {
            insideArenaCheckPoints =
                GameObject.FindGameObjectsWithTag("ArenaRefPoint").Select(x => x.transform.position).ToArray();

            
            
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
            //check if over max enemies
            if(LevelEntity.CurrentEnemyCount >= LevelEntity.GetMaxEnemyCount())
                return;

            //pick card, store the card
            List<LevelSpawnCard> cards = LevelEntity.GetAllNormalEnemiesUnderCost(currentCredits);

            if (cards.Count > 0)
            {
                m_lottery.SetCards(cards);
                LevelSpawnCard selectedCard = m_lottery.PickNextCard();

                if (default(LevelSpawnCard).Equals(selectedCard)) {
                    return;
                }

                bool success = true;
                int maxPackSize = Random.Range(1, 5);
                while (success && maxPackSize > 0)
                {
                    success = SpawnEnemy(selectedCard);
                    maxPackSize--;
                }
            }
        }

        protected virtual bool SpawnEnemy(LevelSpawnCard card)
        {
            //if over max, return false to stop spawning packs
            if(LevelEntity.CurrentEnemyCount >= LevelEntity.GetMaxEnemyCount())
                return false;

            //determine level and cost
            int rarity = card.MinRarity;
            float cost = card.GetRealSpawnCost(levelNumber, card.MinRarity);
            for (int i = card.MaxRarity; i >= card.MinRarity; i--)
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
            int spawnAttempts = 10;
            while (currentCredits > cost && spawnAttempts > 0)
            {
                
                float angle = Random.Range(0, 360); 
                float radius = Random.Range(minSpawnRange, maxSpawnRange); 


                float x = transform.position.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float z = transform.position.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);

                Vector3 spawnPos = new Vector3(
                    x, 
                    transform.position.y, //+ 500f,  
                    z
                );
                
                //if (Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 600f, spawnMask))
                //{
                     //if (hit.collider.gameObject.layer ==  LayerMask.NameToLayer("Ground")){//PhysicsUtility.IsInLayerMask(hit.collider.gameObject, LayerMask.NameToLayer("Ground"))) {
                        //spawnPos = hit.point;
                        //spawnPos.y += 3f;
                        NavMesh.SamplePosition(spawnPos, out NavMeshHit hitNavMesh, Mathf.Infinity, NavMesh.AllAreas);
                        spawnPos = hitNavMesh.position;

                        Vector3 fixedSpawnPos =
                            SpawningUtility.FindNavMeshSuitablePosition(
                                () => card.Prefab.GetComponent<ICreatureViewController>().SpawnSizeCollider,
                                spawnPos, 90, NavMeshHelper.GetSpawnableAreaMask(), insideArenaCheckPoints, 1f, 3f,
                                spawnAttempts,
                                out int usedAttempts, out _);
                        
                        spawnAttempts -= usedAttempts;
                       
                        if (!float.IsInfinity(fixedSpawnPos.magnitude)) {
                            GameObject spawnedEnemy = CreatureVCFactory.Singleton.SpawnCreatureVC(card.Prefab, fixedSpawnPos, 
                                Quaternion.Euler(0, Random.Range(0, 360), 0),
                                null, rarity,
                                levelNumber, true, 5, 30);
                            IEnemyEntity enemyEntity = spawnedEnemy.GetComponent<IEnemyViewController>().EnemyEntity;
                            onSpawnEnemy?.Invoke(spawnedEnemy, this);
                            // Debug.Log($"Spawn Success: {enemyEntity.EntityName} at {spawnPos} with rarity {rarity} and cost {cost}");
                    
                            currentCredits -= cost;
                            return true;
                        }
                     //}
                //}
                //spawnAttempts--;
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


        public override void OnRecycled() {
            base.OnRecycled();
            onSpawnEnemy = null;
            m_lottery = null;
            LevelEntity = null;
        }
    }
}