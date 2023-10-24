using System;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	public interface IInventoryModel : IModel, IResourceSlotsModel, ISavableModel {
		
		
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
		bool AddHotBarSlots(HotBarCategory category, int slotCount, Func<ResourceSlot> getter);
		

		//void InitWithInitialSlots();
		
		int GetHotBarSlotCount(HotBarCategory category);
		
		List<ResourceSlot> GetHotBarSlots(HotBarCategory category);
		
		void SelectHotBarSlot(HotBarCategory category, int index);
		
		void SelectNextHotBarSlot(HotBarCategory category);
		
		void SelectPreviousHotBarSlot(HotBarCategory category);
		
		int GetSelectedHotBarSlotIndex(HotBarCategory category);
		
		ResourceSlot GetSelectedHotBarSlot(HotBarCategory category);
		
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
		public List<ResourceSlot> Slots = new List<ResourceSlot>();
		
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
	
	public class InventoryModel: ResourceSlotsModel, IInventoryModel {

		[ES3Serializable]
		private Dictionary<HotBarCategory, HotBarSlotsInfo> hotBarSlots =
			new Dictionary<HotBarCategory, HotBarSlotsInfo>();

		
		
		public static int MaxSlotCount = 32;
		
		public static Dictionary<HotBarCategory, int> MaxHotBarSlotCount = new Dictionary<HotBarCategory, int>() {
			{HotBarCategory.Right, 3},
			{HotBarCategory.Left, 5}
		};
		

		

		
		
		public bool AddHotBarSlots(HotBarCategory category, int slotCount, Func<ResourceSlot> getter) {
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
				ResourceSlot slot = getter.Invoke();
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

		public override bool AddItem(IResourceEntity item) {
			//check each hot bar first
			foreach (var hotBarSlot in hotBarSlots) {
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (!slot.IsEmpty() && slot.CanPlaceItem(item)) {
						return AddItemAt(item, slot);
					}
				}
				
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (slot.CanPlaceItem(item)) {
						return AddItemAt(item, slot);
					}
				}
			}
			return base.AddItem(item);
		}

		protected override bool AddItemAt(IResourceEntity item, ResourceSlot slot) {
			if (base.AddItemAt(item, slot)) {
				this.SendEvent<OnInventoryItemAddedEvent>(new OnInventoryItemAddedEvent() {
					Item = item
				});
				return true;
			}

			return false;
		}

		public override bool RemoveItem(string uuid) {
			//check each hot bar first
			foreach (var hotBarSlot in hotBarSlots) {
				foreach (var slot in hotBarSlot.Value.Slots) {
					if (slot.RemoveItem(uuid)) {
						IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
						entity.UnRegisterOnEntityRecycled(OnEntityRecycled);
						return true;
					}
				}
			}
			return base.RemoveItem(uuid);
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

		public bool AddSlots(int slotCount) {
			int actualAddedCount = slotCount;
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

		/*public void InitWithInitialSlots() {
			
		}*/

		protected override void OnInit() {
			base.OnInit();
			if (IsFirstTimeCreated) { //not load from saved
				slots = new List<ResourceSlot>();
			}
			
		}

		public int GetHotBarSlotCount(HotBarCategory category) {
			return hotBarSlots[category].Slots.Count;
		}

		public List<ResourceSlot> GetHotBarSlots(HotBarCategory category) {
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

		public ResourceSlot GetSelectedHotBarSlot(HotBarCategory category) {
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