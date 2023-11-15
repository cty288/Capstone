using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Utilities;
using UnityEngine;

public class UpgradeSlotViewController : ResourceSlotViewController {
	protected override void Awake() {
		base.Awake();
		
	}


	protected override void OnCurrentDraggingSlotChanged(ResourceSlot oldSlot, ResourceSlot newSlot) {
		base.OnCurrentDraggingSlotChanged(oldSlot, newSlot);
		if (newSlot == null) {
			StopSlotBGColor();
		}
		else {
			IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(newSlot.GetLastItemUUID());
			if (!Slot.CanPlaceItem(topItem, true)) {
				StopSlotBGColor();
			}
			else {
				LoopSlotBGColor();
			}
		}
	}

	protected void LoopSlotBGColor() {
		slotBG.DOColor(Color.green, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
	}
	
	protected void StopSlotBGColor() {
		slotBG.DOKill();
		slotBG.DOColor(Color.white, 0.2f).SetUpdate(true);
	}
}
