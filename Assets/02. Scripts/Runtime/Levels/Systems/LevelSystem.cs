using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;

namespace _02._Scripts.Runtime.Levels.Systems {
	public interface ILevelSystem : ISystem {
		public void SetBossFight(IEnemyEntity bossEntity);
	}
	public class LevelSystem : AbstractSystem, ILevelSystem {
		private ILevelModel levelModel;
		
		protected override void OnInit() {
			levelModel = this.GetModel<ILevelModel>();
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