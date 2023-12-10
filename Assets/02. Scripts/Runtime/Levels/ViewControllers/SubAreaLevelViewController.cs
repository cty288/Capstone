using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework.Architecture;
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
        List<GameObject> Enemies { get; }
        public void SetLevelNumber(int levelNumber);
        
        public NavMeshModifier GetSubAreaLevelModifierMask();
        public int GetSubAreaLevelModifierAreaMask();

        public ISubAreaLevelEntity OnInitEntity();

        public void StartSpawningCooldown();

        public void InitDirector(IDirectorViewController director);

        public void InitDirectors();
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
        
        [FormerlySerializedAs("subAreaLevelModifierMask")]
        [Header("SubArea Modifier")]
        [SerializeField] private NavMeshModifier subAreaLevelModifier;
        
        [Header("Enemies")]
        [SerializeField] protected List<LevelEnemyPrefabConfig> enemies = new List<LevelEnemyPrefabConfig>();
        [SerializeField] protected int enemyCount = 0;
        private HashSet<IEntity> currentEnemies = new HashSet<IEntity>();
        
        public List<GameObject> Enemies {
            get {
                List<GameObject> enemyPrefabs = new List<GameObject>();
                foreach (var enemy in enemies) {
                    enemyPrefabs.Add(enemy.mainPrefab);
                    if (enemy.variants != null) {
                        enemyPrefabs.AddRange(enemy.variants);
                    }
                }
                return enemyPrefabs;
            }
        }

        public ISubAreaLevelEntity OnInitEntity(){
            if (subAreaLevelModel == null) {
                subAreaLevelModel = this.GetModel<ISubAreaLevelModel>();
            }

            SubAreaLevelBuilder<T> builder = subAreaLevelModel.GetSubAreaLevelBuilder<T>();
            builder
                .SetProperty(new PropertyNameInfo(PropertyName.spawn_cards), CreateTemplateEnemies(levelNumber))
                .SetProperty(new PropertyNameInfo(PropertyName.max_enemies), maxEnemiesPerArea)
                .SetProperty(new PropertyNameInfo(PropertyName.sub_area_nav_mesh_modifier), CalculateSubAreaMaskIndex());

            return OnInitSubLevelEntity(builder);
        }

        protected int CalculateSubAreaMaskIndex()
        {
            return (int) Mathf.Pow(2, subAreaLevelModifier.area);
        }
        
        protected abstract ISubAreaLevelEntity OnInitSubLevelEntity(SubAreaLevelBuilder<T> builder);

        public List<LevelSpawnCard> CreateTemplateEnemies(int levelNumber) {
            List<LevelSpawnCard> spawnCards = new List<LevelSpawnCard>();
			
            foreach (var enemy in enemies) {
                GameObject prefab = enemy.mainPrefab;
                ICreatureViewController enemyViewController = prefab.GetComponent<ICreatureViewController>();
                IEnemyEntity enemyEntity = enemyViewController.OnInitEntity(levelNumber, 1) as IEnemyEntity;
				
				
                string[] prefabNames = new string[
                    (enemy.variants?.Count ?? 0) + 1];
                prefabNames[0] = prefab.name;
                for (int i = 0; i < enemy.variants.Count; i++) {
                    prefabNames[i + 1] = enemy.variants[i].name;
                }
				
                //templateEnemies.Add(enemyEntity);
                spawnCards.Add(new LevelSpawnCard(enemyEntity, enemyEntity.GetRealSpawnWeight(levelNumber), prefabNames,
                    enemy.minRarity, enemy.maxRarity));
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
            
            if (!BoundEntity.IsActiveSpawner) {
                if (cooldownTimer <= areaSpawningCooldown)
                {
                    cooldownTimer += Time.deltaTime;
                }
                else
                {
                    //off cooldown
                    BoundEntity.IsActiveSpawner = true;
                    BoundEntity.TotalEnemiesSpawnedSinceOffCooldown = 0;
                }
            }
        }

        public void InitDirector(IDirectorViewController director)
        {
            director.RegisterOnSpawnEnemy(OnSpawnEnemy).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        }

        public void InitDirectors()
        {
            //director.RegisterOnSpawnEnemy(OnSpawnEnemy).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            UpdatePreExistingEnemies();
            //Tim update: get all directors in children and init
            
        }
        
        private void OnSpawnEnemy(GameObject enemyObject, IDirectorViewController director) {
            IEnemyViewController enemyViewController = enemyObject.GetComponent<IEnemyViewController>();

            if(enemyViewController.EnemyEntity.SpawnedAreaIndex == CalculateSubAreaMaskIndex()) {
                OnInitEnemy(enemyViewController);
            }
        }
        
        private void UpdatePreExistingEnemies() {
        	IEnemyViewController[] enemies = GetComponentsInChildren<IEnemyViewController>(true);
        	foreach (var enemy in enemies) {
        		enemy.RegisterOnEntityViewControllerInit(OnExistingEnemyInit)
        			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        	}
        }

        private void OnExistingEnemyInit(IEntityViewController entity) {
        	entity.UnRegisterOnEntityViewControllerInit(OnExistingEnemyInit);
        	OnInitEnemy(entity as IEnemyViewController);
        }
        
        private void OnInitEnemy(IEnemyViewController enemyObject) {
        	IEnemyEntity enemyEntity = enemyObject.EnemyEntity;
        	enemyEntity.RegisterOnEntityRecycled(OnEnemyEntityRecycled)
        		.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        	enemyCount++;
        	BoundEntity.CurrentEnemyCount++;
            BoundEntity.TotalEnemiesSpawnedSinceOffCooldown++;
        	currentEnemies.Add(enemyEntity);
            
            if(BoundEntity.TotalEnemiesSpawnedSinceOffCooldown >= BoundEntity.GetMaxEnemyCount()) {
                StartSpawningCooldown();
            }
        }
        
        private void OnEnemyEntityRecycled(IEntity enemy) {
        	enemy.UnRegisterOnEntityRecycled(OnEnemyEntityRecycled);
        	enemyCount--;
        	BoundEntity.CurrentEnemyCount = Mathf.Max(0, BoundEntity.CurrentEnemyCount - 1);
        	currentEnemies.Remove(enemy);
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