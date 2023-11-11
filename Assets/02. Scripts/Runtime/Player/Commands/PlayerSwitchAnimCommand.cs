using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Utilities.AnimatorSystem;

namespace _02._Scripts.Runtime.Player.Commands {
	public class PlayerSwitchAnimCommand : AbstractCommand<PlayerSwitchAnimCommand> {
		private string layerName;
		private float weight;
		protected override void OnExecute() {
			
			
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Neutral",
				AnimationEventType.CrossFade, 0.2f));
			this.SendEvent<PlayerSwitchAnimEvent>(new PlayerSwitchAnimEvent() {
				layerName = layerName,
				weight = weight
			});
		}
		
		
		public PlayerSwitchAnimCommand() {
			
		}
		
		public static PlayerSwitchAnimCommand Allocate(string layerName, float weight) {
			PlayerSwitchAnimCommand command = SafeObjectPool<PlayerSwitchAnimCommand>.Singleton.Allocate();
			command.layerName = layerName;
			command.weight = weight;
			return command;
		}
	}
}