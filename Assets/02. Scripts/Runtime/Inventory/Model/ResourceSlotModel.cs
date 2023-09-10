using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	public interface IResourceSlotsModel : IModel {
		/// <summary>
		/// Add item to the slots. If the item is stackable, it will be added to the existing stack. <br/>
		/// If there's no existing stack, a new stack will be created. <br/>
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		bool AddItem(IResourceEntity item);
		
		/// <summary>
		/// Add an item to the slot at the specified index. If the item is stackable, it will be added to the existing stack.
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
		/// Check if the item can be placed at the specified index. <br/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index">The nearest index that can be placed.</param>
		/// <returns></returns>
		bool CanPlaceItem(IResourceEntity item, out int index);
		
		/// <summary>
		/// Find and remove the first item with the specified uuid.
		/// </summary>
		/// <param name="uuid"></param>
		/// <returns></returns>
		bool RemoveItem(string uuid, out int index);

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
		/// Get the number of all slots currently.
		/// </summary>
		/// <returns></returns>
		int GetSlotCount();

		/// <summary>
		/// Returns the number of items in the slot at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		int GetSlotCurrentItemCount(int index);
		
		List<string> GetUUIDsByIndex(int index);
		
		/// <summary>
		/// Reset all slots to the initial state.
		/// </summary>
		void Reset();
		
		public List<ResourceSlot> GetAllSlots();

	}
	public abstract class ResourceSlotsModel : AbstractSavableModel, IResourceSlotsModel {
		[ES3Serializable] protected List<ResourceSlot> slots; //= new List<InventorySlot>();
		
		public bool AddItemAt(IResourceEntity item, int index) {
			if (!CanPlaceItem(item, index)) return false;
			ResourceSlot slot = slots[index];
			if (slot.TryAddItem(item)) {
				return true;
			}
			return false;
		}

		public bool CanPlaceItem(IResourceEntity item, out int index) {
			for (int i = 0; i < GetSlotCount(); i++) {
				if (CanPlaceItem(item, i)) {
					index = i;
					return true;
				}
			}
			index = -1;
			return false;
		}
		
		public bool AddItem(IResourceEntity item) {
			for (int i = 0; i < GetSlotCount(); i++) {
				if (CanPlaceItem(item, i)) {
					return AddItemAt(item, i);
				}
			}
			return false;
		}
		public int GetSlotCurrentItemCount(int index) {
			return GetUUIDsByIndex(index).Count;
		}
		public bool RemoveItem(string uuid, out int index) {
			
			for (int i = 0; i < GetSlotCount(); i++) {
				if (slots[i].ContainsItem(uuid)) {
					index = i;
					return RemoveItemAt(i, uuid);
				}
			}
			
			index = -1;
			return false;
		}
		
		public bool RemoveItem(string uuid) {
			if (RemoveItem(uuid, out int index)) {
				return true;
			}
			return false;
		}
		
		public bool CanPlaceItem(IResourceEntity item, int index) {
			if (index < 0 || index >= GetSlotCount()) return false;
			return slots[index].CanPlaceItem(item);
		}

		public bool RemoveItemAt(int index, string uuid) {
			if (index < 0 || index >= GetSlotCount()) return false;
			ResourceSlot slot = slots[index];
			
			if (slot.RemoveItem(uuid)) {
				return true;
			}
			return false;
		}

		public bool RemoveLastItemAt(int index) {
			if (index < 0 || index >= GetSlotCount()) return false;
			ResourceSlot slot = slots[index];
			
			if (slot.RemoveLastItem()) {
				return true;
			}
			return false;
		}


		
		public int GetSlotCount() {
			return slots.Count;
		}

		public List<string> GetUUIDsByIndex(int index) {
			if (index < 0 || index >= GetSlotCount()) return null;
			return slots[index].GetUUIDList();
		}

		public abstract void Reset();
		public List<ResourceSlot> GetAllSlots() {
			return slots;
		}
	}
}