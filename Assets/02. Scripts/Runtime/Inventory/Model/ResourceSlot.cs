using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.BindableProperty;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;

namespace Runtime.Inventory.Model {
	[Serializable]
	public class ResourceSlot {
		public static ResourceSlotViewController currentHoveredSlot = null;

		public static BindableProperty<ResourceSlot> currentDraggingSlot =
			new BindableProperty<ResourceSlot>(null);
		[ES3Serializable]
		protected string ItemKey = null;

		public string EntityKey => ItemKey;
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
		private Action<ResourceSlot, string, string, List<string>> OnSlotUpdateCallback2;

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
		
		public virtual bool TryAddItem(IResourceEntity item) {
			if (CanPlaceItem(item)) {
				string previousTopItemUUID = GetLastItemUUID();
				if (IsEmpty()) {
					ItemKey = item.EntityName;
				}
				UUIDList.Add(item.UUID);
				UUIDSetMain.Add(item.UUID);
				item.OnAddedToSlot();
				this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
				this.OnSlotUpdateCallback2?.Invoke(this, previousTopItemUUID, GetLastItemUUID(), UUIDList);
				return true;
			}
			return false;
		}
		

		
		public virtual bool RemoveLastItem() {
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
			this.OnSlotUpdateCallback2?.Invoke(this, uuid, GetLastItemUUID(), UUIDList);
			
			return true;
		}
		
		public string GetLastItemUUID() {
			if (IsEmpty()) {
				return null;
			}
			return UUIDList[^1];
		}
		
		public virtual bool RemoveItem(string uuid) {
			if (IsEmpty() || !ContainsItem(uuid)) {
				return false;
			}
			string previousTopItemUUID = GetLastItemUUID();
			UUIDList.Remove(uuid);
			UUIDSetMain.Remove(uuid);
			if (IsEmpty()) {
				ItemKey = null;
			}
			this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
			this.OnSlotUpdateCallback2?.Invoke(this, previousTopItemUUID, GetLastItemUUID(), UUIDList);
			return true;
		}
		
		public virtual void Clear() {
			string previousTopItemUUID = GetLastItemUUID();
			ItemKey = null;
			UUIDList.Clear();
			UUIDSetMain.Clear();
			this.OnSlotUpdateCallback?.Invoke(this, GetLastItemUUID(), UUIDList);
			this.OnSlotUpdateCallback2?.Invoke(this, previousTopItemUUID, GetLastItemUUID(), UUIDList);
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
			string slot1PreviousTopItemUUID = slot1.GetLastItemUUID();
			string slot2PreviousTopItemUUID = slot2.GetLastItemUUID();
			
			
			(slot1.ItemKey, slot2.ItemKey) = (slot2.ItemKey, slot1.ItemKey);
			//swap uuid list and uuid set
			(slot1.UUIDList, slot2.UUIDList) = (slot2.UUIDList, slot1.UUIDList);
			(slot1.UUIDSet, slot2.UUIDSet) = (slot2.UUIDSet, slot1.UUIDSet);
			
			slot1.OnSlotUpdateCallback?.Invoke(slot1, slot1.GetLastItemUUID(), slot1.UUIDList);
			slot1.OnSlotUpdateCallback2?.Invoke(slot1, slot1PreviousTopItemUUID, slot1.GetLastItemUUID(), slot1.UUIDList);
			
			
			slot2.OnSlotUpdateCallback?.Invoke(slot2, slot2.GetLastItemUUID(), slot2.UUIDList);
			slot2.OnSlotUpdateCallback2?.Invoke(slot2, slot2PreviousTopItemUUID, slot2.GetLastItemUUID(),
				slot2.UUIDList);
			
			return true;
		}
		
		/// <summary>
		/// Try to move all items from other slot to this slot. If this slot can not store all items, try to move as many as possible.
		/// </summary>
		/// <param name="otherSlot"></param>
		/// <param name="topItem"></param>
		/// <returns></returns>
		public bool TryMoveAllItemFromSlot(ResourceSlot otherSlot, IResourceEntity topItem) {
			if (otherSlot.IsEmpty() || !CanPlaceItem(topItem, true)) {
				return false;
			}
			

			
			
			if (!IsEmpty() && ItemKey != otherSlot.ItemKey) {
				return SwapSlotItems(this, otherSlot);
			}else {
				string previousTopItemUUID = GetLastItemUUID();
				string otherSlotPreviousTopItemUUID = otherSlot.GetLastItemUUID();
				
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
				this.OnSlotUpdateCallback2?.Invoke(this, previousTopItemUUID, GetLastItemUUID(), UUIDList);
				otherSlot.OnSlotUpdateCallback?.Invoke(otherSlot, otherSlot.GetLastItemUUID(), otherSlot.UUIDList);
				otherSlot.OnSlotUpdateCallback2?.Invoke(otherSlot, otherSlotPreviousTopItemUUID, otherSlot.GetLastItemUUID(), otherSlot.UUIDList);
			}
			
			return true;

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
		public void RegisterOnSlotUpdateCallback(Action<ResourceSlot, string, string, List<string>> callback) {
			OnSlotUpdateCallback2 += callback;
		}
		
		public void UnregisterOnSlotUpdateCallback(Action<ResourceSlot, string, List<string>> callback) {
			OnSlotUpdateCallback -= callback;
		}
		
		public void UnregisterOnSlotUpdateCallback(Action<ResourceSlot, string, string, List<string>> callback) {
			OnSlotUpdateCallback2 -= callback;
		}
		
		public List<string> GetUUIDList() {
			return UUIDList;
		}

		public virtual bool GetCanThrow(IResourceEntity item) {
			if (item == null || item is ISkillEntity) {
				return false;
			}

			return true;
		}
	}


	public abstract class HotBarSlot : ResourceSlot {
		public virtual bool GetCanSelect(IResourceEntity topItem, Dictionary<CurrencyType, int> currencyDict) {
			if(topItem == null || topItem.CanInventorySwitchToCondition == null) {
				return true;
			}
			
			return topItem.CanInventorySwitchToCondition(currencyDict);
		}
	}
	

	[Serializable]
	public class LeftHotBarSlot : HotBarSlot {
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if(item == null) {
				return false;
			}
			if (item.GetResourceCategory() != ResourceCategory.Bait &&
			    item.GetResourceCategory() != ResourceCategory.Item &&
			    item.GetResourceCategory() != ResourceCategory.Skill) {
				return false;
			}
			return base.CanPlaceItem(item, isSwapping);
		}
		
	}
	
	
	[Serializable]
	public class RightHotBarSlot : HotBarSlot {
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if (item!=null && item.GetResourceCategory() != ResourceCategory.Weapon) {
				return false;
			}
			return base.CanPlaceItem(item, isSwapping);
		}
	}
	
	[Serializable]
	public class RubbishSlot : ResourceSlot {
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if (item!=null && item.GetResourceCategory() == ResourceCategory.Skill) {
				return false;
			}

			return base.CanPlaceItem(item, isSwapping);
		}
	}
	
	[Serializable]
	public class UpgradeSlot : ResourceSlot {
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if (item == null || item.GetResourceCategory() != ResourceCategory.Skill) {
				return false;
			}

			return true;
		}
	}

	[Serializable]
	public class PreparationSlot : ResourceSlot {
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			//infinite size
			if (IsEmpty()) {
				return true;
			}

			ResourceCategory category = item.GetResourceCategory();
			if (category == ResourceCategory.Skill || category == ResourceCategory.Weapon ||
			    category == ResourceCategory.WeaponParts) {
				int maxCount = item.GetMaxStackProperty().RealValue.Value;
				if(GetQuantity() >= maxCount) {
					return false;
				}
			}
			
			

			if (!isSwapping) {
				if (ItemKey == item.EntityName && !ContainsItem(item.UUID)) {
					return true;
				}
			}else {
				return true;
			}


			return false;
		}
	}
	
	[Serializable]
	public class WeaponPartsSlot : ResourceSlot {

		[field: ES3Serializable]
		private WeaponPartType weaponPartType;
		
		public WeaponPartsSlot(IWeaponPartsEntity entity) : base() {
			SetSlotMaxItemCount(1);
			this.weaponPartType = entity.WeaponPartType;
			TryAddItem(entity);
		}
		
		public WeaponPartsSlot() : base() {
			SetSlotMaxItemCount(1);
		}
		
		
		public WeaponPartsSlot(WeaponPartType type) : base() {
			SetSlotMaxItemCount(1);
			this.weaponPartType = type;
		}
		
		
		public override bool CanPlaceItem(IResourceEntity item, bool isSwapping = false) {
			if (item!=null && item.GetResourceCategory() != ResourceCategory.WeaponParts) {
				return false;
			}
			
			if (item is IWeaponPartsEntity weaponParts) {
				if (weaponParts.WeaponPartType != weaponPartType) {
					return false;
				}
			}
			
			return base.CanPlaceItem(item, isSwapping);
		}
	}
}