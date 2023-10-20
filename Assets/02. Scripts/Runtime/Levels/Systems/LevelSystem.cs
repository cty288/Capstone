using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Levels.Systems {
	public interface ILevelSystem : ISystem {
		public void SetBossFight(IEnemyEntity bossEntity);
		
		public void OnOneSecondPassed();
	}
	public class LevelSystem : AbstractSystem, ILevelSystem {
		private ILevelModel levelModel;
		private LevelSystemExitEventHandler levelSystemExitEventHandler;
		
		protected override void OnInit() {
			levelModel = this.GetModel<ILevelModel>();
			
			levelSystemExitEventHandler = new LevelSystemExitEventHandler();
			levelSystemExitEventHandler.Init();
			levelModel.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged);
			//CoroutineRunner.Singleton
		}

		private void OnCurrentLevelChanged(ILevelEntity oldLevel, ILevelEntity newLevel) {
			levelSystemExitEventHandler.SetLevelEntity(newLevel);
		}

		public void SetBossFight(IEnemyEntity bossEntity) {
			if (levelModel.CurrentLevel.Value.IsInBossFight) {
				return;
			}
			
			levelModel.CurrentLevel.Value.IsInBossFight.Value = true;
			bossEntity.RegisterOnEntityRecycled(OnBossRecycled);
		}

		public void OnOneSecondPassed() {
			levelSystemExitEventHandler.OnOneSecondPassed();
		}

		private void OnBossRecycled(IEntity e) {
			levelModel.CurrentLevel.Value.IsInBossFight.Value = false;
		}
	}
}