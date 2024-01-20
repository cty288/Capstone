using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using _02._Scripts.Runtime.Skills;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Properties;
using _02._Scripts.Runtime.WeaponParts.Model;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanelViewController : SwitchableSubPanel {
   private  PreparationSlotLayoutViewController buildableResourcePanel;
	private PreparationSlotLayoutViewController materialPanel;
	private GameObject previewPanel;
	//private ISkillModel skillModel;
	private IInventoryModel inventoryModel;
	private SlotResourceDescriptionPanel previewDescriptionPanel;
	private Button purchaseButton;
	private ResourceSlot currentPreviewSlot;
	private Transform previewLayout;
	private ICurrencyModel currencyModel;
	private ICurrencySystem currencySystem;
	//private ISkillSystem skillSystem;
	private IInventorySystem inventorySystem;
	private ResourceCategory category;
	private IResourceBuildModel resourceBuildModel;
	private int selectedRarity = 1;
	private HashSet<IResourceEntity> entitiesToRemoveWhenClear = new HashSet<IResourceEntity>();
	private IBuildableResourceEntity currentPreviewEntity = null;

	[SerializeField] private GameObject requiredResourceDisplayPrefab;
	[SerializeField] private Sprite moneySprite;
	[SerializeField] private TMP_Text previewLevelText;
	[SerializeField] private Button lastRarityButton;
	[SerializeField] private Button nextRarityButton;
	[SerializeField] private GameObject noBuildableResourceHint;
	[SerializeField] private GameObject noResourceHint;
	
	private void Awake() {
		buildableResourcePanel = transform.Find("BuildableResourcePanel").GetComponent<PreparationSlotLayoutViewController>();
		materialPanel = transform.Find("OwnedMaterialPanel").GetComponent<PreparationSlotLayoutViewController>();
		previewPanel = transform.Find("PreviewPanel").gameObject;
		//skillModel = this.GetModel<ISkillModel>();
		resourceBuildModel = this.GetModel<IResourceBuildModel>();
		
		inventoryModel = this.GetModel<IInventoryModel>();
		previewDescriptionPanel = previewPanel.transform.Find("Mask/UpgradeGroup/DescriptionTag")
			.GetComponent<SlotResourceDescriptionPanel>();
		purchaseButton = previewPanel.transform.Find("PurchaseButton").GetComponent<Button>();
		purchaseButton.onClick.AddListener(OnPurchaseClicked);
		
		previewLayout = previewPanel.transform.Find("RequiredResourcesLayout");
		currencyModel = this.GetModel<ICurrencyModel>();
		currencySystem = this.GetSystem<ICurrencySystem>();
		//skillSystem = this.GetSystem<ISkillSystem>();
		inventorySystem = this.GetSystem<IInventorySystem>();
		
		lastRarityButton.onClick.AddListener(OnLastRarityClicked);
		nextRarityButton.onClick.AddListener(OnNextRarityClicked);
	}

	private void OnLastRarityClicked() {
		SetPreviewedRarity(selectedRarity - 1);
	}

	private void OnNextRarityClicked() {
		SetPreviewedRarity(selectedRarity + 1);
	}

	public override void OnSwitchToPanel() {
		base.OnSwitchToPanel();
		Refresh();
	}

	private void Clear() {
		foreach (IResourceEntity resourceEntity in entitiesToRemoveWhenClear) {
			GlobalEntities.GetEntityAndModel(resourceEntity.UUID).Item2.RemoveEntity(resourceEntity.UUID);
		}

		entitiesToRemoveWhenClear.Clear();
		buildableResourcePanel.OnUIClosed();
		materialPanel.OnUIClosed();
		buildableResourcePanel.UnRegisterOnSlotClicked(OnSlotClicked);
		ClearPreview();
		
	}

	private void ClearPreview() {
		previewPanel.SetActive(false);
		previewDescriptionPanel.Clear();
		purchaseButton.interactable = false;
		currentPreviewSlot = null;
		foreach (Transform obj in previewLayout) {
			Destroy(obj.gameObject);
		}
		if (currentPreviewEntity != null) {
			GlobalEntities.GetEntityAndModel(currentPreviewEntity.UUID).Item2.RemoveEntity(currentPreviewEntity.UUID);
			currentPreviewEntity = null;
		}
	}
	
	private void Refresh() {
		Clear();
		var materials = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);
		
		noBuildableResourceHint.SetActive(false);
		noResourceHint.SetActive(materials == null || materials.Count == 0);
		materialPanel.OnShowItems(materials);
		HashSet<string> buildableResourceNames = resourceBuildModel.GetBuildableResources(category);
		if (buildableResourceNames == null || buildableResourceNames.Count == 0) {
			noBuildableResourceHint.SetActive(true);
			return;
		}

		HashSet<PreparationSlot> buildableSlots = new HashSet<PreparationSlot>();
		foreach (string resourceName in buildableResourceNames) {
			IResourceEntity entity = ResourceTemplates.Singleton.GetResourceTemplates(resourceName).EntityCreater
				.Invoke(false, 1);
			
			entitiesToRemoveWhenClear.Add(entity);
			PreparationSlot slot = new PreparationSlot();
			slot.TryAddItem(entity);
			buildableSlots.Add(slot);
		}

		buildableResourcePanel.OnShowItems(buildableSlots);
		
		buildableResourcePanel.RegisterOnSlotClicked(OnSlotClicked);
	}

	private void OnSlotClicked(ResourceSlotViewController slotVC, PreparationSlotLayoutViewController layout, bool isSelectedAlready) {
		currentPreviewSlot = slotVC.Slot;
		SetPreviewedRarity(1);
	}

	private void SetPreviewedRarity(int rarity) {
		var slot = currentPreviewSlot;
		ClearPreview();
		if(slot == null) return;
		
	
		
		currentPreviewSlot = slot;
		previewPanel.SetActive(true);
		
		IBuildableResourceEntity templateEntity =
			GlobalGameResourceEntities.GetAnyResource(currentPreviewSlot.GetLastItemUUID()) as IBuildableResourceEntity;
		
		if (templateEntity == null) {
			return;
		}
		
		int maxRarity = templateEntity.GetMaxRarity();
		if (rarity > maxRarity) {
			rarity = 1;
		}else if (rarity < 1) {
			rarity = maxRarity;
		}

		selectedRarity = rarity;

		currentPreviewEntity = ResourceTemplates.Singleton.GetResourceTemplates(templateEntity.EntityName)
			.EntityCreater
			.Invoke(true, rarity) as IBuildableResourceEntity;
		
		if (currentPreviewEntity == null) {
			return;
		}

		Dictionary<CurrencyType, int> skillUseCost = null;
		if (currentPreviewEntity is ISkillEntity skillEntity) {
			skillUseCost = skillEntity.GetSkillUseCostOfCurrentLevel();
		}

		previewDescriptionPanel.SetContent(currentPreviewEntity.GetDisplayName(), currentPreviewEntity.GetDescription(),
			InventorySpriteFactory.Singleton.GetSprite(currentPreviewEntity.EntityName), true, rarity,
			ResourceVCFactory.GetLocalizedResourceCategory(currentPreviewEntity.GetResourceCategory()),
			currentPreviewEntity.GetResourcePropertyDescriptions(), skillUseCost);

		PurchaseCostInfo costInfo = templateEntity.GetPurchaseCost();
		HashSet<PreparationSlot> ownedResources = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);


		previewLevelText.text = Localization.GetFormat("BUILD_PREVIEW_LEVEL_TEXT", rarity);

		bool isAllEnough = CheckIsAllEnough(costInfo, ownedResources);
		
		if (isAllEnough) {
			purchaseButton.interactable = true;
		}
		else {
			purchaseButton.interactable = false;
		}
	}
	
	private bool CheckIsAllEnough(PurchaseCostInfo costInfo, HashSet<PreparationSlot> ownedResources) {
		bool isAllEnough = true;

		if (costInfo != null) {
			SpawnRequiredResourceDisplay(moneySprite, costInfo.MoneyCost, costInfo.MoneyCost <= currencyModel.Money.Value);
			if (costInfo.MoneyCost > currencyModel.Money.Value) {
				isAllEnough = false;
			}


			foreach (KeyValuePair<string,int> info in costInfo.ResourceCost) {
				Sprite sprite = InventorySpriteFactory.Singleton.GetSprite(info.Key);
				int ownedCount = GetTotalResourceCountInSlots(ownedResources, info.Key);
				bool isEnough = ownedCount >= info.Value;
				SpawnRequiredResourceDisplay(sprite, info.Value, isEnough);
				if (!isEnough) {
					isAllEnough = false;
				}
			}
			
			StartCoroutine(RebuildLayout(previewLayout.GetComponent<RectTransform>()));
			
		}
		
		return isAllEnough;
	}
	
	
	private GameObject SpawnRequiredResourceDisplay(Sprite sprite, int count, bool isEnough) {
		GameObject obj = Instantiate(requiredResourceDisplayPrefab, previewLayout);
		obj.transform.localScale = Vector3.one;
		obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
		obj.transform.Find("Text").GetComponent<TMP_Text>().text = count.ToString();
		obj.transform.Find("Text").GetComponent<TMP_Text>().color = isEnough ? Color.green : Color.red;
		StartCoroutine(RebuildLayout(obj.GetComponent<RectTransform>()));
		return obj;
	}
	
	private IEnumerator RebuildLayout(RectTransform rectTransform) {
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
	}

	private int GetTotalResourceCountInSlots(HashSet<PreparationSlot> slots, string resourceName) {
		int count = 0;
		if (slots == null) return count;
		
		foreach (PreparationSlot slot in slots) {
			string itemUUID = slot.GetLastItemUUID();
			if (itemUUID == null || slot.IsEmpty()) {
				continue;
			}
			IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(itemUUID) as IResourceEntity;
			if (entity.EntityName == resourceName) {
				count = slot.GetQuantity();
				break;
			}
		}

		return count;
	}

	
	private void OnPurchaseClicked() {
		if (currentPreviewSlot == null) {
			return;
		}
		IBuildableResourceEntity entity =
			GlobalGameResourceEntities.GetAnyResource(currentPreviewSlot.GetLastItemUUID()) as IBuildableResourceEntity;
		if (entity == null) {
			return;
		}
		
		PurchaseCostInfo costInfo = entity.GetPurchaseCost();
		HashSet<PreparationSlot> ownedResources = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);
		bool isAllEnough = CheckIsAllEnough(costInfo, ownedResources);

		if (isAllEnough) {
			currencySystem.RemoveMoney(costInfo.MoneyCost);

			foreach (KeyValuePair<string,int> valuePair in costInfo.ResourceCost) {
				string resourceName = valuePair.Key;
				int count = valuePair.Value;
				inventorySystem.RemoveResourceEntityFromBaseStock(ResourceCategory.RawMaterial, resourceName, count,
					true);
			}

			resourceBuildModel.RemoveFromBuild(category, entity.EntityName);

			if (entity.GetResourceCategory() is ResourceCategory.Skill or ResourceCategory.Weapon) {
				inventoryModel.AddToBaseStock(entity);
				entitiesToRemoveWhenClear.Remove(entity);
				
			}else if (entity.GetResourceCategory() is ResourceCategory.WeaponParts) {
				IWeaponPartsModel weaponPartsModel = this.GetModel<IWeaponPartsModel>();
				weaponPartsModel.AddToUnlockedParts(entity.EntityName);
			}
			
			
			//skillModel.GetPurchasableSkillSlots()
		}
		Refresh();

	}


	public override void OnSwitchToOtherPanel() {
		base.OnSwitchToOtherPanel();
		Clear();
	}

	public void OnSetResourceCategory(ResourceCategory msgCategory) {
		category = msgCategory;
	}
}
