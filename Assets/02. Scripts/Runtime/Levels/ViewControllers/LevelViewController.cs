using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.Properties;
using _02._Scripts.Runtime.Utilities;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Weapons.Model.Builders;
using UnityEngine;
using UnityEngine.AI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;


namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public interface ILevelViewController: IEntityViewController {
		public void SetLevelNumber(int levelNumber);
		public ILevelEntity OnBuildNewLevel(int levelNumber);

		public void Init();
		
		List<GameObject> Enemies { get; }
	}

	public struct LevelSpawnCard : ICanGetModel {
		[field: ES3Serializable]
		public string TemplateEntityUUID { get; }

		public IEnemyEntity TemplateEntity {
			get {
				return this.GetModel<IEnemyEntityModel>().GetEntity(TemplateEntityUUID);
			}
		}
		
		[field: ES3Serializable]
		public int RealSpawnWeight { get; }
		//[field: ES3Serializable]
		//public int RealSpawnCost { get; }
		[field: ES3Serializable]
		public string PrefabName { get; }
		
		[field: ES3Serializable]
		public bool IsNormalEnemy { get; }
		
		[field: ES3Serializable]
		public int MinRarity { get; }
		
		[field: ES3Serializable]
		public int MaxRarity { get; }

		public GameObject Prefab => GlobalLevelManager.Singleton.GetEnemyPrefab(PrefabName);

		public string EntityName => TemplateEntity.EntityName;

		public float GetRealSpawnCost(int level, int rarity) {
			return TemplateEntity.GetRealSpawnCost(level, rarity);
		} 
		
		public LevelSpawnCard(IEnemyEntity templateEntity, int realSpawnWeight, string prefabName, int minRarity, int maxRarity) {
			TemplateEntityUUID = templateEntity.UUID;
			RealSpawnWeight = realSpawnWeight;
			//RealSpawnCost = realSpawnCost;
			PrefabName = prefabName;
			IsNormalEnemy = templateEntity is INormalEnemyEntity;
			MinRarity = minRarity;
			MaxRarity = maxRarity;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}

	[Serializable]
	public struct LevelEnemyPrefabConfig {
		public GameObject prefab;
		public int minRarity;
		public int maxRarity;
	}
	
	
	[RequireComponent(typeof(NavMeshSurface))]
	public abstract class LevelViewController<T> : AbstractBasicEntityViewController<T>, ILevelViewController
		where  T : class, ILevelEntity, new() {

		[Header("Player")] 
		[SerializeField] protected List<Transform> playerSpawnPoints = new List<Transform>();
		
		[Header("Enemies")]
		[SerializeField] protected List<LevelEnemyPrefabConfig> enemies = new List<LevelEnemyPrefabConfig>();

		public List<GameObject> Enemies => enemies.Select(e => e.prefab).ToList();
		
		[SerializeField] protected int maxEnemiesBaseValue = 50;

		//private List<IEnemyEntity> templateEnemies = new List<IEnemyEntity>();
		private ILevelModel levelModel;
		private int levelNumber;
		private NavMeshSurface navMeshSurface;
		protected override void Awake() {
			base.Awake();
			levelModel = this.GetModel<ILevelModel>();
			navMeshSurface = GetComponent<NavMeshSurface>();
			
			
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildNewLevel(levelNumber);
		}
	
		
		protected abstract IEntity OnInitLevelEntity(LevelBuilder<T> builder, int levelNumber);


		public void SetLevelNumber(int levelNumber) {
			this.levelNumber = levelNumber;
		}

		public List<LevelSpawnCard> CreateTemplateEnemies(int levelNumber) {
			List<LevelSpawnCard> spawnCards = new List<LevelSpawnCard>();
			
			foreach (var enemy in enemies) {
				GameObject prefab = enemy.prefab;
				IEnemyViewController enemyViewController = prefab.GetComponent<IEnemyViewController>();
				IEnemyEntity enemyEntity = enemyViewController.OnInitEntity();
				
				//templateEnemies.Add(enemyEntity);
				spawnCards.Add(new LevelSpawnCard(enemyEntity, enemyEntity.GetRealSpawnWeight(levelNumber), prefab.name,
					enemy.minRarity, enemy.maxRarity));
			}
			
			return spawnCards;
		}
		public ILevelEntity OnBuildNewLevel(int levelNumber) {
			if (levelModel == null) {
				levelModel = this.GetModel<ILevelModel>();
			}
			LevelBuilder<T> builder = levelModel.GetLevelBuilder<T>();
			builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), levelNumber)
				.SetProperty(new PropertyNameInfo(PropertyName.max_enemies), maxEnemiesBaseValue)
				.SetProperty(new PropertyNameInfo(PropertyName.spawn_cards), CreateTemplateEnemies(levelNumber));

			return OnInitLevelEntity(builder, levelNumber) as ILevelEntity;
		}

		public void Init() {
			//navMeshSurface.BuildNavMesh();
			//navMeshSurface.navMeshData 
			UpdateNavMesh();
			
			OnSpawnPlayer();
		}

		protected override void Update() {
			base.Update();
			if (Input.GetKeyDown(KeyCode.F6)) {
				UpdateNavMesh();
			}
		}
		
		protected void UpdateNavMesh() {
			NavMeshBuildSettings buildSettings = navMeshSurface.GetBuildSettings();
			buildSettings.preserveTilesOutsideBounds = true;
			navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData, buildSettings);
		}


		protected virtual void OnSpawnPlayer() {
			if (playerSpawnPoints.Count == 0) {
				throw new Exception("No player spawn points found for level {gameObject.name}");
				return;
			}
			this.SendCommand<TeleportPlayerCommand>(
				TeleportPlayerCommand.Allocate(playerSpawnPoints.GetRandomElement().position));
		}

		public override void OnRecycled() {
			base.OnRecycled();
		}
	}
}