using System;
using System.Collections.Generic;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.ViewController;

namespace Runtime.Inventory.Model {
	[Serializable]
	public class ResourceSlot {
		public static ResourceSlotViewController currentHoveredSlot = null;
		[ES3Serializable]
		private string ItemKey = null;
		//public int Quantity = 0;
		[ES3Serializable]
		private List<string> UUIDList;
		private HashSet<string> UUIDSet;
		[ES3Serializable]
		private int slotMaxItemCount = -1;

		private HashSet<string> UUIDSetMain {
			get {
				if (UUIDSet == null) {
					UUIDSet = new HashSet<string>(UUIDList);
				}
				return UUIDSet;
			}
		}
		
		private Action<ResourceSlot, string, List<string>> OnSlotUpdateCallback;



		public ResourceSlot() {
			ItemKey = null;
			UUIDList = new List<string>();
		}
		
		public int SetSlotMaxItemCount(int minItemCount) {
			slotMaxItemCount = minItemCount;
			return slotMaxItemCount;
		}
		
		public bool IsEmpty() {
			return UUIDList.Count == 0;
		}
		
		public int GetQuantity() {
			return UUIDList.Count;
		}
		
		public bool TryAddItem(IResourceEntity item) {
			if (CanPlaceItem(item)) {
				if (IsEmpty()) {
					ItemKey = item.EntityName;
				}
				UUIDList.Add(item.UUID);
				UUIDSetMain.Add(item.UUID);
				item.OnPicked();
				this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
				return true;
			}
			return false;
		}
		

		
		public bool RemoveLastItem() {
			if (IsEmpty()) {
				return false;
			}
			string uuid = UUIDList[^1];
			UUIDList.RemoveAt(UUIDList.Count - 1);
			UUIDSetMain.Remove(uuid);
			if (IsEmpty()) {
				ItemKey = null;
			}
			this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
			return true;
		}
		
		public string GetLastItemUUID() {
			if (IsEmpty()) {
				return null;
			}
			return UUIDList[^1];
		}
		
		public bool RemoveItem(string uuid) {
			if (IsEmpty() || !ContainsItem(uuid)) {
				return false;
			}
			UUIDList.Remove(uuid);
			UUIDSetMain.Remove(uuid);
			if (IsEmpty()) {
				ItemKey = null;
			}
			this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
			return true;
		}
		
		public void Clear() {
			ItemKey = null;
			UUIDList.Clear();
			UUIDSetMain.Clear();
			this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
		}
		
		
		public static bool SwapSlotItems(ResourceSlot slot1, ResourceSlot slot2) {
			//translation
			//other slot: slot1
			//No . : slot 2
			if (!slot1.IsEmpty() && !slot2.CanPlaceItem(GlobalGameResourceEntities.GetAnyResource(slot1.GetLastItemUUID()), true)) {
				return false;
			}
			
			if(!slot2.IsEmpty() && !slot1.CanPlaceItem(GlobalGameResourceEntities.GetAnyResource(slot2.GetLastItemUUID()), true)) {
				return false;
			}
			
			
			(slot1.ItemKey, slot2.ItemKey) = (slot2.ItemKey, slot1.ItemKey);
			//swap uuid list and uuid set
			(slot1.UUIDList, slot2.UUIDList) = (slot2.UUIDList, slot1.UUIDList);
			(slot1.UUIDSet, slot2.UUIDSet) = (slot2.UUIDSet, slot1.UUIDSet);
			
			slot1.OnSlotUpdateCallback?.Invoke(slot1, slot1.GetLastItemUUID(), slot1.UUIDList);
			slot2.OnSlotUpdateCallback?.Invoke(slot2, slot2.GetLastItemUUID(), slot2.UUIDList);
			return true;
		}
		
		/// <summary>
		/// Try to move all items from other slot to this slot. If this slot can not store all items, try to move as many as possible.
		/// </summary>
		/// <param name="otherSlot"></param>
		/// <param name="topItem"></param>
		/// <returns></returns>
		public void TryMoveAllItemFromSlot(ResourceSlot otherSlot, IResourceEntity topItem) {
			if (otherSlot.IsEmpty() || !CanPlaceItem(topItem, true)) {
				return;
			}
			

			
			
			if (!IsEmpty() && ItemKey != otherSlot.ItemKey) {
				SwapSlotItems(this, otherSlot);
			}else {
				if (IsEmpty()) {
					ItemKey = otherSlot.ItemKey;
				}
			
				int maxCount = topItem.GetMaxStackProperty().RealValue.Value;
				if (slotMaxItemCount >= 0) {
					maxCount = Math.Min(maxCount, slotMaxItemCount);
				}
			
				int count = Math.Min(maxCount - GetQuantity(), otherSlot.GetQuantity());
			
				//move from the first item of other slot
				for (int i = 0; i < count; i++) {
					string uuid = otherSlot.UUIDList[0];
					otherSlot.UUIDList.RemoveAt(0);
					otherSlot.UUIDSetMain.Remove(uuid);
					UUIDList.Add(uuid);
					UUIDSetMain.Add(uuid);
				}
				if (otherSlot.IsEmpty()) {
					otherSlot.ItemKey = null;
				}
				this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
				otherSlot.OnSlotUpdateCallback?.Invoke(otherSlot, otherSlot.GetLastItemUUID(), otherSlot.UUIDList);
			}

			
		}
		
		public bool ContainsItem(string uuid) {
			return UUIDSetMain.Contains(uuid);
		}
		
		
		
		public virtual bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if (IsEmpty()) {
				return true;
			}

			int maxCount = item.GetMaxStackProperty().RealValue.Value;
			if (slotMaxItemCount >= 0) {
				maxCount = Math.Min(maxCount, slotMaxItemCount);
			}

			if (!isSwapping) {
				if (ItemKey == item.EntityName && GetQuantity() < maxCount && !ContainsItem(item.UUID)) {
					return true;
				}
			}else {
				return true;
			}


			return false;
		}
		
		public void RegisterOnSlotUpdateCallback(Action<ResourceSlot, string, List<string>> callback) {
			OnSlotUpdateCallback += callback;
		}
		
		public void UnregisterOnSlotUpdateCallback(Action<ResourceSlot, string, List<string>> callback) {
			OnSlotUpdateCallback -= callback;
		}
		public List<string> GetUUIDList() {
			return UUIDList;
		}
	}


	public abstract class HotBarSlot : ResourceSlot {
		public virtual bool GetCanSelect() {
			return true;
		}
	}
	

	[Serializable]
	public class LeftHotBarSlot : HotBarSlot {
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if(item == null) {
				return false;
			}
			if (item.GetResourceCategory() != ResourceCategory.Bait &&
			    item.GetResourceCategory() != ResourceCategory.Item) {
				return false;
			}
			return base.CanPlaceItem(item, isSwapping);
		}

		public override bool GetCanSelect() {
			return base.GetCanSelect();
		}
	}
	
	
	[Serializable]
	public class RightHotBarSlot : HotBarSlot {
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if (item.GetResourceCategory() != ResourceCategory.Weapon) {
				return false;
			}
			return base.CanPlaceItem(item, isSwapping);
		}
	}
	
	[Serializable]
	public class RubbishSlot : ResourceSlot {
		
	}
}