using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Systems;
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
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPartsTradingPanel  : AbstractPanelContainer, IController, IGameUIPanel {
	//TODO: after successfully purchase, set lastSelectedSlot to null
	//TODO: after successfully upgrade, refresh upgrade panel and preview panel
	//TODO: after successfully purchase, refresh exchange panel and purchase panel
	
	[SerializeField]
	private BasicSlotLayoutViewController upgradeSlotLayout;
	[SerializeField]
	private BasicSlotLayoutViewController exchangeSlotLayout;
	[SerializeField] private Transform exchangeSlotSpawnParent;
	[SerializeField] private Transform previewPanelSpawnParent;
	
	[Header("Preview Panel")]
	[SerializeField] private RectTransform rarityBar;
	[SerializeField] private GameObject rarityIconPrefab;
	[SerializeField] private GameObject nextLevelText;
	[SerializeField] private Image itemIconImage;
	[SerializeField] private TMP_Text itemNameText;
	[SerializeField] private TMP_Text descriptionText;
	[SerializeField] private TMP_Text itemCostText;
	[SerializeField] private Button purchaseButton;
	[SerializeField] private TMP_Text purchaseButtonText;
	[SerializeField] private GameObject fullyUpgradedText;
	
	
	
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
	public override void OnInit() {
		inventoryModel = this.GetModel<IInventoryModel>();
		currencyModel = this.GetModel<ICurrencyModel>();
		weaponPartsSystem = this.GetSystem<IWeaponPartsSystem>();
		currencySystem = this.GetSystem<ICurrencySystem>();
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

		if (!currentSlot) { 
			Refresh();
			return;
		}
		currentSlot.GetComponent<PreparationSlotViewController>().SetTicked(true);

		string uuid = currentSlot.Slot?.GetLastItemUUID();
		IWeaponPartsEntity entity = GlobalGameResourceEntities.GetAnyResource(uuid) as IWeaponPartsEntity;
		if (entity == null) return;

		if (entity.GetRarity() == entity.GetMaxRarity() && !isExchange) {
			fullyUpgradedText.gameObject.SetActive(true);
			previewPanelSpawnParent.gameObject.SetActive(false);
			return;
		}
		
		int targetRarity = isExchange ? entity.GetRarity() : entity.GetRarity() + 1;
		ResourceTemplateInfo templateInfo = ResourceTemplates.Singleton.GetResourceTemplates(entity.EntityName);
		IWeaponPartsEntity previewEntity = templateInfo.EntityCreater.Invoke(true, targetRarity) as IWeaponPartsEntity;
		ShowPreviewPanel(previewEntity, isExchange);
	}




	public override void OnOpen(UIMsg msg) {
		Refresh();
		
		upgradeSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
		exchangeSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
	}

	private void Refresh() {
		ResetPreviewPanel();
		
		
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

	private void ResetPreviewPanel() {
		previewPanelSpawnParent.gameObject.SetActive(false);
		fullyUpgradedText.gameObject.SetActive(false);
		foreach (Transform child in rarityBar) {
			if (child.GetSiblingIndex() == 0) continue;
			Destroy(child.gameObject);
		}
	}
	
	private void ShowPreviewPanel(IWeaponPartsEntity entity, bool isExchange) {
		ResetPreviewPanel();
		previewPanelSpawnParent.gameObject.SetActive(true);
		nextLevelText.gameObject.SetActive(!isExchange);
		currentPreviewingWeaponPartsEntity = entity;

		itemNameText.text = entity.GetDisplayName();
		
		//rarity bar
		float height = rarityBar.rect.height;
		for (int i = 0; i < entity.GetRarity(); i++) {
			GameObject star = Instantiate(rarityIconPrefab, rarityBar);
			RarityIndicator rarityIndicator = star.GetComponent<RarityIndicator>();
			rarityIndicator.SetCurrency(entity.GetBuildType());
			RectTransform starRect = star.GetComponent<RectTransform>();
			starRect.sizeDelta = new Vector2(height, height);
		}

		itemIconImage.sprite = InventorySpriteFactory.Singleton.GetSprite(entity);
		descriptionText.text = entity.GetDescription();

		int cost = this.isExchange
			? entity.GetInGamePurchaseCostOfLevel(entity.GetRarity())
			: entity.GetUpgradeCostOfLevel(entity.GetRarity());

		int totalMoney = 0;
		foreach (var val in Enum.GetValues(typeof(CurrencyType))) {
			totalMoney += currencyModel.GetCurrencyAmountProperty((CurrencyType) val);
		}
		bool canPurchase = totalMoney >= cost;
		string color = canPurchase ? "<color=#00C612>" : "<color=red>";

		itemCostText.text = Localization.GetFormat("PARTS_PREVIEW_COST", $"{color}{cost}</color>");
		purchaseButton.gameObject.SetActive(canPurchase);
		purchaseButtonText.text = isExchange
			? Localization.Get("PARTS_TRADING_TITLE_EXCHANGE")
			: Localization.Get("PARTS_TRADING_TITLE_UPGRADE");
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
