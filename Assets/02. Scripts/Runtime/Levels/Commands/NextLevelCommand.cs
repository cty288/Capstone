using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using MikroFramework.Pool;

namespace _02._Scripts.Runtime.Levels.Commands {

	/*public struct OnSwitchLevel {
		//public int levelCount;
	}*/
	public class NextLevelCommand : AbstractCommand<NextLevelCommand> {

		protected override void OnExecute() {
			ILevelModel levelModel = this.GetModel<ILevelModel>();

			if (levelModel.CurrentLevelCount.Value >= LevelModel.MAX_LEVEL) {
				this.SendCommand<BackToBaseCommand>();
			}
			else {
				levelModel.SwitchToLevel(levelModel.CurrentLevelCount.Value + 1);
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
			this.SendEvent<OnReturnToBase>();
		}
		
		
		public BackToBaseCommand() {
			
		}
		
		public static BackToBaseCommand Allocate() {
			BackToBaseCommand command = SafeObjectPool<BackToBaseCommand>.Singleton.Allocate();
			return command;
		}
	}
}