using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	public struct OnInventoryReloadEvent {
		public List<InventorySlotInfo> InventorySlots;
	}

	
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
				List<IResourceEntity> resources = GetResourcesByIDs(uuids);

				this.SendEvent<OnInventorySlotUpdateEvent>(new OnInventorySlotUpdateEvent() {
					UpdatedSlot = new InventorySlotInfo() {
						Items = resources,
						SlotIndex = index
					}
				});
				return true;
			}
			return false;
		}
		
		private List<IResourceEntity> GetResourcesByIDs(List<string> uuids) {
			List<IResourceEntity> resources = new List<IResourceEntity>();
			foreach (string uuid in uuids) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
				resources.Add(entity);
			}
			return resources;
		}

		public bool CanPlaceItem(IResourceEntity item, int index) {
			return inventoryModel.CanPlaceItem(item, index);
		}

		public bool RemoveItem(string uuid) {
			if (inventoryModel.RemoveItem(uuid, out int index)) {
				List<string> uuids = inventoryModel.GetUUIDsByIndex(index);
				List<IResourceEntity> resources = GetResourcesByIDs(uuids);

				this.SendEvent<OnInventorySlotUpdateEvent>(new OnInventorySlotUpdateEvent() {
					UpdatedSlot = new InventorySlotInfo() {
						Items = resources,
						SlotIndex = index
					}
				});
				return true;
			}
			return false;
		}

		public bool RemoveItemAt(int index, string uuid) {
			if (inventoryModel.RemoveItemAt(index, uuid)) {
				List<string> uuids = inventoryModel.GetUUIDsByIndex(index);
				List<IResourceEntity> resources = GetResourcesByIDs(uuids);

				this.SendEvent<OnInventorySlotUpdateEvent>(new OnInventorySlotUpdateEvent() {
					UpdatedSlot = new InventorySlotInfo() {
						Items = resources,
						SlotIndex = index
					}
				});
				return true;
			}
			return false;
		}

		public bool RemoveLastItemAt(int index) {
			if (inventoryModel.RemoveLastItemAt(index)) {
				List<string> uuids = inventoryModel.GetUUIDsByIndex(index);
				List<IResourceEntity> resources = GetResourcesByIDs(uuids);

				this.SendEvent<OnInventorySlotUpdateEvent>(new OnInventorySlotUpdateEvent() {
					UpdatedSlot = new InventorySlotInfo() {
						Items = resources,
						SlotIndex = index
					}
				});
				return true;
			}
			return false;
		}

		public bool AddSlots(int slotCount) {
			if (inventoryModel.AddSlots(slotCount)) {
				return true;
			}
			return false;
		}

		public int GetSlotCount() {
			return inventoryModel.GetSlotCount();
		}

		public void ResetInventory() {
			inventoryModel.ResetInventory();
			List<InventorySlotInfo> slots = new List<InventorySlotInfo>();
			for (int i = 0; i < GetSlotCount(); i++) {
				List<string> uuids = inventoryModel.GetUUIDsByIndex(i);
				List<IResourceEntity> resources = GetResourcesByIDs(uuids);
				slots.Add(new InventorySlotInfo() {
					Items = resources,
					SlotIndex = i
				});
			}

			this.SendEvent<OnInventoryReloadEvent>(new OnInventoryReloadEvent() {
				InventorySlots = slots
			});
		}

		public int GetSlotCurrentItemCount(int index) {
			return inventoryModel.GetUUIDsByIndex(index).Count;
		}

		public InventorySlotInfo GetItemsAt(int index) {
			List<string> uuids = inventoryModel.GetUUIDsByIndex(index);
			List<IResourceEntity> resources = GetResourcesByIDs(uuids);
			return new InventorySlotInfo() {
				Items = resources,
				SlotIndex = index
			};
		}
	}
}