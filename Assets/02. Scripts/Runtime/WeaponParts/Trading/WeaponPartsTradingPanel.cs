using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Systems;
using _02._Scripts.Runtime.WeaponParts.Trading;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.UI;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPartsTradingPanel  : AbstractPanelContainer, IController, IGameUIPanel {
	//TODO: after successfully purchase, set lastSelectedSlot to null
	//TODO: after successfully upgrade, refresh upgrade panel and preview panel (force reselect current slot)
	//TODO: after successfully purchase, refresh exchange panel and purchase panel, and preview panel (everything)
	
	[SerializeField]
	private BasicSlotLayoutViewController upgradeSlotLayout;
	[SerializeField]
	private BasicSlotLayoutViewController exchangeSlotLayout;
	[SerializeField] private Transform exchangeSlotSpawnParent;
	[SerializeField] private Transform previewPanelSpawnParent;
	[SerializeField] private GameObject fullyUpgradedText;
	[SerializeField] private WeaponPartsTradingPreviewPanel previewPanel;
	[SerializeField] private Button purchaseButton;
	[SerializeField] private GameObject fullInventoryHint;

	private IInventoryModel inventoryModel;
	private IWeaponPartsSystem weaponPartsSystem;
	private BindableProperty<ResourceSlotViewController> currentlySelectedSlot =
		new BindableProperty<ResourceSlotViewController>(null); //force update after upgrade

	private IWeaponPartsEntity currentPreviewingWeaponPartsEntity = null;
	[SerializeField]
	private bool isExchange = true;

	[SerializeField] private GameObject emptySlotPrefab;
	
	private ICurrencySystem currencySystem;
	private ICurrencyModel currencyModel;
	private IInventorySystem inventorySystem;
	public override void OnInit() {
		inventoryModel = this.GetModel<IInventoryModel>();
		currencyModel = this.GetModel<ICurrencyModel>();
		weaponPartsSystem = this.GetSystem<IWeaponPartsSystem>();
		currencySystem = this.GetSystem<ICurrencySystem>();
		inventorySystem = this.GetSystem<IInventorySystem>();
		currentlySelectedSlot.RegisterOnValueChanged(OnLastSelectedSlotChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		
		purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
		this.RegisterEvent<OnTradeWeaponPart>(OnTradeWeaponPart).UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnTradeWeaponPart(OnTradeWeaponPart e) {
		if (e.IsExchange) { 
			Refresh();
		}else {
			currentlySelectedSlot.SetValueAndForceNotify(currentlySelectedSlot.Value);
			WeaponPartsTradingAllocatePanel panel = UIManager.Singleton.GetPanel<WeaponPartsTradingAllocatePanel>(true);
			panel.OnRefresh(e.TradedPart, e.PreviewedPart, e.IsExchange);
		}
	}

	private void OnPurchaseButtonClicked() {
		if(!currentlySelectedSlot.Value || currentlySelectedSlot.Value.Slot == null || currentPreviewingWeaponPartsEntity == null) return;
		string uuid = currentlySelectedSlot.Value.Slot.GetLastItemUUID();
		IWeaponPartsEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid) as IWeaponPartsEntity;
		if (entity == null) return;

		WeaponPartsTradingAllocatePanel allocatePanel = MainUI.Singleton.Open<WeaponPartsTradingAllocatePanel>(this, null);
		allocatePanel.OnRefresh(entity, currentPreviewingWeaponPartsEntity, isExchange);
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

		if (!currentSlot) { 
			Refresh();
			return;
		}
		currentSlot.GetComponent<PreparationSlotViewController>().SetTicked(true);

		string uuid = currentSlot.Slot?.GetLastItemUUID();
		IWeaponPartsEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid) as IWeaponPartsEntity;
		if (entity == null) return;
		
		fullInventoryHint.gameObject.SetActive(false);
		fullyUpgradedText.gameObject.SetActive(false);
		previewPanelSpawnParent.gameObject.SetActive(true);
		
		if (entity.GetRarity() == entity.GetMaxRarity() && !isExchange) {
			fullyUpgradedText.gameObject.SetActive(true);
			previewPanelSpawnParent.gameObject.SetActive(false);
			return;
		}

		if (isExchange && !inventorySystem.CanPlaceItem(entity)) {
			fullInventoryHint.gameObject.SetActive(true);
			previewPanelSpawnParent.gameObject.SetActive(false);
			return;
		}

		int targetRarity = isExchange ? entity.GetRarity() : entity.GetRarity() + 1;
		ResourceTemplateInfo templateInfo = ResourceTemplates.Singleton.GetResourceTemplates(entity.EntityName);
		IWeaponPartsEntity previewEntity = templateInfo.EntityCreater.Invoke(true, targetRarity) as IWeaponPartsEntity;
		currentPreviewingWeaponPartsEntity = previewEntity;
		previewPanel.ShowPreviewPanel(previewEntity, isExchange, true);
	}




	public override void OnOpen(UIMsg msg) {
		Refresh();
		
		upgradeSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
		exchangeSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
	}

	private void Refresh() {
		fullInventoryHint.gameObject.SetActive(false);
		currentlySelectedSlot.Value = null;
		previewPanel.ResetPreviewPanel();
		
		
		upgradeSlotLayout.ClearLayout();
		exchangeSlotLayout.ClearLayout();
		
		upgradeSlotLayout.OnShowItems(GetOwnedWeaponParts());
		
		HashSet<ResourceSlot> purchaseableWeaponParts = GetPurchaseableWeaponParts();
		exchangeSlotLayout.OnShowItems(purchaseableWeaponParts);
		if (purchaseableWeaponParts.Count < 5) {
			//add empty slots
			for (int i = 0; i < 5 - purchaseableWeaponParts.Count; i++) {
				GameObject emptySlot = Instantiate(emptySlotPrefab, exchangeSlotSpawnParent.transform);
				emptySlot.transform.SetAsLastSibling();
			}
		}
		
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
		upgradeSlotLayout.ClearLayout();
		exchangeSlotLayout.ClearLayout();
		isExchange = true;
	}

	private void OnSlotClicked(ResourceSlotViewController slot,
		BasicSlotLayoutViewController slotLayout, bool originallySelected) {
		if (slot != null && !originallySelected) {
			isExchange = slotLayout == exchangeSlotLayout;
		}
		
		
		if (slot == currentlySelectedSlot.Value && originallySelected) {
			currentlySelectedSlot.Value = null;
		}else if (slot != currentlySelectedSlot.Value) {
			currentlySelectedSlot.Value = slot;
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
