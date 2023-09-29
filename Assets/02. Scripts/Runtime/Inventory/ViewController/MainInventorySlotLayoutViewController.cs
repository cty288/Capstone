using System;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.Inventory.Model;
using UnityEngine;

namespace Runtime.Inventory.ViewController {
	public class MainInventorySlotLayoutViewController : InventorySlotLayoutViewController {
		
		private RectTransform slotLayout;
		[SerializeField] private GameObject slotPrefab;
		protected List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
		
		private IInventoryModel inventoryModel;
		[SerializeField] private GameObject lockedSlotPrefab;
		private int unlockedSlotCount = 0;

		private bool awaked = false;
		
		[SerializeField] private bool isRightSide = false;
		protected virtual void Awake() {
			slotLayout = transform.Find("InventoryLayout").GetComponent<RectTransform>();
			
			inventoryModel = this.GetModel<IInventoryModel>();
			if (lockedSlotPrefab && !awaked) {
				int lockedSlotCount = InventoryModel.MaxSlotCount;
				for (int i = 0; i < lockedSlotCount; i++) {
					GameObject lockedSlot = Instantiate(lockedSlotPrefab, slotLayout);
					lockedSlot.transform.SetParent(slotLayout);
					lockedSlot.transform.SetAsLastSibling();
				}
			}
			
			awaked = true;
			
		}

		

		public override void OnInventorySlotAdded(List<ResourceSlot> addedSlots, int addedCount) {
			Awake();
			if (lockedSlotPrefab) {
				//first remove addCount locked slots, from the end of the list
				for (int i = 0; i < addedCount; i++) {
					DestroyImmediate(slotLayout.GetChild(slotLayout.childCount - 1).gameObject);
				}
			}
			
			
			int j = 0;
			
			for (int i = 0; i < addedCount; i++) {
				RectTransform targetLayout = slotLayout;

				GameObject slot = Instantiate(slotPrefab, targetLayout);
				slot.transform.SetParent(targetLayout);
				//slot.transform.SetAsLastSibling();
				slot.transform.SetSiblingIndex(unlockedSlotCount);
				unlockedSlotCount++;
				
                
				ResourceSlotViewController slotViewController = slot.GetComponent<ResourceSlotViewController>();
				slotViewController.SetSlot(addedSlots[j++]);
				slotViewControllers.Add(slotViewController);
				OnSlotViewControllerSpawned(slotViewController, i);
			}
		}
		
		public virtual void OnSlotViewControllerSpawned(ResourceSlotViewController slotViewController, int index) {
			
		}
		

		public override void OnShowSlotItem() {
			slotViewControllers.ForEach(slot => slot.Activate(true, isRightSide));
		}

		public override void OnHideSlotItem() {
			slotViewControllers.ForEach(slot => slot.Activate(false, isRightSide));
		}

		public override void OnSelected(int slotIndex) {
			
		}
	}
}