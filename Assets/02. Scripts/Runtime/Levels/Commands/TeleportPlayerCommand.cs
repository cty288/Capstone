 using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Inventory.Commands;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Commands {
	public struct OnPlayerTeleport {
		public Transform targetTransform;
	}
	public class TeleportPlayerCommand : AbstractCommand<TeleportPlayerCommand> {
		private Transform targetTransform;
		protected override void OnExecute() {
			this.SendEvent<OnPlayerTeleport>(new OnPlayerTeleport() {
				targetTransform = this.targetTransform
			});
		}
		
		
		public TeleportPlayerCommand() {
			
		}
		
		public static TeleportPlayerCommand Allocate(Transform targetTransform) {
			TeleportPlayerCommand command = SafeObjectPool<TeleportPlayerCommand>.Singleton.Allocate();
			command.targetTransform = targetTransform;
			return command;
		}
	}
}