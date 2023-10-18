﻿using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.Properties;
using _02._Scripts.Runtime.Utilities;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Spawning;
using Runtime.Temporary;
using Runtime.Utilities;
using Runtime.Weapons.Model.Builders;
using UnityEngine;
using UnityEngine.AI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;


namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public interface ILevelViewController: IEntityViewController {
		public void SetLevelNumber(int levelNumber);
		public ILevelEntity OnBuildNewLevel(int levelNumber);

		public void Init();
		
		public void OnExitLevel();
		
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

		public override bool Equals(object obj) {
			if (obj is LevelSpawnCard other) {
				return other.TemplateEntityUUID == TemplateEntityUUID;
			}

			return false;
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

		//[SerializeField] protected List<LevelEnemyPrefabConfig> bosses = new List<LevelEnemyPrefabConfig>();

		[SerializeField] protected GameObject playerSpawner;


		public List<GameObject> Enemies => enemies.Select(e => e.prefab).ToList();
		
		[SerializeField] protected int maxEnemiesBaseValue = 50;

		//private List<IEnemyEntity> templateEnemies = new List<IEnemyEntity>();
		private ILevelModel levelModel;
		private int levelNumber;
		private NavMeshSurface navMeshSurface;

		private HashSet<IDirectorViewController> playerSpawners = new HashSet<IDirectorViewController>();

		private HashSet<IEntity> currentEnemies = new HashSet<IEntity>();
		[SerializeField] protected bool autoUpdateNavMeshOnStart = true;
		[Header("Debug Only")]
		[SerializeField]
		private int enemyCount = 0;
 
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;

		protected override void Awake() {
			base.Awake();
			levelModel = this.GetModel<ILevelModel>();
			navMeshSurface = GetComponent<NavMeshSurface>();
		//	enemies.AddRange(bosses);

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
				IEnemyEntity enemyEntity = enemyViewController.OnInitEntity(levelNumber, 1);
				
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
			if (autoCreateNewEntityWhenStart) {
				UpdateNavMesh();
			}
			UpdatePreExistingEnemies();
			UpdatePreExistingDirectors();
			OnSpawnPlayer();
		}

		

		private void UpdatePreExistingDirectors() {
			IDirectorViewController[] directors = GetComponentsInChildren<IDirectorViewController>(true);
			foreach (var directorViewController in directors) {
				InitDirector(directorViewController);
			}
		}
		
		protected void InitDirector(IDirectorViewController director) {
			director.SetLevelEntity(BoundEntity);
			
			
			director.RegisterOnSpawnEnemy(OnSpawnEnemy).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnSpawnEnemy(GameObject enemyObject, IDirectorViewController director) {
			OnInitEnemy(enemyObject.GetComponent<IEnemyViewController>());
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
			currentEnemies.Add(enemyEntity);
		}

		private void OnEnemyEntityRecycled(IEntity enemy) {
			enemy.UnRegisterOnEntityRecycled(OnEnemyEntityRecycled);
			enemyCount--;
			BoundEntity.CurrentEnemyCount = Mathf.Max(0, BoundEntity.CurrentEnemyCount - 1);
			currentEnemies.Remove(enemy);
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

			if (playerSpawner) {
				HashSet<PlayerController> players = PlayerController.GetAllPlayers();
				SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePool(playerSpawner, 10, 100);
				foreach (var player in players) {
					GameObject spawner = pool.Allocate();
					
					
					IDirectorViewController director = spawner.GetComponent<IDirectorViewController>();
					playerSpawners.Add(director);
					InitDirector(director);
					spawner.transform.SetParent(player.transform);
					spawner.transform.localPosition = Vector3.zero;
					spawner.transform.localRotation = Quaternion.identity;
					spawner.transform.localScale = Vector3.one;
				}
			}
			
			
		}

		public override void OnRecycled() {
			base.OnRecycled();
			enemyCount = 0;
			currentEnemies.Clear();
			playerSpawners.Clear();
		}
		
		public void OnExitLevel() {
			BoundEntity.CurrentEnemyCount = 0;
			IEnemyEntityModel enemyModel = this.GetModel<IEnemyEntityModel>();
			IDirectorModel directorModel = this.GetModel<IDirectorModel>();
			
			while (currentEnemies.Count > 0) {
				IEntity enemy = currentEnemies.First();
				currentEnemies.Remove(enemy);
				
				enemyModel.RemoveEntity(enemy.UUID);
			}
			
			IDirectorViewController[] directors = GetComponentsInChildren<IDirectorViewController>(true);
			foreach (var directorViewController in directors) {
				directorModel.RemoveEntity(directorViewController.Entity.UUID);
			}

			foreach (IDirectorViewController spawner in playerSpawners) {
				directorModel.RemoveEntity(spawner.Entity.UUID);
			}
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			OnRecycled();
		}
	}
}