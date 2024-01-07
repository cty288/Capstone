using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;

public class ModificationWeaponPartsSlotLayout : AbstractMikroController<MainGame> {
	private TMP_Text title;
	
	
	[SerializeField] private GameObject slotPrefab;
	[SerializeField] private bool allowDrag = true;
	[SerializeField] private bool showDescription = true;
	
	protected List<ResourceSlotViewController> slotViewControllers = new List<ResourceSlotViewController>();
	protected IInventoryModel inventoryModel;
	protected bool awaked = false;
	
	[SerializeField] private bool isRightSide = true;
	protected IInventorySystem inventorySystem;
	
	private void Awake() {
		title = transform.Find("Title").GetComponent<TMP_Text>();
		inventoryModel = this.GetModel<IInventoryModel>();
		awaked = true;
		inventorySystem = this.GetSystem<IInventorySystem>();
	}

	public void Init(HashSet<WeaponPartsSlot> slots, WeaponPartType type) {
		ClearSlots();
		if(slots == null)
			return;
		
		if (!awaked) {
			Awake();
		}
		
		foreach (WeaponPartsSlot slot in slots) {

			GameObject slotObject = Instantiate(slotPrefab, transform);
			slotObject.transform.SetParent(transform);
			slotObject.transform.SetAsLastSibling();

			ResourceSlotViewController slotViewController = slotObject.GetComponent<ResourceSlotViewController>();
			slotViewController.SetSlot(slot, false);
			slotViewController.AllowDrag = allowDrag;
			slotViewController.SpawnDescription = showDescription;
			
			slotViewControllers.Add(slotViewController);
			slotViewController.Activate(true, isRightSide);
		}
		
		title.text = Localization.Get($"NAME_{type.ToString()}");
		
	}
	
	public void ClearSlots() {
		foreach (Transform child in transform) {
			Destroy(child.gameObject);
		}
		slotViewControllers.Clear();
	}
}
