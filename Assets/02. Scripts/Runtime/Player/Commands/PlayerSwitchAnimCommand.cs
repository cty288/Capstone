using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.ViewControllers;
using Runtime.Utilities.AnimatorSystem;

namespace _02._Scripts.Runtime.Player.Commands {
	public class PlayerSwitchAnimCommand : AbstractCommand<PlayerSwitchAnimCommand> {
		private List<AnimLayerInfo> layerInfos = new List<AnimLayerInfo>();
		protected override void OnExecute() {
			
			
			this.SendCommand<PlayerAnimationCommand>(PlayerAnimationCommand.Allocate("Neutral",
				AnimationEventType.CrossFade, 0.2f));
			this.SendEvent<PlayerSwitchAnimEvent>(new PlayerSwitchAnimEvent() {
				layerInfos = layerInfos
			});
		}
		
		
		public PlayerSwitchAnimCommand() {
			
		}
		
		public static PlayerSwitchAnimCommand Allocate(List<AnimLayerInfo> layerInfos) {
			PlayerSwitchAnimCommand command = SafeObjectPool<PlayerSwitchAnimCommand>.Singleton.Allocate();
			command.layerInfos = layerInfos;
			return command;
		}
	}
}