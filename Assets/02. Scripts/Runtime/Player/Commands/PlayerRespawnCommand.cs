using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.ViewControllers;
using Runtime.Player;
using Runtime.Utilities.AnimatorSystem;

namespace _02._Scripts.Runtime.Player.Commands {
	public struct OnPlayerRespawn {
		
	}
	public class PlayerRespawnCommand : AbstractCommand<PlayerRespawnCommand> {
		protected override void OnExecute() {
			ILevelModel levelModel = this.GetModel<ILevelModel>();
			levelModel.SwitchToLevel(0);
			IGamePlayerModel playerModel = this.GetModel<IGamePlayerModel>();
			IPlayerEntity playerEntity = playerModel.GetPlayer();
			playerEntity.Heal(playerEntity.GetMaxHealth(), null);
			playerEntity.AddArmor(playerEntity.GetArmor().InitialValue);
			
			this.SendEvent<OnPlayerRespawn>();
		}
		
		
		public PlayerRespawnCommand() {
			
		}
		
		public static PlayerRespawnCommand Allocate(List<AnimLayerInfo> layerInfos) {
			PlayerRespawnCommand command = SafeObjectPool<PlayerRespawnCommand>.Singleton.Allocate();
			return command;
		}
	}
}