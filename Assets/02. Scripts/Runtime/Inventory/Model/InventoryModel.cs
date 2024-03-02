using System;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using UnityEngine;

namespace Runtime.Inventory.Model {
	public interface IInventoryModel : IModel, IResourceSlotsModel, ISavableModel {
		
		
		/// <summary>
		/// Add the specified number of slots to the inventory. <br/>
		/// Factors causing false: <br/>
		/// 1. The resulting number of slots exceeds the maximum number of slots. In this case, only the slots that can be added will be added.
		/// </summary>
		/// <param name="slotCount"></param>
		/// <returns></returns>
		bool AddSlots(int slotCount, out int actualAddedCount);
		
		/// <summary>
		/// Add the specified number of slots to the hotbar. <br/>
		/// </summary>
		/// <param name="slotCount"></param>
		/// <returns></returns>
		bool AddHotBarSlots(HotBarCategory category, int slotCount, Func<HotBarSlot> getter);
		

		//void InitWithInitialSlots();
		
		int GetHotBarSlotCount(HotBarCategory category);
		
		List<HotBarSlot> GetHotBarSlots(HotBarCategory category);
		
		void SelectHotBarSlot(HotBarCategory category, int index);
		
		//void SelectNextHotBarSlot(HotBarCategory category);
		
		//void SelectPreviousHotBarSlot(HotBarCategory category);
		
		int GetSelectedHotBarSlotIndex(HotBarCategory category);
		
		HotBarSlot GetSelectedHotBarSlot(HotBarCategory category);
		
		void ReplenishHotBarSlot(HotBarCategory category, HotBarSlot targetSlotToReplenish);
		
		HashSet<string> GetAllItemUUIDs();

		void AddToBaseStock(IResourceEntity entity);
		
		HashSet<PreparationSlot> GetBaseStock(ResourceCategory category);
		
		void RemoveFromBaseStock(ResourceCategory category, PreparationSlot slot);
		
		bool HasEntityInBaseStockByName(ResourceCategory category, string entityName);
		
		List<ResourceSlot> GetAllSlots(Predicate<ResourceSlot> predicate);
		void RemoveSlots(int slots, bool spawnRemovedItems);
		
		int MaxSlotCount { get; set; }
	}
	
	public struct OnInventorySlotAddedEvent {
		public int AddedCount;
		public List<ResourceSlot> AddedSlots;
	}
	
	public struct OnInventoryHotBarSlotAddedEvent {
		public HotBarCategory Category;
		public int AddedCount;
		public List<ResourceSlot> AddedSlots;
	}

	public enum HotBarCategory {
		None,
		Right,
		Left
	}

	[Serializable]
	public class HotBarSlotsInfo {
		public int CurrentSelectedIndex = 0;
		public List<HotBarSlot> Slots = new List<HotBarSlot>();
		
		public HotBarSlotsInfo() {
			
		}
	}
	
	public class OnHotBarSlotSelectedEvent {
		public HotBarCategory Category;
		public int SelectedIndex;
	}
	
	public struct OnInventoryItemAddedEvent {
		public IResourceEntity Item;
	}

	public struct OnInventorySlotRemovedEvent {
		public int RemovedCount;
		public List<ResourceSlot> RemovedSlots;
		public bool SpawnRemovedItems;
		public List<string> RemovedUUIDs;
	}

	public class InventoryModel: ResourceSlotsModel, IInventoryModel {

		[ES3Serializable]
		private Dictionary<HotBarCategory, HotBarSlotsInfo> hotBarSlots =
			new Dictionary<HotBarCategory, HotBarSlotsInfo>();

		[ES3Serializable]
		private Dictionary<ResourceCategory, HashSet<PreparationSlot>> baseStockedItems =
			new Dictionary<ResourceCategory, HashSet<PreparationSlot>>();

		//[ES3Serializable]
		
		
		[field: ES3Serializable]
		public int MaxSlotCount { get; set; } = 32;
		
		public static Dictionary<HotBarCategory, int> MaxHotBarSlotCount = new Dictionary<HotBarCategory, int>() {
			{HotBarCategory.Right, 3},
			{HotBarCategory.Left, 5}
		};
		

		

		
		
		public bool AddHotBarSlots(HotBarCategory category, int slotCount, Func<HotBarSlot> getter) {
			int actualAddedCount = slotCount;
			if (category == HotBarCategory.None) {
				return false;
			}
			
			if (!hotBarSlots.ContainsKey(category)) {
				hotBarSlots.Add(category, new HotBarSlotsInfo());
			}
			
			int curCount = GetHotBarSlotCount(category);
			int maxCount = MaxHotBarSlotCount[category];
			if (slotCount + curCount > maxCount) {
				actualAddedCount = maxCount - curCount;
			}
			
		
			//insert to the end of slots[slotCount-1]
			List<ResourceSlot> addedSlots = new List<ResourceSlot>();
			for (int i = 0; i < actualAddedCount; i++) {
				HotBarSlot slot = getter.Invoke();
				hotBarSlots[category].Slots.Add(slot);
				addedSlots.Add(slot);
			}
			
			
			this.SendEvent<OnInventoryHotBarSlotAddedEvent>(new OnInventoryHotBarSlotAddedEvent() {
				Category = category,
				AddedCount = actualAddedCount,
				AddedSlots = addedSlots
			});
			return actualAddedCount == slotCount;
		}

		public override bool AddItem(IResourceEntity item, out ResourceSlot addedSlot) {
			//check each hot bar first
			foreach (var hotBarSlot in hotBarSlots) {
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (!slot.IsEmpty() && slot.CanPlaceItem(item)) {
						addedSlot = slot;
						return AddItemAt(item, slot);
					}
				}
				
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (slot.CanPlaceItem(item)) {
						addedSlot = slot;
						return AddItemAt(item, slot);
					}
				}
			}

			return base.AddItem(item, out addedSlot);
		}
		
		
		
		public void ReplenishHotBarSlot(HotBarCategory category, HotBarSlot targetSlotToReplenish) {
			if (!targetSlotToReplenish.IsEmpty()) {
				return;
			}
			foreach (ResourceSlot resourceSlot in slots) {
				if (resourceSlot.IsEmpty()) {
					continue;
				}

				IResourceEntity resourceEntity = GlobalGameResourceEntities.GetAnyResource(resourceSlot.GetLastItemUUID());
				
				if (targetSlotToReplenish.CanPlaceItem(resourceEntity)) {
					resourceSlot.RemoveLastItem();
					AddItemAt(resourceEntity, targetSlotToReplenish);
					break;
				}
			}
		}

		public HashSet<string> GetAllItemUUIDs() {
			HashSet<string> uuids = new HashSet<string>();
			foreach (var hotBarSlot in hotBarSlots) {
				foreach (var slot in hotBarSlot.Value.Slots) {
					uuids.UnionWith(slot.GetUUIDList());
				}
			}

			foreach (ResourceSlot slot in slots) {
				uuids.UnionWith(slot.GetUUIDList());
			}
			
			return uuids;
		}

		public void AddToBaseStock(IResourceEntity entity) {
			ResourceCategory category = entity.GetResourceCategory();
			if (!baseStockedItems.ContainsKey(entity.GetResourceCategory())) {
				baseStockedItems.Add(category, new HashSet<PreparationSlot>());
			}

			HashSet<PreparationSlot> slots = baseStockedItems[category];
			bool addSuccess = false;
			//see if any slot can place this item
			foreach (PreparationSlot slot in slots) {
				if (slot.CanPlaceItem(entity)) {
					if (slot.TryAddItem(entity)) {
						addSuccess = true;
						break;
					}
				}
			}

			if (!addSuccess) {
				PreparationSlot newSlot = new PreparationSlot();
				newSlot.TryAddItem(entity);
				slots.Add(newSlot);
			}
		}

		public HashSet<PreparationSlot> GetBaseStock(ResourceCategory category) {
			if (!baseStockedItems.ContainsKey(category)) {
				return null;
			}

			return baseStockedItems[category];
		}

		public void RemoveFromBaseStock(ResourceCategory category, PreparationSlot slot) {
			if (!baseStockedItems.ContainsKey(category)) {
				return;
			}

			baseStockedItems[category].Remove(slot);
		}

		public bool HasEntityInBaseStockByName(ResourceCategory category, string entityName) {
			if (!baseStockedItems.ContainsKey(category)) {
				return false;
			}

			foreach (PreparationSlot slot in baseStockedItems[category]) {
				if (slot.EntityKey == entityName) {
					return true;
				}
			}

			return false;
		}

		public List<ResourceSlot> GetAllSlots(Predicate<ResourceSlot> predicate) {
			List<ResourceSlot> result = new List<ResourceSlot>();
			foreach (var hotBarSlot in hotBarSlots) {
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (predicate.Invoke(slot)) {
						result.Add(slot);
					}
				}
			}

			foreach (ResourceSlot slot in slots) {
				if (predicate.Invoke(slot)) {
					result.Add(slot);
				}
			}

			return result;
		}

		


		protected override bool AddItemAt(IResourceEntity item, ResourceSlot slot) {
			return base.AddItemAt(item, slot);
		}
		
	
		public override bool RemoveItem(string uuid) {
			//check each hot bar first
			foreach (var hotBarSlot in hotBarSlots) {
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (RemoveItemFromSlot(uuid, slot)) {
						return true;
					}
				}
			}
			for (int i = 0; i < GetSlotCount(); i++) {
				if (slots[i].ContainsItem(uuid)) {
					RemoveItemFromSlot(uuid, slots[i]);
					return true;
				}
			}
			return false;
		}
		
		protected bool RemoveItemFromSlot(string uuid, ResourceSlot slot) {
			if (slot.RemoveItem(uuid)) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
				entity.OnRemovedFromInventory();
				entity.UnRegisterOnEntityRecycled(OnEntityRecycled);
				return true;
			}
			return false;
		}

		public override bool CanPlaceItem(IResourceEntity item) {
			//check each hot bar first
			foreach (var hotBarSlot in hotBarSlots) {
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (slot.CanPlaceItem(item)) {
						return true;
					}
				}
			}
			return base.CanPlaceItem(item);
		}

		public bool AddSlots(int slotCount, out int actualAddedCount) {
			actualAddedCount = slotCount;
			if (slotCount + GetSlotCount() > MaxSlotCount) {
				actualAddedCount = MaxSlotCount - GetSlotCount();
			}
			
			
			List<ResourceSlot> addedSlots = new List<ResourceSlot>();
			for (int i = 0; i < actualAddedCount; i++) {
				ResourceSlot slot = new ResourceSlot();
				slots.Add(slot);
				addedSlots.Add(slot);
			}
			
			this.SendEvent<OnInventorySlotAddedEvent>(new OnInventorySlotAddedEvent() {
				AddedCount = actualAddedCount,
				AddedSlots = addedSlots
			});
			return actualAddedCount == slotCount;
		}
		
		
		public void RemoveSlots(int slots, bool spawnRemovedItems) {
			int actualCount = Mathf.Min(slots, GetSlotCount());
			List<ResourceSlot> removedSlots = new List<ResourceSlot>();
			List<string> removedUUIDs = new List<string>();
			for (int i = 0; i < actualCount; i++) {
				ResourceSlot slot = this.slots[^1];
				List<string> uuids = new List<string>(slot.GetUUIDList());
				this.slots.RemoveAt(this.slots.Count - 1);
				removedSlots.Add(slot);
				foreach (string uuid in uuids) {
					if (RemoveItemFromSlot(uuid, slot)) {
						removedUUIDs.AddRange(uuids);
					}
				}
			}
			
			this.SendEvent<OnInventorySlotRemovedEvent>(new OnInventorySlotRemovedEvent() {
				RemovedCount = actualCount,
				RemovedSlots = removedSlots,
				SpawnRemovedItems = spawnRemovedItems,
				RemovedUUIDs = removedUUIDs
			});
		}
		

		protected override void OnInit() {
			base.OnInit();
			if (IsFirstTimeCreated) { //not load from saved
				slots = new List<ResourceSlot>();
			}
			
		}

		public int GetHotBarSlotCount(HotBarCategory category) {
			return hotBarSlots[category].Slots.Count;
		}

		public List<HotBarSlot> GetHotBarSlots(HotBarCategory category) {
			if (!hotBarSlots.ContainsKey(category)) {
				return null;
			}
			return hotBarSlots[category].Slots;
		}

		public void SelectHotBarSlot(HotBarCategory category, int index) {
			HotBarSlotsInfo info = hotBarSlots[category];
			if (index < 0 || index >= info.Slots.Count) {
				return;
			}
			info.CurrentSelectedIndex = index;

			this.SendEvent<OnHotBarSlotSelectedEvent>(new OnHotBarSlotSelectedEvent() {
				Category = category,
				SelectedIndex = index
			});
		}

		public void SelectNextHotBarSlot(HotBarCategory category) {
			HotBarSlotsInfo info = hotBarSlots[category];
			if (info.Slots.Count == 0) {
				return;
			}
			info.CurrentSelectedIndex = (info.CurrentSelectedIndex + 1) % info.Slots.Count;
			this.SendEvent<OnHotBarSlotSelectedEvent>(new OnHotBarSlotSelectedEvent() {
				Category = category,
				SelectedIndex = info.CurrentSelectedIndex
			});
		}

		public void SelectPreviousHotBarSlot(HotBarCategory category) {
			HotBarSlotsInfo info = hotBarSlots[category];
			if (info.Slots.Count == 0) {
				return;
			}
			info.CurrentSelectedIndex = (info.CurrentSelectedIndex - 1 + info.Slots.Count) % info.Slots.Count;
			this.SendEvent<OnHotBarSlotSelectedEvent>(new OnHotBarSlotSelectedEvent() {
				Category = category,
				SelectedIndex = info.CurrentSelectedIndex
			});
		}

		public int GetSelectedHotBarSlotIndex(HotBarCategory category) {
			return hotBarSlots[category].CurrentSelectedIndex;
		}

		public HotBarSlot GetSelectedHotBarSlot(HotBarCategory category) {
			HotBarSlotsInfo info = hotBarSlots[category];
			if (info.Slots.Count == 0) {
				return null;
			}
			return info.Slots[info.CurrentSelectedIndex];
		}



		public override void Clear() {
			if (slots == null) {
				return;
			}
			
			
			
			foreach (var resourceSlot in slots) {
				while (!resourceSlot.IsEmpty()) {
					string uuid = resourceSlot.GetLastItemUUID();
					GlobalEntities.GetEntityAndModel(uuid).Item2.RemoveEntity(uuid);
				}
			}
			
			foreach (HotBarCategory category in Enum.GetValues(typeof(HotBarCategory))) {
				if (!hotBarSlots.ContainsKey(category)) {
					continue;
				}

				foreach (ResourceSlot resourceSlot in hotBarSlots[category].Slots) {
					while (!resourceSlot.IsEmpty()) {
						string uuid = resourceSlot.GetLastItemUUID();
						GlobalEntities.GetEntityAndModel(uuid).Item2.RemoveEntity(uuid);
					}
				}
			}
			
		}

		public override void Reset() {
			Clear();
			slots.Clear();
		}
	}
}