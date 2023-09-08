using System;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	public interface IInventoryModel : IModel {

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
		/// <summary>
		/// Get the number of all slots currently.
		/// </summary>
		/// <returns></returns>
		int GetSlotCount();
		
		List<string> GetUUIDsByIndex(int index);
		
		/// <summary>
		/// Reset the inventory to the initial state.
		/// </summary>
		void ResetInventory();

		void InitWithInitialSlots();

	}
	
	public struct OnInventorySlotAddedEvent {
		public int AddedCount;
	}


	
	
	
	public class InventoryModel: AbstractSavableModel, IInventoryModel {

		[ES3Serializable] private List<InventorySlot> slots;
		
		
		public static int MaxSlotCount = 40;
		
		public static int InitialSlotCount = 10;
		protected override void OnInit() {
			base.OnInit();
			
		}
		

		public bool AddItemAt(IResourceEntity item, int index) {
			if (!CanPlaceItem(item, index)) return false;
			InventorySlot slot = slots[index];
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
		
		public bool CanPlaceItem(IResourceEntity item, int index) {
			if (index < 0 || index >= GetSlotCount()) return false;
			return slots[index].CanPlaceItem(item);
		}

		public bool RemoveItemAt(int index, string uuid) {
			if (index < 0 || index >= GetSlotCount()) return false;
			InventorySlot slot = slots[index];
			
			if (slot.RemoveItem(uuid)) {
				return true;
			}
			return false;
		}

		public bool RemoveLastItemAt(int index) {
			if (index < 0 || index >= GetSlotCount()) return false;
			InventorySlot slot = slots[index];
			
			if (slot.RemoveLastItem()) {
				return true;
			}
			return false;
		}

		public bool AddSlots(int slotCount) {
			int actualAddedCount = slotCount;
			if (slotCount + GetSlotCount() > MaxSlotCount) {
				actualAddedCount = MaxSlotCount - GetSlotCount();
			}
			
			for (int i = 0; i < actualAddedCount; i++) {
				slots.Add(new InventorySlot());
			}
			
			this.SendEvent<OnInventorySlotAddedEvent>(new OnInventorySlotAddedEvent() {
				AddedCount = actualAddedCount
			});
			return actualAddedCount == slotCount;
		}

		public int GetSlotCount() {
			return slots.Count;
		}

		public List<string> GetUUIDsByIndex(int index) {
			if (index < 0 || index >= GetSlotCount()) return null;
			return slots[index].GetUUIDList();
		}

		public void ResetInventory() {
			slots.Clear();
			AddSlots(InitialSlotCount);
		}

		public void InitWithInitialSlots() {
			if (slots == null) { //not load from saved
				slots = new List<InventorySlot>();
				AddSlots(InitialSlotCount);
			}
		}
	}
}