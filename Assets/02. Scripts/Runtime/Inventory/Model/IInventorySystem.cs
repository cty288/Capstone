using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	public struct OnInventorySlotUpdateEvent {
		public InventorySlotInfo UpdatedSlot;
	}

	public struct InventorySlotInfo {
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
		/// Add item to the inventory. If the item is stackable, it will be added to the existing stack. <br/>
		/// If there's no existing stack, a new stack will be created. <br/>
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		bool AddItem(IResourceEntity item);
		
		/// <summary>
		/// Add an item to the inventory at the specified index. If the item is stackable, it will be added to the existing stack.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		bool AddItemAt(IResourceEntity item, int index);
		
		/// <summary>
		/// Returns true if the item can be placed at the specified index. <br/>
		/// Factors causing false: <br/>
		/// 1. The index is out of range. <br/>
		/// 2. The item is not currently stackable at the specified index. <br/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		bool CanPlaceItem(IResourceEntity item, int index);
		
		/// <summary>
		/// Find and remove the first item with the specified uuid.
		/// </summary>
		/// <param name="uuid"></param>
		/// <returns></returns>
		bool RemoveItem(string uuid);
		
		/// <summary>
		/// Find and remove the first item with the specified uuid at the specified slot.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="uuid"></param>
		/// <returns></returns>
		bool RemoveItemAt(int index, string uuid);
		
		/// <summary>
		/// Remove the last item at the specified slot.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		bool RemoveLastItemAt(int index);
		
		/// <summary>
		/// Add the specified number of slots to the inventory. <br/>
		/// Factors causing false: <br/>
		/// 1. The resulting number of slots exceeds the maximum number of slots. In this case, only the slots that can be added will be added.
		/// </summary>
		/// <param name="slotCount"></param>
		/// <returns></returns>
		bool AddSlots(int slotCount);
		
		int GetSlotCount();
		
		/// <summary>
		/// Reset the inventory to the initial state.
		/// </summary>
		void ResetInventory();
		
		/// <summary>
		/// Get the item count at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		int GetSlotCurrentItemCount(int index);
		
		/// <summary>
		/// Find all resources with the specified index in the inventory.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		InventorySlotInfo GetItemsAt(int index);
	}
}