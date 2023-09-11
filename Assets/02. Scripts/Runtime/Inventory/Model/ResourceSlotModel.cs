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
		/// Returns true if the item can be placed at the specified index. <br/>
		/// Factors causing false: <br/>
		/// 1. The index is out of range. <br/>
		/// 2. The item is not currently stackable at the specified index. <br/>
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		bool CanPlaceItem(IResourceEntity item);
		
		


		/// <summary>
		/// Find and remove the first item with the specified uuid.
		/// </summary>
		/// <param name="uuid"></param>
		/// <returns></returns>
		bool RemoveItem(string uuid);
		
		

		/// <summary>
		/// Get the number of all slots currently.
		/// </summary>
		/// <returns></returns>
		int GetSlotCount();
		
		
		/// <summary>
		/// Reset all slots to the initial state.
		/// </summary>
		void Clear();
		
		public List<ResourceSlot> GetAllSlots();
		
		public int GetSlotCurrentItemCount(int index);

	}
	public abstract class ResourceSlotsModel : AbstractSavableModel, IResourceSlotsModel {
		[ES3Serializable] protected List<ResourceSlot> slots; //= new List<InventorySlot>();
		
		
		
		
		
		
		protected virtual bool AddItemAt(IResourceEntity item, ResourceSlot slot) {
			if (slot.TryAddItem(item)) {
				item.OnPicked();
				return true;
			}
			return false;
		}

		public virtual bool CanPlaceItem(IResourceEntity item) {
			for (int i = 0; i < GetSlotCount(); i++) {
				if (CanPlaceItem(item, i)) {
					return true;
				}
			}
			return false;
		}
		
		
		public virtual bool AddItem(IResourceEntity item) {
			for (int i = 0; i < GetSlotCount(); i++) {
				if (CanPlaceItem(item, i)) {
					return AddItemAt(item, slots[i]);
				}
			}
			return false;
		}
		
		public virtual bool RemoveItem(string uuid) {
			for (int i = 0; i < GetSlotCount(); i++) {
				if (slots[i].ContainsItem(uuid)) {
					return slots[i].RemoveItem(uuid);
				}
			}
			return false;
		}
		
		private bool CanPlaceItem(IResourceEntity item, int index) {
			if (index < 0 || index >= GetSlotCount()) return false;
			return slots[index].CanPlaceItem(item);
		}

		
		public int GetSlotCount() {
			return slots.Count;
		}
		

		public abstract void Clear();
		public List<ResourceSlot> GetAllSlots() {
			return slots;
		}

		public int GetSlotCurrentItemCount(int index) {
			return slots[index].GetQuantity();
		}
	}
}