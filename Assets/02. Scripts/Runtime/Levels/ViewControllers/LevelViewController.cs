using System;
using System.Collections.Generic;
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
		[field: ES3Serializable]
		public int RealSpawnCost { get; }
		[field: ES3Serializable]
		public string PrefabName { get; }

		public GameObject Prefab => GlobalLevelManager.Singleton.GetEnemyPrefab(PrefabName);

		public string EntityName => TemplateEntity.EntityName;
		
		public LevelSpawnCard(IEnemyEntity templateEntity, int realSpawnWeight, int realSpawnCost, string prefabName) {
			TemplateEntityUUID = templateEntity.UUID;
			RealSpawnWeight = realSpawnWeight;
			RealSpawnCost = realSpawnCost;
			PrefabName = prefabName;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}

		public int GetSpawnCostGivenRarity(int rarity)
		{
			//TODO: get real function, probably turn into "baseSpawnCost * rarity"
			return RealSpawnCost * rarity;
		}
	}
	public abstract class LevelViewController<T> : AbstractBasicEntityViewController<T>, ILevelViewController
		where  T : class, ILevelEntity, new() {

		[Header("Player")] 
		[SerializeField] protected List<Transform> playerSpawnPoints = new List<Transform>();
		
		[Header("Enemies")]
		[SerializeField] protected List<GameObject> enemies = new List<GameObject>();

		public List<GameObject> Enemies => enemies;
		
		[SerializeField] protected int maxEnemiesBaseValue = 50;

		//private List<IEnemyEntity> templateEnemies = new List<IEnemyEntity>();
		private ILevelModel levelModel;
		private int levelNumber;
		
		protected override void Awake() {
			base.Awake();
			levelModel = this.GetModel<ILevelModel>();
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
				IEnemyViewController enemyViewController = enemy.GetComponent<IEnemyViewController>();
				IEnemyEntity enemyEntity = enemyViewController.OnInitEntity();
				
				//templateEnemies.Add(enemyEntity);
				spawnCards.Add(new LevelSpawnCard(enemyEntity, enemyEntity.GetRealSpawnWeight(levelNumber),
					enemyEntity.GetRealSpawnCost(levelNumber), enemy.name));
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
			OnSpawnPlayer();
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