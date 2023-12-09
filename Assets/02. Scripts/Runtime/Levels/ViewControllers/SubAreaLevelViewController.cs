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
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using UnityEngine.AI;
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
    }
    
    public abstract class SubAreaLevelViewController<T> : AbstractBasicEntityViewController<T>, ISubAreaLevelViewController  where  T : class, ISubAreaLevelEntity, new()  {
        protected override bool CanAutoRemoveEntityWhenLevelEnd { get; }
        private ISubAreaLevelModel subAreaLevelModel;
        private ILevelEntity parentLevelEntity;
        private int levelNumber;
        
        [Header("SubArea Modifier Mask")]
        [SerializeField] private NavMeshModifier subAreaLevelModifierMask;
        
        [Header("Enemies")]
        [SerializeField] protected List<LevelEnemyPrefabConfig> enemies = new List<LevelEnemyPrefabConfig>();
        
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
                .SetProperty(new PropertyNameInfo(PropertyName.sub_area_nav_mesh_modifier), subAreaLevelModifierMask.area);

            return OnInitSubLevelEntity(builder);
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


        public void SetLevelNumber(int levelNumber) {
            this.levelNumber = levelNumber;
        }

        public NavMeshModifier GetSubAreaLevelModifierMask()
        {
            return subAreaLevelModifierMask;
        }
        
        public int GetSubAreaLevelModifierAreaMask()
        {
            return subAreaLevelModifierMask.area;
        }
    }
}