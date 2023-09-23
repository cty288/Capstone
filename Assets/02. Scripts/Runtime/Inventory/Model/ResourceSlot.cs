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
		
		
		/// <summary>
		/// Try to move all items from other slot to this slot. If this slot can not store all items, try to move as many as possible.
		/// </summary>
		/// <param name="otherSlot"></param>
		/// <param name="topItem"></param>
		/// <returns></returns>
		public void TryMoveAllItemFromSlot(ResourceSlot otherSlot, IResourceEntity topItem) {
			if (otherSlot.IsEmpty() || !CanPlaceItem(topItem)) {
				return;
			}
			
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
			otherSlot.OnSlotUpdateCallback?.Invoke(this, otherSlot.GetLastItemUUID(), otherSlot.UUIDList);
		}
		
		public bool ContainsItem(string uuid) {
			return UUIDSetMain.Contains(uuid);
		}
		
		
		
		public virtual bool CanPlaceItem(IResourceEntity item) {
			if (IsEmpty()) {
				return true;
			}

			int maxCount = item.GetMaxStackProperty().RealValue.Value;
			if (slotMaxItemCount >= 0) {
				maxCount = Math.Min(maxCount, slotMaxItemCount);
			}
			if (ItemKey == item.EntityName && GetQuantity() < maxCount && !ContainsItem(item.UUID)) {
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


	[Serializable]
	public class LeftHotBarSlot : ResourceSlot {
		public override bool CanPlaceItem(IResourceEntity item) {
			if (item.GetResourceCategory() != ResourceCategory.Bait &&
			    item.GetResourceCategory() != ResourceCategory.Trap) {
				return false;
			}
			return base.CanPlaceItem(item);
		}
	}
	
	
	[Serializable]
	public class RightHotBarSlot : ResourceSlot {
		public override bool CanPlaceItem(IResourceEntity item) {
			if (item.GetResourceCategory() != ResourceCategory.Weapon) {
				return false;
			}
			return base.CanPlaceItem(item);
		}
	}
	
	[Serializable]
	public class RubbishSlot : ResourceSlot {
		
	}
}