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
			
			levelModel.SwitchToLevel(levelModel.CurrentLevelCount.Value + 1);
		}
		
		
		public NextLevelCommand() {
			
		}
		
		public static NextLevelCommand Allocate() {
			NextLevelCommand command = SafeObjectPool<NextLevelCommand>.Singleton.Allocate();
			return command;
		}
	}
}