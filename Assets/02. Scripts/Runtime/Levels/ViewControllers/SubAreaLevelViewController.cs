using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models;
using AYellowpaper.SerializedCollections;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework.Architecture;
using Runtime.DataFramework.Description;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model;
using Runtime.Spawning;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace _02._Scripts.Runtime.Levels.ViewControllers
{
    public interface ISubAreaLevelViewController: IEntityViewController {
        // public ILevelEntity OnBuildNewLevel(int levelNumber);
        //
        // public void Init();
		      //
        // public void OnExitLevel();
		      //
        HashSet<GameObject> Enemies { get; }
        public void SetLevelNumber(int levelNumber);
        
        public NavMeshModifier GetSubAreaLevelModifierMask();
        public int GetSubAreaLevelModifierAreaMask();

        public ISubAreaLevelEntity OnInitEntity();

        public void StartSpawningCooldown();

        public void InitDirector(IDirectorViewController director);

        public void InitDirectors();

        public void OnExitLevel();
    }
    
    public abstract class SubAreaLevelViewController<T> : AbstractBasicEntityViewController<T>, ISubAreaLevelViewController  where  T : class, ISubAreaLevelEntity, new()  {
        protected override bool CanAutoRemoveEntityWhenLevelEnd { get; }
        private ISubAreaLevelModel subAreaLevelModel;
        private ILevelEntity parentLevelEntity;
        private int levelNumber;
        
        [Header("Enemy Spawning Info")]
        [SerializeField] protected int maxEnemiesPerArea = 20;
        [SerializeField] protected float areaSpawningCooldown = 360f;
        protected float cooldownTimer = 0f;
        
        [Header("SubArea Modifier")]
        [SerializeField] private NavMeshModifier subAreaLevelModifier;
        
        [Header("Enemies")]
        [SerializeField] protected List<SpawnCardListConfig> enemySpawnCardConfigs;
        [SerializeField] protected SerializedDictionary<string, int> maxSpawnPerEnemy = new SerializedDictionary<string, int>();

        [SerializeField] protected int totalEnemyCount = 0;
        private HashSet<IEntity> currentEnemies = new HashSet<IEntity>();
        private List<IEntity> templateEnemies = new List<IEntity>();
        //test
        public int totalEnemiesSpawned;
        public bool isActive = true;

        
        public HashSet<GameObject> Enemies {
            get {
                HashSet<GameObject> enemyPrefabs = new HashSet<GameObject>();
				
                foreach (SpawnCardListConfig spawnCardList in enemySpawnCardConfigs)
                {
                    foreach (EnemySpawnInfo info in spawnCardList.enemySpawnInfos)
                    {
                        enemyPrefabs.Add(info.mainPrefab);
                        if (info.variants != null)
                        {
                            enemyPrefabs.UnionWith(info.variants);
                        }
                    }
                }
                return enemyPrefabs;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            SetUpEnemyCountDictionary();
        }
#endif

        private void SetUpEnemyCountDictionary()
        {
            HashSet<string> enemyNameHashSet = new HashSet<string>();
            foreach (SpawnCardListConfig spawnCardList in enemySpawnCardConfigs)
            {
                foreach (EnemySpawnInfo info in spawnCardList.enemySpawnInfos)
                {
                    enemyNameHashSet.Add(info.mainPrefab.name.Split('_')[0]);
                }
            }

            List<string> keysToRemove = new List<string>();

            foreach (var entry in maxSpawnPerEnemy)
            {
                // If the entry is not in the hashset, add it to the list of keys to remove
                if (!enemyNameHashSet.Contains(entry.Key))
                {
                    keysToRemove.Add(entry.Key);
                }
            }

            // Remove the keys from the dictionary
            foreach (var key in keysToRemove)
            {
                maxSpawnPerEnemy.Remove(key);
            }

            foreach (var entry in enemyNameHashSet)
            {
                // If the entry is not in the dictionary, add it with a value of 1
                if (!maxSpawnPerEnemy.ContainsKey(entry))
                {
                    maxSpawnPerEnemy.Add(entry, 1);
                }
            }
        }
        
        public ISubAreaLevelEntity OnInitEntity(){
            if (subAreaLevelModel == null)
            {
                subAreaLevelModel = this.GetModel<ISubAreaLevelModel>();
            }
            
            SubAreaLevelBuilder<T> builder = subAreaLevelModel.GetSubAreaLevelBuilder<T>();
            builder
                .SetProperty(new PropertyNameInfo(PropertyName.spawn_cards), CreateTemplateEnemies(levelNumber))
                .SetProperty(new PropertyNameInfo(PropertyName.max_enemies), maxEnemiesPerArea)
                .SetProperty(new PropertyNameInfo(PropertyName.max_spawn_per_enemy), maxSpawnPerEnemy)
                .SetProperty(new PropertyNameInfo(PropertyName.sub_area_nav_mesh_modifier), CalculateSubAreaMaskIndex());

            return OnInitSubLevelEntity(builder);
        }
        
        protected int CalculateSubAreaMaskIndex()
        {
            return (int) Mathf.Pow(2, subAreaLevelModifier.area);
        }

        protected override void OnEntityStart()
        {
            // initialize sub area count of enemies
            BoundEntity.InitializeEnemyCountDictionary();
        }

        protected override void OnEntityRecycled(IEntity ent)
        {
            base.OnEntityRecycled(ent);
            OnExitLevel();
            cooldownTimer = 0f;
            IEnemyEntityModel enemyModel = this.GetModel<IEnemyEntityModel>();
            foreach (IEntity enemyEntity in templateEnemies) {
                enemyModel.RemoveEntity(enemyEntity.UUID, true);
            }
            templateEnemies.Clear();
            OnRecycled();
        }

        protected abstract ISubAreaLevelEntity OnInitSubLevelEntity(SubAreaLevelBuilder<T> builder);

        public List<LevelSpawnCard[]> CreateTemplateEnemies(int levelNumber) {
            List<LevelSpawnCard[]> spawnCards = new List<LevelSpawnCard[]>();

            foreach (var spawnCardList in enemySpawnCardConfigs)
            {
                LevelSpawnCard[] cards = new LevelSpawnCard[spawnCardList.enemySpawnInfos.Length];
                for (int card_index = 0; card_index < cards.Length; card_index++)
                {
                    EnemySpawnInfo enemyInfo = spawnCardList.enemySpawnInfos[card_index];
                    GameObject prefab = enemyInfo.mainPrefab;
                    ICreatureViewController enemyViewController = prefab.GetComponent<ICreatureViewController>();
                    IEnemyEntity enemyEntity = enemyViewController.OnInitEntity(levelNumber, 1) as IEnemyEntity;

                    string[] prefabNames = new string[(enemyInfo.variants?.Count ?? 0) + 1];
                    prefabNames[0] = prefab.name;
                    for (int i = 0; i < enemyInfo.variants.Count; i++)
                    {
                        prefabNames[i + 1] = enemyInfo.variants[i].name;
                    }

                    templateEnemies.Add(enemyEntity);
                    cards[card_index] = new LevelSpawnCard(enemyEntity, enemyEntity.GetRealSpawnWeight(levelNumber),
                        prefabNames,
                        enemyInfo.minRarity, enemyInfo.maxRarity);
                }
                spawnCards.Add(cards);
            }

            return spawnCards;
        }
        
        protected override IEntity OnBuildNewEntity() {
            int level = this.GetModel<ILevelModel>().CurrentLevelCount.Value;
            return OnInitEntity();
        }

        protected override void Update()
        {
            base.Update();
            if(BoundEntity == null) return;
            if (!BoundEntity.IsActiveSpawner) {
                isActive = false;
                if (cooldownTimer <= areaSpawningCooldown)
                {
                    cooldownTimer += Time.deltaTime;
                }
                else
                {
                    //off cooldown
                    BoundEntity.IsActiveSpawner = true;
                    BoundEntity.TotalEnemiesSpawnedSinceOffCooldown = 0;
                    totalEnemiesSpawned = 0;
                }
            }
        }

        public void InitDirector(IDirectorViewController director)
        {
            director.RegisterOnSpawnEnemy(OnSpawnEnemy).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        }

        public void InitDirectors()
        {
            //director.RegisterOnSpawnEnemy(OnSpawnEnemy).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);();
            //Tim update: get all directors in children and init
            
        }
        
        private void OnSpawnEnemy(GameObject enemyObject, IDirectorViewController director) {
            IEnemyViewController enemyViewController = enemyObject.GetComponent<IEnemyViewController>();

            if(enemyViewController.EnemyEntity.SpawnedAreaIndex == CalculateSubAreaMaskIndex()) {
                OnInitEnemy(enemyViewController);
            }
        }
        
        private void OnInitEnemy(IEnemyViewController enemyVC) {
        	IEnemyEntity enemyEntity = enemyVC.EnemyEntity;
        	enemyEntity.RegisterOnEntityRecycled(OnEnemyEntityRecycled)
        		.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        	totalEnemyCount++;
            BoundEntity.IncrementEnemyCountDictionary(enemyEntity.EntityName);
        	BoundEntity.CurrentEnemyCount++;
            BoundEntity.TotalEnemiesSpawnedSinceOffCooldown++;
            
            totalEnemiesSpawned++;
        	currentEnemies.Add(enemyEntity);
            
            if(BoundEntity.TotalEnemiesSpawnedSinceOffCooldown >= BoundEntity.GetMaxEnemyCount()) {
                StartSpawningCooldown();
            }
        }
        
        private void OnEnemyEntityRecycled(IEntity enemy) {
        	enemy.UnRegisterOnEntityRecycled(OnEnemyEntityRecycled);
        	totalEnemyCount--;
            BoundEntity.DecrementEnemyCountDictionary(enemy.EntityName);
        	BoundEntity.CurrentEnemyCount = Mathf.Max(0, BoundEntity.CurrentEnemyCount - 1);
        	currentEnemies.Remove(enemy);
        }
        
        public void OnExitLevel() {
            IEnemyEntityModel enemyModel = this.GetModel<IEnemyEntityModel>();
            
            while (currentEnemies.Count > 0) {
            	IEntity enemy = currentEnemies.First();
            	currentEnemies.Remove(enemy);
            	enemyModel.RemoveEntity(enemy.UUID, true);
            }
            currentEnemies.Clear();
        }

        public override void OnRecycled()
        {
            base.OnRecycled();
            totalEnemyCount = 0;
            BoundEntity?.ClearEnemyCountDictionary();
            currentEnemies.Clear();
        }

        public void StartSpawningCooldown()
        {
            cooldownTimer = 0f;
            BoundEntity.IsActiveSpawner = false;
        }

        public void SetLevelNumber(int levelNumber) {
            this.levelNumber = levelNumber;
        }

        public NavMeshModifier GetSubAreaLevelModifierMask()
        {
            return subAreaLevelModifier;
        }
        
        public int GetSubAreaLevelModifierAreaMask()
        {
            return subAreaLevelModifier.area;
        }
    }
}