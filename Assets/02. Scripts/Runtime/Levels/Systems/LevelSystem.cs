using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
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
		
		public bool IsInBossFight();
	}

	public struct OnCurrentLevelExitContitionSatisfied {
		public LevelExitCondition Condition;
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
			levelSystemExitEventHandler.RegisterOnCurrentLevelConditionSatisfied(OnCurrentLevelConditionSatisfied);
			//CoroutineRunner.Singleton
		}

		private void OnCurrentLevelConditionSatisfied(LevelExitCondition condition) {
			this.SendEvent<OnCurrentLevelExitContitionSatisfied>(new OnCurrentLevelExitContitionSatisfied() {
				Condition = condition
			});
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
			bossEntity.RegisterReadyToRecycle(OnBossRecycled);
		}

		public void OnOneSecondPassed() {
			levelSystemExitEventHandler.OnOneSecondPassed();
		}

		public BindableProperty<bool> IsLevelExitSatisfied { get; } = new BindableProperty<bool>(false);
		public bool IsInBossFight() {
			return levelModel.CurrentLevel.Value.IsInBossFight.Value;
		}

		private void OnBossRecycled(IEntity e) {
			levelModel.CurrentLevel.Value.IsInBossFight.Value = false;
			e.UnRegisterReadyToRecycle(OnBossRecycled);
		}
	}
}