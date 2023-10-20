using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Levels.Systems {
	public interface ILevelSystem : ISystem {
		public void SetBossFight(IEnemyEntity bossEntity);
		
		public void OnOneSecondPassed();
		
		public BindableProperty<bool> IsLevelExitSatisfied { get; }
	}
	public class LevelSystem : AbstractSystem, ILevelSystem {
		private ILevelModel levelModel;
		private LevelSystemExitEventHandler levelSystemExitEventHandler;
		
		protected override void OnInit() {
			levelModel = this.GetModel<ILevelModel>();
			
			levelSystemExitEventHandler = new LevelSystemExitEventHandler();
			levelSystemExitEventHandler.Init();
			levelModel.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged);
			levelSystemExitEventHandler.RegisterOnCurrentLevelExitSatisfied(OnCurrentLevelExitSatisfied);
			//CoroutineRunner.Singleton
		}

		private void OnCurrentLevelExitSatisfied() {
			IsLevelExitSatisfied.Value = true;
		}

		private void OnCurrentLevelChanged(ILevelEntity oldLevel, ILevelEntity newLevel) {
			IsLevelExitSatisfied.Value = false;
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

		public BindableProperty<bool> IsLevelExitSatisfied { get; } = new BindableProperty<bool>(false);

		private void OnBossRecycled(IEntity e) {
			levelModel.CurrentLevel.Value.IsInBossFight.Value = false;
		}
	}
}