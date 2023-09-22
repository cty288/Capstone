using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	/*public struct OnInventorySlotUpdateEvent {
		public InventorySlotInfo UpdatedSlot;
	}*/

	public struct SlotInfo {
		public int SlotIndex;
		public List<IResourceEntity> Items;
		public IResourceEntity TopItem {
			get {
				if (Items.Count == 0) {
					return null;
				}
				return Items[^1];
			}
		}
	}

	public interface IInventorySystem : IResourceSlotsSystem {
		
	}

	public interface IResourceSlotsSystem : ISystem {
		public void ResetSlots();

		public void ClearSlots();
	}
	
	public abstract class AbstractResourceSlotsSystem<T> : AbstractSystem, IResourceSlotsSystem where T : class, IResourceSlotsModel {
		protected T model;

		protected override void OnInit() {
			model = this.GetModel<T>();
			List<ResourceSlot> slots = model.GetAllSlots();
			foreach (ResourceSlot resourceSlot in slots) {
				List<string> uuids = resourceSlot.GetUUIDList();
				for (int i = 0; i < uuids.Count; i++) {
					IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuids[i]);
					if (entity != null) {
						model.RegisterInitialEntityEvents(entity);
					}
				}
			}
		}

		public abstract void ResetSlots();
		public abstract void ClearSlots();
	}
}