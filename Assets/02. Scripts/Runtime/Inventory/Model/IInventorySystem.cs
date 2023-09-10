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

	public interface IInventorySystem : ISystem {
		
		/// <summary>
		/// Get all items in the inventory at the specified index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public SlotInfo GetItemsAt(int index);
	}
}