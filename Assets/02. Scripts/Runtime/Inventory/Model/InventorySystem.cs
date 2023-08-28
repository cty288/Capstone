using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	

	
	public class InventorySystem : AbstractSystem, IInventorySystem {
		private IInventoryModel inventoryModel;
		protected override void OnInit() {
			inventoryModel = this.GetModel<IInventoryModel>();
		}

		public bool AddItem(IResourceEntity item) {
			for (int i = 0; i < GetSlotCount(); i++) {
				if (CanPlaceItem(item, i)) {
					return AddItemAt(item, i);
				}
			}
			return false;
		}

		public bool AddItemAt(IResourceEntity item, int index) {
			if(inventoryModel.AddItemAt(item, index)) {
				List<string> uuids = inventoryModel.GetUUIDsByIndex(index);
				/*this.get
				this.SendEvent<OnInventorySlotUpdateEvent>(new OnInventorySlotUpdateEvent() {
					UpdatedSlot = new InventorySlotInfo() {
						SlotIndex = index,
						Quantity = 
					}
				});*/
				return true;
			}
			return false;
		}

		public bool CanPlaceItem(IResourceEntity item, int index) {
			return inventoryModel.CanPlaceItem(item, index);
		}

		public bool RemoveItem(string uuid) {
			throw new System.NotImplementedException();
		}

		public bool RemoveItemAt(int index, string uuid) {
			throw new System.NotImplementedException();
		}

		public bool RemoveLastItemAt(int index) {
			throw new System.NotImplementedException();
		}

		public bool AddSlots(int slotCount) {
			throw new System.NotImplementedException();
		}

		public int GetSlotCount() {
			throw new System.NotImplementedException();
		}
	}
}