using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using MikroFramework.Architecture;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using UnityEngine;

public class ResearchPanelSlotLayoutViewController : AbstractMikroController<MainGame>
{
	protected RectTransform slotLayout;
	[SerializeField] private GameObject slotPrefab;
	protected List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
	
	private IInventoryModel inventoryModel;


	private bool awaked = false;
	
	[SerializeField] private bool isRightSide = false;
	
	
	private Action<ResourceSlotViewController> onSlotClicked;

	protected virtual void Awake() {
		slotLayout = transform.Find("ScrollView/Viewport/InventoryLayoutParent/InventoryLayout").GetComponent<RectTransform>();
		inventoryModel = this.GetModel<IInventoryModel>();
		awaked = true;
		
	}

	public void OnUIClosed() {
		slotViewControllers.ForEach(slot => {
			slot.UnRegisterOnSlotClickedCallback(OnSlotClicked);
			slot.OnInventoryUIClosed();
			
		});

		foreach (Transform child in slotLayout) {
			Destroy(child.gameObject);
		}
		
		slotViewControllers.Clear();
	}


	public void OnShowItems(List<ResourceSlot> slots) {
		if(slots == null)
			return;
		
		Awake();
		foreach (ResourceSlot slot in slots) {
			RectTransform targetLayout = slotLayout;
			GameObject slotObject = Instantiate(slotPrefab, targetLayout);
			slotObject.transform.SetParent(targetLayout);

			ResourceSlotViewController slotViewController = slotObject.GetComponent<ResourceSlotViewController>();
			slotViewController.SetSlot(slot, false);
			slotViewController.AllowDrag = false;
			slotViewControllers.Add(slotViewController);
			slotViewController.Activate(true, isRightSide);

			slotViewController.RegisterOnSlotClickedCallback(OnSlotClicked);
		}
	}

	private void OnSlotClicked(ResourceSlotViewController vc) {
		onSlotClicked?.Invoke(vc);
	}
	
	public void RegisterOnSlotClicked(Action<ResourceSlotViewController> callback) {
		onSlotClicked += callback;
	}
	
	public void UnRegisterOnSlotClicked(Action<ResourceSlotViewController> callback) {
		onSlotClicked -= callback;
	}
}
