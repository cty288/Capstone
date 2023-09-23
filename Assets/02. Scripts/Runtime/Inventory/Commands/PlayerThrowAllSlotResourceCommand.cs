using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;

namespace Runtime.Inventory.Commands {

	public struct OnPlayerThrowResource {
		public List<IResourceEntity> resources;
	}
	public class PlayerThrowAllSlotResourceCommand: AbstractCommand<PlayerThrowAllSlotResourceCommand> {
		
		protected ResourceSlot fromSlot;

		public PlayerThrowAllSlotResourceCommand() {
			
		}
		protected override void OnExecute() {
			List<string> uuids = fromSlot.GetUUIDList().Select(item => item).ToList();
			fromSlot.Clear();
			List<IResourceEntity> entities = new List<IResourceEntity>();
			foreach (string id in uuids) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(id);
				if (entity != null) {
					entities.Add(entity);
				}
			}
			
			this.SendEvent<OnPlayerThrowResource>(new OnPlayerThrowResource() {
				resources = entities
			});
		}
		
		public static PlayerThrowAllSlotResourceCommand Allocate(ResourceSlot fromSlot) {
			PlayerThrowAllSlotResourceCommand command = SafeObjectPool<PlayerThrowAllSlotResourceCommand>.Singleton.Allocate();
			command.fromSlot = fromSlot;
			return command;
		}
	}
}