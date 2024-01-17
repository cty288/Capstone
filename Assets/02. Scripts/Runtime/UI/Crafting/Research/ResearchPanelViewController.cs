using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using UnityEngine;

public class ResearchPanelViewController : SwitchableSubPanel {
	[SerializeField] private ResearchPanelSlotLayoutViewController ownedSlotLayoutViewController;
	[SerializeField] private ResearchPanelSlotLayoutViewController selectedSlotLayoutViewController;
	private ResourceCategory category;
	private IInventoryModel inventoryModel;
	
	private void Awake() {
		inventoryModel = this.GetModel<IInventoryModel>();
	}


	public override void OnSwitchToPanel() {
		base.OnSwitchToPanel();
		InitPanel();
	}


	public override void OnSwitchToOtherPanel() {
		base.OnSwitchToOtherPanel();
		ownedSlotLayoutViewController.OnUIClosed();
		selectedSlotLayoutViewController.OnUIClosed();
		//TODO: move selected back to owned
	}


	private void InitPanel() {
		List<ResourceSlot> resourceSlots  = new List<ResourceSlot>();
		var rawMaterials = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);
		if (rawMaterials != null) {
			foreach (var rawMaterial in rawMaterials) {
				resourceSlots.Add(rawMaterial);
			}
		}
		


		ownedSlotLayoutViewController.OnShowItems(resourceSlots);


		List<ResourceSlot> selectedSlots = new List<ResourceSlot>();
		int slotCount = resourceSlots.Count;
		
		for (int i = 0; i < slotCount; i++) {
			selectedSlots.Add(new PreparationSlot());
		}
		
		selectedSlotLayoutViewController.OnShowItems(selectedSlots);
	}
	
	public void OnSetResourceCategory(ResourceCategory category) {
		this.category = category;
		
	}
}
