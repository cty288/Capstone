using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Systems;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public class WeaponPartsTradingPanel  : AbstractPanelContainer, IController, IGameUIPanel {
	//TODO: after successfully purchase, set lastSelectedSlot to null
	//TODO: after successfully upgrade, refresh upgrade panel and preview panel
	
	[SerializeField]
	private BasicSlotLayoutViewController upgradeSlotLayout;
	[SerializeField]
	private BasicSlotLayoutViewController exchangeSlotLayout;
	private IInventoryModel inventoryModel;
	private IWeaponPartsSystem weaponPartsSystem;

	private BindableProperty<ResourceSlotViewController> currentlySelectedSlot =
		new BindableProperty<ResourceSlotViewController>(null); //force update after upgrade

	private IWeaponPartsEntity currentPreviewingWeaponPartsEntity = null;
	[SerializeField]
	private bool isExchange = true;
	
	public override void OnInit() {
		inventoryModel = this.GetModel<IInventoryModel>();
		weaponPartsSystem = this.GetSystem<IWeaponPartsSystem>();
		currentlySelectedSlot.RegisterOnValueChanged(OnLastSelectedSlotChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnLastSelectedSlotChanged(ResourceSlotViewController previousSlot, ResourceSlotViewController currentSlot)
	{
		//remove current previewing weapon parts
		if (currentPreviewingWeaponPartsEntity != null) {
			GlobalEntities.GetEntityAndModel(currentPreviewingWeaponPartsEntity.UUID).Item2
				.RemoveEntity(currentPreviewingWeaponPartsEntity.UUID);
			currentPreviewingWeaponPartsEntity = null;
		}

		if (previousSlot) {
			previousSlot.GetComponent<PreparationSlotViewController>().SetTicked(false);
		}

		if (currentSlot) { 
			currentSlot.GetComponent<PreparationSlotViewController>().SetTicked(true);
		}
		
		
	}

	public override void OnOpen(UIMsg msg) {
		upgradeSlotLayout.OnShowItems(GetOwnedWeaponParts());
		exchangeSlotLayout.OnShowItems(GetPurchaseableWeaponParts());
		
		upgradeSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
		exchangeSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
	}

	private HashSet<ResourceSlot> GetOwnedWeaponParts() {
		HashSet<ResourceSlot> slots = new HashSet<ResourceSlot>();
		slots.UnionWith(inventoryModel.GetAllSlots((slot => {
			string uuid = slot.GetLastItemUUID();
			if (uuid != null) {
				IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid);
				if (entity is IWeaponPartsEntity) {
					return true;
				}
			}
			return false;
		})));
		
		
		foreach (string allItemUuiD in inventoryModel.GetAllItemUUIDs()) {
			IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(allItemUuiD);
			if (entity is IWeaponEntity weaponEntity) {
				foreach (var weaponPartType in Enum.GetValues(typeof(WeaponPartType))) {
					var weaponPartsSlots = weaponEntity.GetWeaponPartsSlots((WeaponPartType) weaponPartType);
					slots.UnionWith(weaponPartsSlots);
				}
			}
		}
		return slots;
	}
	
	private HashSet<ResourceSlot> GetPurchaseableWeaponParts() {
		HashSet<ResourceSlot> slots = new HashSet<ResourceSlot>();
		var parts = weaponPartsSystem.GetCurrentLevelPurchaseableParts();
		foreach (var part in parts) {
			ResourceSlot slot = new ResourceSlot();
			if (slot.TryAddItem(part)) {
				slots.Add(slot);
			}
		}
		return slots;
	}



	
	public override void OnClosed() {
		upgradeSlotLayout.UnRegisterOnSlotClicked(OnSlotClicked);
		exchangeSlotLayout.UnRegisterOnSlotClicked(OnSlotClicked);
		currentlySelectedSlot.Value = null;
		upgradeSlotLayout.OnUIClosed();
		exchangeSlotLayout.OnUIClosed();
		isExchange = true;
	}

	private void OnSlotClicked(ResourceSlotViewController slot,
		BasicSlotLayoutViewController slotLayout, bool originallySelected) {
		if (slot == currentlySelectedSlot.Value && originallySelected) {
			currentlySelectedSlot.Value = null;
		}else if (slot != currentlySelectedSlot.Value) {
			currentlySelectedSlot.Value = slot;
		}

		if (currentlySelectedSlot.Value != null) {
			isExchange = slotLayout == exchangeSlotLayout;
		}
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}

	public IPanel GetClosePanel() {
		IPanel openedChild = GetTopChild();
		if (openedChild != null) {
			return openedChild;
		}
            
		return this;
	}
}
