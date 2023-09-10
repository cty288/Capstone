using System;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	public interface IInventoryModel : IModel, IResourceSlotsModel {
		
		
		/// <summary>
		/// Add the specified number of slots to the inventory. <br/>
		/// Factors causing false: <br/>
		/// 1. The resulting number of slots exceeds the maximum number of slots. In this case, only the slots that can be added will be added.
		/// </summary>
		/// <param name="slotCount"></param>
		/// <returns></returns>
		bool AddSlots(int slotCount);
		
		/// <summary>
		/// Add the specified number of slots to the hotbar. <br/>
		/// </summary>
		/// <param name="slotCount"></param>
		/// <returns></returns>
		bool AddHotBarSlots(int slotCount);
		

		//void InitWithInitialSlots();
		
		int GetHotBarSlotCount();

	}
	
	public struct OnInventorySlotAddedEvent {
		public int StartIndex;
		public int AddedCount;
		public List<ResourceSlot> AddedSlots;
	}

	
	
	
	
	public class InventoryModel: ResourceSlotsModel, IInventoryModel {
		
		[ES3Serializable] 
		private int hotBarSlotCount = 0; //refer to those in the hotbar, always the first x of the slots
		
		public static int MaxSlotCount = 27;
		public static int MaxHotBarSlotCount = 9;
		
		public static int InitialSlotCount = 9;
		public static int InitialHotBarSlotCount = 3;
		
		
		public bool AddHotBarSlots(int slotCount) {
			int actualAddedCount = slotCount;
			if (slotCount + hotBarSlotCount > MaxHotBarSlotCount) {
				actualAddedCount = MaxHotBarSlotCount - hotBarSlotCount;
			}
			int startIndex = hotBarSlotCount;
			//insert to the end of slots[slotCount-1]
			List<ResourceSlot> addedSlots = new List<ResourceSlot>();
			for (int i = 0; i < actualAddedCount; i++) {
				ResourceSlot slot = new ResourceSlot();
				slots.Insert(hotBarSlotCount + i, slot);
				addedSlots.Add(slot);
			}
			hotBarSlotCount += actualAddedCount;
			
			this.SendEvent<OnInventorySlotAddedEvent>(new OnInventorySlotAddedEvent() {
				StartIndex = startIndex,
				AddedCount = actualAddedCount,
				AddedSlots = addedSlots
			});
			return actualAddedCount == slotCount;
		}


		public bool AddSlots(int slotCount) {
			int actualAddedCount = slotCount;
			if (slotCount + GetSlotCount() > MaxSlotCount + hotBarSlotCount) {
				actualAddedCount = MaxSlotCount + hotBarSlotCount - GetSlotCount();
			}
			
			int startIndex = GetSlotCount();
			
			List<ResourceSlot> addedSlots = new List<ResourceSlot>();
			for (int i = 0; i < actualAddedCount; i++) {
				ResourceSlot slot = new ResourceSlot();
				slots.Add(slot);
				addedSlots.Add(slot);
			}
			
			this.SendEvent<OnInventorySlotAddedEvent>(new OnInventorySlotAddedEvent() {
				StartIndex = startIndex,
				AddedCount = actualAddedCount,
				AddedSlots = addedSlots
			});
			return actualAddedCount == slotCount;
		}

		/*public void InitWithInitialSlots() {
			
		}*/

		protected override void OnInit() {
			base.OnInit();
			if (slots == null) { //not load from saved
				slots = new List<ResourceSlot>();
				AddSlots(InitialSlotCount);
				AddHotBarSlots(InitialHotBarSlotCount);
			}
		}

		public int GetHotBarSlotCount() {
			return hotBarSlotCount;
		}

		public override void Reset() {
			if (slots == null) {
				return;
			}
			slots.Clear();
			AddSlots(InitialSlotCount);
			AddHotBarSlots(InitialHotBarSlotCount);
		}
	}
}