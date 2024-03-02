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

		public bool StartWithTutorial { get; set; }

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

		public static int MAX_LEVEL = 3;

		protected override void OnInit() {
			base.OnInit();
			foreach (ILevelEntity levelEntity in entities.Values) {
				levelEntities.Add(levelEntity.GetCurrentLevelCount(), levelEntity);
			}
		}

		public BindableProperty<ILevelEntity> CurrentLevel { get; set; } = new BindableProperty<ILevelEntity>();
		public void AddLevel(ILevelEntity level) {
			levelEntities.TryAdd(level.GetCurrentLevelCount(), level);
		}

		public bool IsLevelSpawned(int levelNumber) {
			return levelEntities.ContainsKey(levelNumber);
		}

		public void SwitchToLevel(int levelNumber) {
			//TODO: if levelnum > 0 && tutorial lev = true, then back to base.
			HashSet<ILevelEntity> removedLevels = new HashSet<ILevelEntity>();
			if (levelNumber > 0 && StartWithTutorial) {
				StartWithTutorial = false;
				foreach (var level in levelEntities) {
					if (level.Key > 0) {
						removedLevels.Add(level.Value);
					}
				}

				levelEntities.Clear();
				levelNumber = 0;
				CurrentLevelCount.Value = 0;
			}
			

			if (!IsLevelSpawned(levelNumber)) {
				this.SendEvent<OnTryToSwitchUnSpawnedLevel>(new OnTryToSwitchUnSpawnedLevel() {
					LevelNumber = levelNumber
				});
			}

			if (!IsLevelSpawned(levelNumber)) {
				levelNumber = 0;
			}
			
			
			CurrentLevel.Value = levelEntities[levelNumber];
			CurrentLevelCount.SetValueAndForceNotify(levelNumber);
			
			foreach (var level in removedLevels) {
				RemoveEntity(level.UUID);
			}
			
			if (levelNumber == 0) {
				HashSet<int> toRemove = new HashSet<int>();
				//remove all other levels
				foreach (var level in levelEntities) {
					if (level.Key == 0) {
						continue;
					}
					RemoveEntity(level.Value.UUID);
					toRemove.Add(level.Key);
				}
				
				foreach (var key in toRemove) {
					levelEntities.Remove(key);
				}
			}
		}
		
		

		public ILevelEntity GetLevel(int levelNumber) {
			if (!IsLevelSpawned(levelNumber)) {
				Debug.LogError($"Level {levelNumber} is not spawned yet!");
				return null;
			}

			return levelEntities[levelNumber];
		}

		[field: ES3Serializable]
		public bool StartWithTutorial { get; set; } = false;
	}
}