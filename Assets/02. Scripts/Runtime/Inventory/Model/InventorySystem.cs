using System;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	/*public struct OnInventoryReloadEvent {
		public List<InventorySlotInfo> InventorySlots;
	}*/

	public struct OnCurrentHotbarUpdateEvent {
		public HotBarCategory Category;
		public IResourceEntity TopItem;
	}
	
	public class InventorySystem : AbstractSystem, IInventorySystem {
		private IInventoryModel inventoryModel;
		

		public static int InitialSlotCount = 9;
		public static Dictionary<HotBarCategory, int> InitialHotBarSlotCount = new Dictionary<HotBarCategory, int>() {
			{HotBarCategory.Right, 2},
			{HotBarCategory.Left, 3}
		};
		
		private Dictionary<HotBarCategory, ResourceSlot> currentSelectedSlot = new Dictionary<HotBarCategory, ResourceSlot>();
		private Dictionary<ResourceSlot, HotBarCategory> slotToCategories = new Dictionary<ResourceSlot, HotBarCategory>();

		protected override void OnInit() {
			inventoryModel = this.GetModel<IInventoryModel>();

			this.RegisterEvent<OnHotBarSlotSelectedEvent>(OnHotBarSlotSelected);
			if (inventoryModel.IsFirstTimeCreated) {
				ResetInventory();
			}
			else {
				inventoryModel.SelectHotBarSlot(HotBarCategory.Left, inventoryModel.GetSelectedHotBarSlotIndex(HotBarCategory.Left));
				inventoryModel.SelectHotBarSlot(HotBarCategory.Right, inventoryModel.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
			}
		}

		private void OnHotBarSlotSelected(OnHotBarSlotSelectedEvent e) {
			ResourceSlot slot = inventoryModel.GetHotBarSlots(e.Category)[e.SelectedIndex];
			currentSelectedSlot.TryAdd(e.Category, null);
			
			ResourceSlot previousSlot = currentSelectedSlot[e.Category];
			if (previousSlot != null) {
				previousSlot.UnregisterOnSlotUpdateCallback(OnCurrentSlotUpdate);
			}

			currentSelectedSlot[e.Category] = slot;
			slotToCategories.TryAdd(slot, e.Category);
			
			slot.RegisterOnSlotUpdateCallback(OnCurrentSlotUpdate);
			OnCurrentSlotUpdate(slot, slot.GetLastItemUUID(), slot.GetUUIDList());
		}

		private void OnCurrentSlotUpdate(ResourceSlot slot, string topUUID, List<string> allUUIDs) {
			if (!slotToCategories.ContainsKey(slot)) {
				return;
			}
			HotBarCategory category = slotToCategories[slot];

			IResourceEntity resourceEntity = null;
			if (topUUID != null) {
				resourceEntity = GlobalGameResourceEntities.GetAnyResource(topUUID);
			}
			
			this.SendEvent<OnCurrentHotbarUpdateEvent>(new OnCurrentHotbarUpdateEvent() {
				Category = category,
				TopItem = resourceEntity
			});
		}


		private List<IResourceEntity> GetResourcesByIDs(List<string> uuids) {
			List<IResourceEntity> resources = new List<IResourceEntity>();
			foreach (string uuid in uuids) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
				resources.Add(entity);
			}
			return resources;
		}


		public void ResetInventory() {
			inventoryModel.Clear();
			AddInitialSlots();
			inventoryModel.SelectHotBarSlot(HotBarCategory.Left, 0);
			inventoryModel.SelectHotBarSlot(HotBarCategory.Right, 0);
		}
		
		
		private void AddInitialSlots() {
			inventoryModel.AddSlots(InitialSlotCount);

			inventoryModel.AddHotBarSlots(HotBarCategory.Left, InitialHotBarSlotCount[HotBarCategory.Left],
				()=>new LeftHotBarSlot());
				
			inventoryModel.AddHotBarSlots(HotBarCategory.Right, InitialHotBarSlotCount[HotBarCategory.Right],
				()=>new RightHotBarSlot());
		}
	}
}