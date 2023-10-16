using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Models {
	public interface ILevelModel : IEntityModel, IModel {
		BindableProperty<int> CurrentLevelCount { get; }
		
		LevelBuilder<T> GetLevelBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, ILevelEntity, new();
		
		BindableProperty<ILevelEntity> CurrentLevel { get;}
		
		void AddLevel(ILevelEntity level);
		
		bool IsLevelSpawned(int levelNumber);
		
		void SwitchToLevel(int levelNumber);
		
		ILevelEntity GetLevel(int levelNumber);
		
	}

	public struct OnTryToSwitchUnSpawnedLevel {
		public int LevelNumber;
	}
	public class LevelModel: EntityModel<ILevelEntity>, ILevelModel {
		[field: ES3Serializable] public BindableProperty<int> CurrentLevelCount { get; } = new BindableProperty<int>(0);

		private Dictionary<int, ILevelEntity> levelEntities = new Dictionary<int, ILevelEntity>();
		public LevelBuilder<T> GetLevelBuilder<T>(bool addToModelOnceBuilt = true) where T : class, ILevelEntity, new() {
			LevelBuilder<T> builder = entityBuilderFactory.GetBuilder<LevelBuilder<T>, T>(1);

			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}

		protected override void OnInit() {
			base.OnInit();
			foreach (ILevelEntity levelEntity in entities.Values) {
				levelEntities.Add(levelEntity.GetCurrentLevelCount(), levelEntity);
			}
		}

		public BindableProperty<ILevelEntity> CurrentLevel { get; set; } = new BindableProperty<ILevelEntity>();
		public void AddLevel(ILevelEntity level) {
			levelEntities.Add(level.GetCurrentLevelCount(), level);
		}

		public bool IsLevelSpawned(int levelNumber) {
			return levelEntities.ContainsKey(levelNumber);
		}

		public void SwitchToLevel(int levelNumber) {
			if (!IsLevelSpawned(levelNumber)) {
				this.SendEvent<OnTryToSwitchUnSpawnedLevel>(new OnTryToSwitchUnSpawnedLevel() {
					LevelNumber = levelNumber
				});
			}

			if (!IsLevelSpawned(levelNumber)) {
				return;
			}
			
			CurrentLevel.Value = levelEntities[levelNumber];
		}

		public ILevelEntity GetLevel(int levelNumber) {
			if (!IsLevelSpawned(levelNumber)) {
				Debug.LogError($"Level {levelNumber} is not spawned yet!");
				return null;
			}

			return levelEntities[levelNumber];
		}
	}
}