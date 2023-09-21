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
	
	public class InventorySystem : AbstractResourceSlotsSystem<IInventoryModel>, IInventorySystem {
		public static int InitialSlotCount = 8;
		public static Dictionary<HotBarCategory, int> InitialHotBarSlotCount = new Dictionary<HotBarCategory, int>() {
			{HotBarCategory.Right, 2},
			{HotBarCategory.Left, 3}
		};
		
		private Dictionary<HotBarCategory, ResourceSlot> currentSelectedSlot = new Dictionary<HotBarCategory, ResourceSlot>();
		private Dictionary<ResourceSlot, HotBarCategory> slotToCategories = new Dictionary<ResourceSlot, HotBarCategory>();

		protected override void OnInit() {
			base.OnInit();

			this.RegisterEvent<OnHotBarSlotSelectedEvent>(OnHotBarSlotSelected);
			if (model.IsFirstTimeCreated) {
				ResetSlots();
			}
			else {
				model.SelectHotBarSlot(HotBarCategory.Left, model.GetSelectedHotBarSlotIndex(HotBarCategory.Left));
				model.SelectHotBarSlot(HotBarCategory.Right, model.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
			}

			List<ResourceSlot> allHotBarSlots = new List<ResourceSlot>();
			allHotBarSlots.AddRange(model.GetHotBarSlots(HotBarCategory.Left));
			allHotBarSlots.AddRange(model.GetHotBarSlots(HotBarCategory.Right));
			foreach (ResourceSlot resourceSlot in allHotBarSlots) {
				List<string> uuids = resourceSlot.GetUUIDList();
				for (int i = 0; i < uuids.Count; i++) {
					IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuids[i]);
					if (entity != null) {
						model.RegisterInitialEntityEvents(entity);
					}
				}
			}
		}

		private void OnHotBarSlotSelected(OnHotBarSlotSelectedEvent e) {
			ResourceSlot slot = model.GetHotBarSlots(e.Category)[e.SelectedIndex];
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


		public override void ResetSlots() {
			model.Clear();
			AddInitialSlots();
			model.SelectHotBarSlot(HotBarCategory.Left, 0);
			model.SelectHotBarSlot(HotBarCategory.Right, 0);
		}

		public override void ClearSlots() {
			model.Clear();
		}

		private void AddInitialSlots() {
			model.AddSlots(InitialSlotCount);

			model.AddHotBarSlots(HotBarCategory.Left, InitialHotBarSlotCount[HotBarCategory.Left],
				()=>new LeftHotBarSlot());
				
			model.AddHotBarSlots(HotBarCategory.Right, InitialHotBarSlotCount[HotBarCategory.Right],
				()=>new RightHotBarSlot());
		}
	}
}