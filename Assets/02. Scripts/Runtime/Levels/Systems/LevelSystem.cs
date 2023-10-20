using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Levels.Systems {
	public interface ILevelSystem : ISystem {
		public void SetBossFight(IEnemyEntity bossEntity);
	}
	public class LevelSystem : AbstractSystem, ILevelSystem {
		private ILevelModel levelModel;
		private LevelSystemExitEventHandler levelSystemExitEventHandler;
		
		protected override void OnInit() {
			levelModel = this.GetModel<ILevelModel>();
			levelModel.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged);
			levelSystemExitEventHandler = new LevelSystemExitEventHandler();
			levelSystemExitEventHandler.Init();
			//CoroutineRunner.Singleton.st
		}

		private void OnCurrentLevelChanged(ILevelEntity oldLevel, ILevelEntity newLevel) {
			//levelSystemExitEventHandler.SetLevelEntity(newLevel);
		}

		public void SetBossFight(IEnemyEntity bossEntity) {
			if (levelModel.CurrentLevel.Value.IsInBossFight) {
				return;
			}
			
			levelModel.CurrentLevel.Value.IsInBossFight.Value = true;
			bossEntity.RegisterOnEntityRecycled(OnBossRecycled);
		}

		private void OnBossRecycled(IEntity e) {
			levelModel.CurrentLevel.Value.IsInBossFight.Value = false;
		}
	}
}