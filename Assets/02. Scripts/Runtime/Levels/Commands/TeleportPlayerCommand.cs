 using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Inventory.Commands;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Commands {
	public struct OnPlayerTeleport {
		public Vector3 targetPos;
	}
	public class TeleportPlayerCommand : AbstractCommand<TeleportPlayerCommand> {
		private Vector3 targetPos;
		protected override void OnExecute() {
			this.SendEvent<OnPlayerTeleport>(new OnPlayerTeleport() {
				targetPos = this.targetPos
			});
		}
		
		
		public TeleportPlayerCommand() {
			
		}
		
		public static TeleportPlayerCommand Allocate(Vector3 targetPos) {
			TeleportPlayerCommand command = SafeObjectPool<TeleportPlayerCommand>.Singleton.Allocate();
			command.targetPos = targetPos;
			return command;
		}
	}
}