using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.Properties;
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
		public ILevelEntity OnBuildNewLevel();

		public void Init();
	}

	public struct LevelSpawnCard {
		public IEnemyEntity TemplateEntity { get; }
		public int RealSpawnWeight { get; }
		public int RealSpawnCost { get; }
		public GameObject Prefab { get; }

		public string EntityName => TemplateEntity.EntityName;
		
		public LevelSpawnCard(IEnemyEntity templateEntity, int realSpawnWeight, int realSpawnCost, GameObject prefab) {
			TemplateEntity = templateEntity;
			RealSpawnWeight = realSpawnWeight;
			RealSpawnCost = realSpawnCost;
			Prefab = prefab;
		}
	}
	public abstract class LevelViewController<T> : AbstractBasicEntityViewController<T>, ILevelViewController
		where  T : class, ILevelEntity, new()  {
		
		[SerializeField] protected List<GameObject> enemies = new List<GameObject>();
		[SerializeField] protected int maxEnemiesBaseValue = 50;

		private List<IEnemyEntity> templateEnemies = new List<IEnemyEntity>();
		private ILevelModel levelModel;
		private int levelNumber;
		
		protected override void Awake() {
			base.Awake();
			levelModel = this.GetModel<ILevelModel>();
		}

		protected override IEntity OnBuildNewEntity() {
			LevelBuilder<T> builder = levelModel.GetLevelBuilder<T>();
			builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), levelNumber)
				.SetProperty(new PropertyNameInfo(PropertyName.max_enemies), maxEnemiesBaseValue)
				.SetProperty(new PropertyNameInfo(PropertyName.spawn_cards), CreateTemplateEnemies());
			
			return OnInitLevelEntity(builder, levelNumber);
		}

		protected abstract IEntity OnInitLevelEntity(LevelBuilder<T> builder, int levelNumber);


		public void SetLevelNumber(int levelNumber) {
			this.levelNumber = levelNumber;
		}

		public List<LevelSpawnCard> CreateTemplateEnemies() {
			List<LevelSpawnCard> spawnCards = new List<LevelSpawnCard>();
			foreach (var enemy in enemies) {
				IEnemyViewController enemyViewController = enemy.GetComponent<IEnemyViewController>();
				IEnemyEntity enemyEntity = enemyViewController.OnInitEntity();
				
				templateEnemies.Add(enemyEntity);
				spawnCards.Add(new LevelSpawnCard(enemyEntity, enemyEntity.GetRealSpawnWeight(levelNumber),
					enemyEntity.GetRealSpawnCost(levelNumber), enemy));
			}
			
			return spawnCards;
		}
		public ILevelEntity OnBuildNewLevel() {
			return OnBuildNewEntity() as ILevelEntity;
		}

		public void Init() {
			
		}

		public override void OnRecycled() {
			base.OnRecycled();
			foreach (var enemy in templateEnemies) {
				enemy.RecycleToCache();
			}
			templateEnemies.Clear();
		}
	}
}