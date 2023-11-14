using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities;

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
		
		//private Dictionary<HotBarCategory, ResourceSlot> currentSelectedSlot = new Dictionary<HotBarCategory, ResourceSlot>();
		private HotBarCategory currentSelectedCategory = HotBarCategory.Left;
		private HotBarSlot currentSelectedSlot = null;
		private int currentSelectedIndex = 0;
		private ReferenceCounter lockSwitchCounter = new ReferenceCounter();
		private ICurrencyModel currencyModel;
		
		
		private Dictionary<ResourceSlot, HotBarCategory> slotToCategories = new Dictionary<ResourceSlot, HotBarCategory>();

		protected override void OnInit() {
			base.OnInit();

			currencyModel = this.GetModel<ICurrencyModel>();
			//this.RegisterEvent<OnHotBarSlotSelectedEvent>(OnHotBarSlotSelected);
			if (model.IsFirstTimeCreated) {
				ResetSlots();
			}
			else {
				//model.SelectHotBarSlot(HotBarCategory.Left, model.GetSelectedHotBarSlotIndex(HotBarCategory.Left));
				//model.SelectHotBarSlot(HotBarCategory.Right, model.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
				SelectHotBarSlot(HotBarCategory.Right, model.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
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

			if (category == HotBarCategory.Left && slot.GetQuantity() <= 0) {
				//model.SelectHotBarSlot(HotBarCategory.Right, model.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
				SelectHotBarSlot(HotBarCategory.Right, model.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
			}
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
			model.Reset();
			AddInitialSlots();
			//model.SelectHotBarSlot(HotBarCategory.Left, 0);
			//model.SelectHotBarSlot(HotBarCategory.Right, 0);
			SelectHotBarSlot(HotBarCategory.Right, 0);
			SelectHotBarSlot(HotBarCategory.Left, 0);
		}

		public override void ClearSlots() {
			model.Clear();
		}

		public IResourceEntity GetCurrentlySelectedEntity() {
			if (currentSelectedSlot == null) {
				return null;
			}
			string uuid = currentSelectedSlot.GetLastItemUUID();
			if (uuid == null) {
				return null;
			}
			return GlobalGameResourceEntities.GetAnyResource(uuid);
		}

		public void SelectHotBarSlot(HotBarCategory category, int index) {
			if (lockSwitchCounter.Count > 0) {
				return;
			}
			
			if(model.GetHotBarSlots(category).Count <= index) {
				return;
			}
			
			HotBarSlot targetSlot = model.GetHotBarSlots(category)[index];
			HotBarSlot previousSlot = this.currentSelectedSlot;
			HotBarCategory previousCategory = currentSelectedCategory;
			
			int previousIndex = currentSelectedIndex;
			
			if (previousSlot != null) {
				previousSlot.UnregisterOnSlotUpdateCallback(OnCurrentSlotUpdate);
			}
			
			
			HotBarCategory targetCategory = category;
			int targetIndex = index;
			
			
			//if category is left and last selected index = current selected index, then we select the right hand again
			if ((previousCategory == HotBarCategory.Left && previousSlot == targetSlot) || (category == HotBarCategory.Left && targetSlot.GetQuantity() <= 0)) {

				HotBarCategory otherCategory = HotBarCategory.Right;
				int otherIndex = 0;
				
				if (previousCategory == HotBarCategory.Left && category == HotBarCategory.Left &&
				    targetSlot?.GetQuantity() <= 0 && previousIndex != index) { //try to switch to an empty left slot
					return;

				}
				else {
					otherCategory = HotBarCategory.Right;
					otherIndex = model.GetSelectedHotBarSlotIndex(HotBarCategory.Right);
					targetSlot = model.GetHotBarSlots(otherCategory)[otherIndex];
				}
				targetCategory = otherCategory;
				targetIndex = otherIndex;
			}


			string lastItemUUID = targetSlot.GetLastItemUUID();
		
			if (!CanSelect(targetSlot)) {
				if (targetCategory == HotBarCategory.Left) {
					return;
				}
				else {
					lastItemUUID = null;
				}
			}
			
			
			
			this.currentSelectedSlot = targetSlot;
			currentSelectedCategory = targetCategory;
			slotToCategories.TryAdd(targetSlot, targetCategory);
			currentSelectedIndex = targetIndex;
			model.SelectHotBarSlot(targetCategory, targetIndex);
			targetSlot.RegisterOnSlotUpdateCallback(OnCurrentSlotUpdate);
			OnCurrentSlotUpdate(targetSlot, lastItemUUID, targetSlot.GetUUIDList());
		}

		public void ForceUpdateCurrentHotBarSlotCanSelect() {
			if (currentSelectedSlot == null) {
				return;
			}

			if (CanSelect(currentSelectedSlot)) {
				return;
			}

			SelectHotBarSlot(HotBarCategory.Right, model.GetSelectedHotBarSlotIndex(HotBarCategory.Right));
		}
		
		

		private bool CanSelect(HotBarSlot slot) {
			IResourceEntity resource = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
			return slot.GetCanSelect(resource, currencyModel.GetCurrencyAmountDict());
		}
 
		public void SelectNextHotBarSlot(HotBarCategory category) {
			
			SelectHotBarSlot(category,
				(model.GetSelectedHotBarSlotIndex(category) + 1) % model.GetHotBarSlots(category).Count);
		}

		public void SelectPreviousHotBarSlot(HotBarCategory category) {
			SelectHotBarSlot(category,
				(model.GetSelectedHotBarSlotIndex(category) - 1 + model.GetHotBarSlots(category).Count) %
				model.GetHotBarSlots(category).Count);
		}

		public void RetainLockSwitch(object locker) {
			lockSwitchCounter.Retain(locker);
		}

		public void ReleaseLockSwitch(object locker) {
			lockSwitchCounter.Release(locker);
		}

		public bool AddItem(IResourceEntity item, bool sendEvent = true) {
			if (model.AddItem(item)) {
				if (sendEvent) {
					if (sendEvent) {
						this.SendEvent<OnInventoryItemAddedEvent>(new OnInventoryItemAddedEvent() {
							Item = item
						});
					}
					item.OnAddedToInventory();
				}
				return true;
			}

			return false;
		}

		public bool CanPlaceItem(IResourceEntity item) {
			return model.CanPlaceItem(item);
		}

		public bool RemoveItem(IResourceEntity entity) {
			return model.RemoveItem(entity?.UUID);
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