using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using UnityEngine;

public class BasicSlotLayoutViewController : AbstractMikroController<MainGame>
{
		protected RectTransform slotLayout;
		[SerializeField] private GameObject slotPrefab;
		protected List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
		
		private IInventoryModel inventoryModel;
	

		private bool awaked = false;
		
		[SerializeField] private bool isRightSide = false;
		
		
		private Action<ResourceSlotViewController, BasicSlotLayoutViewController, bool> onSlotClicked;
		private HashSet<ResourceSlotViewController> selectedSlots = new HashSet<ResourceSlotViewController>();
		
		
		protected virtual void Awake() {
			slotLayout = transform.Find("ScrollView/Viewport/InventoryLayoutParent/InventoryLayout").GetComponent<RectTransform>();
			inventoryModel = this.GetModel<IInventoryModel>();
			awaked = true;
			
		}

		public List<PreparationSlot> OnUIClosed() {
			slotViewControllers.ForEach(slot => {
				slot.UnRegisterOnSlotClickedCallback(OnSlotClicked);
				slot.OnInventoryUIClosed();
				
			});
			

			List<PreparationSlot> slots = new List<PreparationSlot>();
			foreach (ResourceSlotViewController slotViewController in selectedSlots) {
				slots.Add(slotViewController.Slot as PreparationSlot);
			}
			selectedSlots.Clear();

			//Destroy all slots
			foreach (Transform child in slotLayout) {
				Destroy(child.gameObject);
			}
			
			slotViewControllers.Clear();
			
			return slots;
		}

		public void OnShowItems(HashSet<ResourceSlot> slots, bool allowDrag = false) {
			if(slots == null)
				return;
			
			OnShowItems(slots.ToList(), allowDrag);
		}

		public void OnShowItems(List<ResourceSlot> slots, bool allowDrag = false) {
			if(slots == null)
				return;
			
			Awake();
			foreach (ResourceSlot slot in slots) {
				if (slot.GetQuantity() == 0) {
					continue;
				}
				RectTransform targetLayout = slotLayout;
				GameObject slotObject = Instantiate(slotPrefab, targetLayout);
				slotObject.transform.SetParent(targetLayout);

				ResourceSlotViewController slotViewController = slotObject.GetComponent<ResourceSlotViewController>();
				slotViewController.SetSlot(slot, false);
				slotViewController.AllowDrag = allowDrag;
				slotViewControllers.Add(slotViewController);
				slotViewController.Activate(true, isRightSide);

				slotViewController.RegisterOnSlotClickedCallback(OnSlotClicked);
			}
		}

		private void OnSlotClicked(ResourceSlotViewController vc) {
			onSlotClicked?.Invoke(vc, this, selectedSlots.Contains(vc));
		}
		
		public void RegisterOnSlotClicked(Action<ResourceSlotViewController,BasicSlotLayoutViewController, bool> callback) {
			onSlotClicked += callback;
		}
		
		public void UnRegisterOnSlotClicked(Action<ResourceSlotViewController, BasicSlotLayoutViewController, bool> callback) {
			onSlotClicked -= callback;
		}
		
		public void SetSelected(ResourceSlotViewController slotViewController, bool selected) {
			if (selected) {
				selectedSlots.Add(slotViewController);
			}
			else {
				selectedSlots.Remove(slotViewController);
			}

			slotViewController.GetComponent<PreparationSlotViewController>().SetTicked(selected);
		}
		
		public HashSet<ResourceSlotViewController> GetAllSlots() {
			return new HashSet<ResourceSlotViewController>(slotViewControllers);
		}


}
