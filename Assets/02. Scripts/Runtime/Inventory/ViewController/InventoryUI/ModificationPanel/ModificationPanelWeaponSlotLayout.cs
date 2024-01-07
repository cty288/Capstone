using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public class ModificationPanelWeaponSlotLayout  : ModificationPanelSlotLayout{
	
	public override void OnOpenPanel(List<ResourceSlot> slots) {
		base.OnOpenPanel(slots);

		bool selectedInitialWeapon = false;
		IResourceEntity currentWeapon = inventorySystem.GetCurrentlySelectedEntity();
		if (currentWeapon is IWeaponEntity) {
			foreach (ResourceSlotViewController slotViewController in slotViewControllers) {
				string uuid = slotViewController.Slot.GetLastItemUUID();
				if (currentWeapon.UUID == uuid) {
					OnSlotClicked(slotViewController);
					selectedInitialWeapon = true;
					break;
				}
			}

		}

		if (!selectedInitialWeapon) {
			OnSlotClicked(slotViewControllers[0]);
		}

	}

	protected override void OnSlotClicked(ResourceSlotViewController vc) {
		base.OnSlotClicked(vc);
		foreach (ResourceSlotViewController slotViewController in slotViewControllers) {
			slotViewController.gameObject.GetComponent<PreparationSlotViewController>()
				.SetTicked(vc == slotViewController);
		}
	}
}
