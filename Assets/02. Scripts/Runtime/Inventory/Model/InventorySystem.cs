using System;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	/*public struct OnInventoryReloadEvent {
		public List<InventorySlotInfo> InventorySlots;
	}*/

	
	public class InventorySystem : AbstractSystem, IInventorySystem {
		private IInventoryModel inventoryModel;
		protected override void OnInit() {
			inventoryModel = this.GetModel<IInventoryModel>();
			//inventoryModel.InitWithInitialSlots();
		}

		private List<IResourceEntity> GetResourcesByIDs(List<string> uuids) {
			List<IResourceEntity> resources = new List<IResourceEntity>();
			foreach (string uuid in uuids) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
				resources.Add(entity);
			}
			return resources;
		}
		

		public SlotInfo GetItemsAt(int index) {
			List<string> uuids = inventoryModel.GetUUIDsByIndex(index);
			List<IResourceEntity> resources = GetResourcesByIDs(uuids);
			return new SlotInfo() {
				Items = resources,
				SlotIndex = index
			};
		}


	}
}