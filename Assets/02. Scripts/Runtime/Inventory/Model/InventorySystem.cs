using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Player.Commands;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
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
			
			this.RegisterEvent<OnReturnToBase>(OnPlayerReturnToBase);
			this.RegisterEvent<OnPlayerRespawn>(OnPlayerRespawn);
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

		public void MoveItemFromBaseStockToInventory(ResourceCategory category, PreparationSlot slot) {
			model.RemoveFromBaseStock(category, slot);
			if(slot.GetQuantity() > 0) {
				foreach (string id in slot.GetUUIDList()) {
					IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(id);
					if (entity != null) {
						AddItem(entity, false);
					}
				}
			}
		}

		public void RemoveResourceEntityFromBaseStock(ResourceCategory category, string resourceName, int count, bool alsoRemoveEntity) {
			HashSet<PreparationSlot> slots = model.GetBaseStock(category);
			
			PreparationSlot slotToRemove = null;
			foreach (PreparationSlot slot in slots) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
				if (entity.EntityName != resourceName) {
					continue;
				}
				
				for (int i = 0; i < count; i++) {
					string uuid = slot.GetLastItemUUID();
					if (uuid == null) {
						break;
					}
					slot.RemoveLastItem();
					if (alsoRemoveEntity) {
						(_, IEntityModel model) = GlobalEntities.GetEntityAndModel(uuid);
						model?.RemoveEntity(uuid, true);
					}
					
				}
				
				if (slot.IsEmpty()) {
					slotToRemove = slot;
				}
				break;
			}
			
			if (slotToRemove != null) {
				model.RemoveFromBaseStock(category, slotToRemove);
			}
		}


		private void AddInitialSlots() {
			model.AddSlots(InitialSlotCount);

			model.AddHotBarSlots(HotBarCategory.Left, InitialHotBarSlotCount[HotBarCategory.Left],
				()=>new LeftHotBarSlot());
				
			model.AddHotBarSlots(HotBarCategory.Right, InitialHotBarSlotCount[HotBarCategory.Right],
				()=>new RightHotBarSlot());
		}
		private void OnPlayerRespawn(OnPlayerRespawn obj) {
			HashSet<string> allItems = model.GetAllItemUUIDs();
			bool hasDefaultWeapon = false;
			foreach (string item in allItems) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(item);
				if (entity.GetResourceCategory() == ResourceCategory.Skill) { //skills are preserved; other items are removed
					IResourceEntity returnToBaseEntity = entity.GetReturnToBaseEntity();
					model.AddToBaseStock(returnToBaseEntity);
				}
				
				model.RemoveItem(item);
				if (entity.EntityName == "RustyPistol") {
					hasDefaultWeapon = true;
				}
				(_, IEntityModel entityModel) = GlobalEntities.GetEntityAndModel(entity.UUID);
				entityModel.RemoveEntity(entity.UUID, true);
			}

			if (hasDefaultWeapon) {
				IResourceEntity defaultWeapon = ResourceVCFactory.Singleton.SpawnNewResourceEntity("RustyPistol");
				model.AddToBaseStock(defaultWeapon);
			}
		}
		
		private void OnPlayerReturnToBase(OnReturnToBase e) {
			//remove all items in the inventory; call their GetReturnToBaseEntity() to get the entity when the player returns to base
			//(since some entities might change when the player returns to base)
			//if the returned entity's uuid is different from the original entity's uuid, then recycle the original entity
			//add the returned entity uuid to baseStock
			
			HashSet<string> allItems = model.GetAllItemUUIDs();
			bool hasDefaultWeapon = false;
			foreach (string item in allItems) {
				model.RemoveItem(item);

				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(item);
				IResourceEntity returnToBaseEntity = entity.GetReturnToBaseEntity();

				if (entity.EntityName == "RustyPistol") {
					hasDefaultWeapon = true;
				}
				
				if (returnToBaseEntity.UUID != entity.UUID) {
					(_, IEntityModel model) = GlobalEntities.GetEntityAndModel(entity.UUID);
					model.RemoveEntity(entity.UUID, true);
				}

				model.AddToBaseStock(returnToBaseEntity);
			}

			if (!hasDefaultWeapon) {
				if (!model.HasEntityInBaseStockByName(ResourceCategory.Weapon ,"RustyPistol")) {
					IResourceEntity defaultWeapon = ResourceVCFactory.Singleton.SpawnNewResourceEntity("RustyPistol");
					model.AddToBaseStock(defaultWeapon);
				}
			}

			this.SendEvent<OnShowGameHint>(new OnShowGameHint() {
				duration = 3f,
				text = Localization.Get("HINT_ITEM_RETURN")
			});
		}
	}
}