using _02._Scripts.Runtime.Levels.Models;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Inventory.Model;
using Runtime.Player;

namespace _02._Scripts.Runtime.Levels.Commands {

	/*public struct OnSwitchLevel {
		//public int levelCount;
	}*/
	public class NextLevelCommand : AbstractCommand<NextLevelCommand> {

		protected override void OnExecute() {
			ILevelModel levelModel = this.GetModel<ILevelModel>();

			if (levelModel.CurrentLevel.Value is TutorialLevelEntity) {
				levelModel.BaseTutorialStatus.Reset();
				
				IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
				playerEntity.AlwaysNonLethal = false;
				
				IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
				inventoryModel.Clear();
				
				playerEntity.SetHealth(playerEntity.GetMaxHealth());
				playerEntity.SetArmor(playerEntity.GetMaxArmor().RealValue);

				playerEntity.AlwaysNonLethal = false;
				levelModel.RandomBossEncounterEventChance = 0;
			
				this.SendEvent<OnReturnToBase>();
				
			}
			
			
			if (levelModel.CurrentLevelCount.Value >= LevelModel.MAX_LEVEL) {
				this.SendCommand<BackToBaseCommand>();
			}
			else {
				levelModel.SwitchToLevel(levelModel.CurrentLevelCount.Value + 1);
				((MainGame) MainGame.Interface).SaveGame();
			}


		}

		public NextLevelCommand() {
			
		}
		
		public static NextLevelCommand Allocate() {
			NextLevelCommand command = SafeObjectPool<NextLevelCommand>.Singleton.Allocate();
			return command;
		}
	}

	public struct OnReturnToBase {
		
	}
	public class BackToBaseCommand : AbstractCommand<BackToBaseCommand> {
		
		protected override void OnExecute() {
			ILevelModel levelModel = this.GetModel<ILevelModel>();
			levelModel.SwitchToLevel(0);
			IGamePlayerModel playerModel = this.GetModel<IGamePlayerModel>();
			IPlayerEntity playerEntity = playerModel.GetPlayer();
			
			playerEntity.SetHealth(playerEntity.GetMaxHealth());
			playerEntity.SetArmor(playerEntity.GetMaxArmor().RealValue);

			playerEntity.AlwaysNonLethal = false;
			levelModel.RandomBossEncounterEventChance = 0;
			
			this.SendEvent<OnReturnToBase>();
			
			((MainGame) MainGame.Interface).SaveGame();
		}
		
		
		public BackToBaseCommand() {
			
		}
		
		public static BackToBaseCommand Allocate() {
			BackToBaseCommand command = SafeObjectPool<BackToBaseCommand>.Singleton.Allocate();
			return command;
		}
	}
}