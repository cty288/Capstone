using System;
using System.Collections.Generic;
using Runtime.GameResources.Model.Base;

namespace Runtime.Inventory.Model {
	[Serializable]
	public class InventorySlot
	{
		[ES3Serializable]
		private string ItemKey = null;
		//public int Quantity = 0;
		[ES3Serializable]
		private List<string> UUIDList;
		
		private HashSet<string> UUIDSet;

		private HashSet<string> UUIDSetMain {
			get {
				if (UUIDSet == null) {
					UUIDSet = new HashSet<string>(UUIDList);
				}
				return UUIDSet;
			}
		}

		public InventorySlot() {
			ItemKey = null;
			UUIDList = new List<string>();
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
			if (IsEmpty()) {
				ItemKey = null;
			}
			return true;
		}
		
		public bool ContainsItem(string uuid) {
			return UUIDSetMain.Contains(uuid);
		}
		
		
		public bool CanPlaceItem(IResourceEntity item) {
			if (IsEmpty()) {
				return true;
			}

			if (ItemKey == item.EntityName && GetQuantity() < item.GetMaxStackProperty().RealValue.Value && !ContainsItem(item.UUID)) {
				return true;
			}

			return false;
		}
		
		public List<string> GetUUIDList() {
			return UUIDList;
		}
	}
}