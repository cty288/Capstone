using System;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Runtime.Inventory.Model;
using UnityEngine;

namespace Runtime.Inventory.ViewController {
	public abstract class InventorySlotLayoutViewController : AbstractMikroController<MainGame> {
		[SerializeField] public bool ShowSlotItemWhenInventoryUIClosed = false;
		[SerializeField] public HotBarCategory HotBarCategory;
		
		public abstract void OnInventorySlotAdded(List<ResourceSlot> addedSlots, int addedCount);
		
		
		public abstract void OnShowSlotItem();
		
		public abstract void OnHideSlotItem();

		public abstract void OnSelected(int slotIndex);
	}
}