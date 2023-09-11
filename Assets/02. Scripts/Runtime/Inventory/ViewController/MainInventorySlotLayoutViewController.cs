using System;
using System.Collections.Generic;
using Runtime.Inventory.Model;
using UnityEngine;

namespace Runtime.Inventory.ViewController {
	public class MainInventorySlotLayoutViewController : InventorySlotLayoutViewController {
		
		private RectTransform slotLayout;
		[SerializeField] private GameObject slotPrefab;
		private List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
		protected virtual void Awake() {
			slotLayout = GetComponent<RectTransform>();
		}

		public override void OnInventorySlotAdded(List<ResourceSlot> addedSlots, int addedCount) {
			
			int j = 0;
			for (int i = 0; i < addedCount; i++) {
				RectTransform targetLayout = slotLayout;

				GameObject slot = Instantiate(slotPrefab, targetLayout);
				slot.transform.SetParent(targetLayout);
				slot.transform.SetAsLastSibling();
                
				ResourceSlotViewController slotViewController = slot.GetComponent<ResourceSlotViewController>();
				slotViewController.SetSlot(addedSlots[j++]);
				slotViewControllers.Add(slotViewController);
			}
		}
		

		public override void OnShowSlotItem() {
			slotViewControllers.ForEach(slot => slot.Activate(true));
		}

		public override void OnHideSlotItem() {
			slotViewControllers.ForEach(slot => slot.Activate(false));
		}
	}
}