using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public class ModificationPanelSlotLayout : AbstractMikroController<MainGame> 
{
    protected RectTransform slotLayout;
	[SerializeField] private GameObject slotPrefab;
	[SerializeField] private bool allowDrag = false;
	[SerializeField] private bool showDescription = false;
	[SerializeField] private ResourceCategory[] slotAllowedCategories;
	[SerializeField] private bool showEmptySlots = false;
	
	protected List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
	
	protected IInventoryModel inventoryModel;


	protected bool awaked = false;
	
	[SerializeField] private bool isRightSide = false;
	protected ResourceSlot selectedSlot;
	protected Action<ResourceSlotViewController> onSlotClicked;
	protected IInventorySystem inventorySystem;



	protected virtual void Awake() {
		slotLayout = transform.Find("ScrollView/Viewport/InventoryLayoutParent/InventoryLayout").GetComponent<RectTransform>();
		inventoryModel = this.GetModel<IInventoryModel>();
		awaked = true;
		inventorySystem = this.GetSystem<IInventorySystem>();
	}
	
	

	public void ClearSlots() {
		slotViewControllers.ForEach(slot => {
			slot.UnRegisterOnSlotClickedCallback(OnSlotClicked);
			slot.OnInventoryUIClosed();
			
		});
		
		foreach (Transform child in slotLayout) {
			Destroy(child.gameObject);
		}
		
		slotViewControllers.Clear();
		selectedSlot = null;
	}

	public void OnClosePanel() {
		ClearSlots();
	}


	public virtual void OnOpenPanel(List<ResourceSlot> slots) {
		ClearSlots();
		
		if(slots == null)
			return;
		if (!awaked) {
			Awake();
		}


		foreach (ResourceSlot slot in slots) {
			if (slot.GetQuantity() == 0 && !showEmptySlots) {
				continue;
			}

			RectTransform targetLayout = slotLayout;
			GameObject slotObject = Instantiate(slotPrefab, targetLayout);
			slotObject.transform.SetParent(targetLayout);

			ResourceSlotViewController slotViewController = slotObject.GetComponent<ResourceSlotViewController>();
			slotViewController.SetSlot(slot, false);
			slotViewController.AllowDrag = allowDrag;
			slotViewController.SpawnDescription = showDescription;
			slotViewController.AllowedResourceCategories = slotAllowedCategories;
			slotViewControllers.Add(slotViewController);
			slotViewController.Activate(true, isRightSide);

			slotViewController.RegisterOnSlotClickedCallback(OnSlotClicked);
		}
	}

	protected virtual void OnSlotClicked(ResourceSlotViewController vc) {
		if (selectedSlot != vc.Slot) {
			selectedSlot = vc.Slot;
			onSlotClicked?.Invoke(vc);
		}
	}
	
	public void RegisterOnSlotClicked(Action<ResourceSlotViewController> callback) {
		onSlotClicked += callback;
	}
	
	public void UnRegisterOnSlotClicked(Action<ResourceSlotViewController> callback) {
		onSlotClicked -= callback;
	}
	
}
