using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using _02._Scripts.Runtime.Skills;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Properties;
using _02._Scripts.Runtime.WeaponParts.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Inventory;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class BuildPanelViewController : SwitchableSubPanel {
   private  PreparationSlotLayoutViewController buildableResourcePanel;
	private PreparationSlotLayoutViewController materialPanel;
	private GameObject previewPanel;
	//private ISkillModel skillModel;
	private IInventoryModel inventoryModel;
	//private SlotResourceDescriptionPanel previewDescriptionPanel;
	private Button purchaseButton;
	private ResourceSlot currentPreviewSlot;
	
	private ICurrencyModel currencyModel;
	private ICurrencySystem currencySystem;
	//private ISkillSystem skillSystem;
	private IInventorySystem inventorySystem;
	private ResearchCategory category;
	private IResourceBuildModel resourceBuildModel;
	private int selectedRarity = 1;
	private HashSet<IResourceEntity> entitiesToRemoveWhenClear = new HashSet<IResourceEntity>();
	private IBuildableResourceEntity currentPreviewEntity = null;

	[SerializeField] private GameObject requiredResourceDisplayPrefab;
	[SerializeField] private Sprite moneySprite;

	[SerializeField] private RectTransform previewRarityBar;
	[SerializeField] private GameObject rarityStarFilled;
	[SerializeField] private GameObject rarityStarEmpty;
	
	//[SerializeField] private TMP_Text previewLevelText;
	[SerializeField] private Button lastRarityButton;
	[SerializeField] private Button nextRarityButton;
	[SerializeField] private GameObject noBuildableResourceHint;
	[SerializeField] private GameObject noResourceHint;
	[SerializeField] private GameObject previewSelectionObject;

	[Header("Description Panel")] 
	[SerializeField] protected RectTransform detailPanel;
	[SerializeField] private Image descriptionIcon;
	[SerializeField] private TMP_Text itemNameText;
	[SerializeField] private TMP_Text descriptionText;
	[SerializeField] protected GameObject useCostPanel;
	[SerializeField] protected GameObject useCostElementSpawnParent;
	[SerializeField] protected GameObject useCostElementPrefab;
	[SerializeField] protected GameObject propertyDescriptionItemPrefab;
	[SerializeField] protected RectTransform propertyDescriptionItemParent;
	[SerializeField] protected ScrollRect propertyDescriptionScrollView;
	[SerializeField] private RectTransform requiredResourceSpawnParent;
	[SerializeField] private ScrollRect requiredResourceScrollView;
	
	private void Awake() {
		buildableResourcePanel = transform.Find("BuildableResourcePanel").GetComponent<PreparationSlotLayoutViewController>();
		materialPanel = transform.Find("OwnedMaterialPanel").GetComponent<PreparationSlotLayoutViewController>();
		previewPanel = transform.Find("PreviewPanel").gameObject;
		//skillModel = this.GetModel<ISkillModel>();
		resourceBuildModel = this.GetModel<IResourceBuildModel>();
		
		inventoryModel = this.GetModel<IInventoryModel>();
		//previewDescriptionPanel = previewPanel.transform.Find("Mask/UpgradeGroup/DescriptionTag")
		//	.GetComponent<SlotResourceDescriptionPanel>();
		purchaseButton = previewPanel.transform.Find("PurchaseButton").GetComponent<Button>();
		purchaseButton.onClick.AddListener(OnPurchaseClicked);
		
		//previewLayout = previewPanel.transform.Find("RequiredResourcesLayout");
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
		//previewDescriptionPanel.Clear();
		ClearDescriptionPanel();
		foreach (Transform obj in previewRarityBar) {
			Destroy(obj.gameObject);
		}
		purchaseButton.gameObject.SetActive(false);
		purchaseButton.interactable = false;
		currentPreviewSlot = null;
		foreach (Transform obj in requiredResourceSpawnParent) {
			Destroy(obj.gameObject);
		}
		if (currentPreviewEntity != null) {
			GlobalEntities.GetEntityAndModel(currentPreviewEntity.UUID).Item2.RemoveEntity(currentPreviewEntity.UUID);
			currentPreviewEntity = null;
		}
	}

	private IEnumerator RebuildDetailDescriptionPanel() {
		LayoutRebuilder.ForceRebuildLayoutImmediate(detailPanel);
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(detailPanel);
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
			ResourceTemplateInfo templateInfo = ResourceTemplates.Singleton.GetResourceTemplates(resourceName);
			IBuildableResourceEntity templateEntity = templateInfo.TemplateEntity as IBuildableResourceEntity;

			IResourceEntity entity = templateInfo.EntityCreater
				.Invoke(true, templateEntity.GetMinRarity());
			
			entitiesToRemoveWhenClear.Add(entity);
			PreparationSlot slot = new PreparationSlot();
			slot.TryAddItem(entity);
			buildableSlots.Add(slot);
		}

		buildableResourcePanel.OnShowItems(buildableSlots);

		foreach (var slots in buildableResourcePanel.GetAllSlots()) {
			slots.GetComponent<PreparationSlotViewController>().SetNewItemHint(GetIsSlotNew(slots.Slot));
		}
		
		buildableResourcePanel.RegisterOnSlotClicked(OnSlotClicked);
	}
	
	private bool GetIsSlotNew(ResourceSlot slot) {
		string itemUUID = slot.GetLastItemUUID();
		if (itemUUID == null || slot.IsEmpty()) {
			return false;
		}

		string resourceName = slot.EntityKey;
		bool isNew = resourceBuildModel.IsNew(resourceName);
		return isNew;
	}

	private void OnSlotClicked(ResourceSlotViewController slotVC, PreparationSlotLayoutViewController layout, bool isSelectedAlready) {
		currentPreviewSlot = slotVC.Slot;

		if (currentPreviewSlot != null && !currentPreviewSlot.IsEmpty()) {
			resourceBuildModel.SetIsNew(slotVC.Slot.EntityKey, false);
			slotVC.GetComponent<PreparationSlotViewController>().SetNewItemHint(false);
		}

		SetPreviewedRarity(1);
	}

	private void SetPreviewedRarity(int rarity) {
		var slot = currentPreviewSlot;
		ClearPreview();
		if(slot == null || slot.IsEmpty()) return;
		
	
		
		currentPreviewSlot = slot;
		previewPanel.SetActive(true);
		
		IBuildableResourceEntity templateEntity =
			GlobalGameResourceEntities.GetAnyResource(currentPreviewSlot.GetLastItemUUID()) as IBuildableResourceEntity;
		
		if (templateEntity == null) {
			return;
		}
		
		int maxRarity = templateEntity.GetMaxRarity();
		int minRarity = templateEntity.GetMinRarity();
		if (rarity > maxRarity) {
			rarity = 1;
		}else if (rarity < 1) {
			rarity = maxRarity;
		}
		
		rarity = Math.Max(minRarity, rarity);

		previewSelectionObject.SetActive(maxRarity != minRarity);
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

		IWeaponPartsEntity weaponPartsEntity = currentPreviewEntity as IWeaponPartsEntity;
		CurrencyType? currencyType = null;
		if (weaponPartsEntity != null) {
			currencyType = weaponPartsEntity.GetBuildType();
		}
		
		
		float height = previewRarityBar.rect.height;
		for (int i = 1; i <= currentPreviewEntity.GetMaxRarity(); i++) {
			GameObject rarityPrefab = i <= rarity ? rarityStarFilled : rarityStarEmpty;
			RectTransform tr = Instantiate(rarityPrefab, previewRarityBar).GetComponent<RectTransform>();
			tr.sizeDelta = new Vector2(height, height);
			RarityIndicator rarityIndicator = tr.GetComponent<RarityIndicator>();
			rarityIndicator.SetCurrency(currencyType);
		}


		SetDescriptionPanel(currentPreviewEntity, rarity, skillUseCost, currencyType);
		
		
		/*previewDescriptionPanel.SetContent(currentPreviewEntity.GetDisplayName(), currentPreviewEntity.GetDescription(),
			InventorySpriteFactory.Singleton.GetSprite(currentPreviewEntity.EntityName), true, rarity,
			ResourceVCFactory.GetLocalizedResourceCategory(currentPreviewEntity.GetResourceCategory()),
			currentPreviewEntity.GetResourcePropertyDescriptions(), skillUseCost, currencyType);*/

		PurchaseCostInfo costInfo = templateEntity.GetPurchaseCost();
		HashSet<PreparationSlot> ownedResources = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);


		//previewLevelText.text = Localization.GetFormat("BUILD_PREVIEW_LEVEL_TEXT", rarity);

		bool isAllEnough = CheckIsAllEnough(costInfo, ownedResources);
		SpawnRequiredResourceDisplay(costInfo, ownedResources);
		
		if (isAllEnough) {
			purchaseButton.interactable = true;
			purchaseButton.gameObject.SetActive(true);
		}
		else {
			purchaseButton.interactable = false;
			purchaseButton.gameObject.SetActive(false);
		}
	}
	private void ClearDescriptionPanel() {
		foreach (Transform child in useCostElementSpawnParent.transform) {
			Destroy(child.gameObject);
		}
		useCostPanel.SetActive(false);
		
		ClearPropertyDescriptionPanel();
		StartCoroutine(RebuildDetailDescriptionPanel());
	}
	
	private void ClearPropertyDescriptionPanel() {
		if (propertyDescriptionItemParent.childCount <= 0) {
			return;
		}
		foreach (Transform child in propertyDescriptionItemParent) {
			Destroy(child.gameObject);
		}
		StartCoroutine(RefreshPropertyDescriptionPanelLayout());
	}
	
	private IEnumerator RefreshPropertyDescriptionPanelLayout() {
		propertyDescriptionScrollView.verticalNormalizedPosition = 1;
		propertyDescriptionScrollView.verticalScrollbar.value = 1;
		LayoutRebuilder.ForceRebuildLayoutImmediate(propertyDescriptionItemParent);
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(propertyDescriptionItemParent);
		propertyDescriptionScrollView.verticalNormalizedPosition = 1;
		propertyDescriptionScrollView.verticalScrollbar.value = 1;
	}
	
	
	private void SetDescriptionPanel(IBuildableResourceEntity buildableResourceEntity, 
		int rarity, Dictionary<CurrencyType,int> skillUseCost, CurrencyType? currencyType) {
		
		descriptionIcon.sprite = InventorySpriteFactory.Singleton.GetSprite(buildableResourceEntity.EntityName);
		itemNameText.text = buildableResourceEntity.GetDisplayName();
		descriptionText.text = buildableResourceEntity.GetDescription();
		
		if(skillUseCost != null && skillUseCost.Count > 0) {
			useCostPanel.SetActive(true);
			foreach (CurrencyType type in skillUseCost.Keys) {
				int cost = skillUseCost[type];
				if (cost <= 0) {
					continue;
				}
				GameObject costElement = Instantiate(useCostElementPrefab, useCostElementSpawnParent.transform);
				TMP_Text text = costElement.transform.Find("Text").GetComponent<TMP_Text>();
				text.text = $"<size=130%><sprite index={(int)type}></size> {cost}";
			}
		}
		else {
			useCostPanel.SetActive(false);
		}

		var propertyDescriptions = buildableResourceEntity.GetResourcePropertyDescriptions();
		if (propertyDescriptions is {Count: > 0} && propertyDescriptions.Any((propertyDescription => propertyDescription.display))) {
			propertyDescriptionScrollView.gameObject.SetActive(true);
			SetPropertyDescriptions(propertyDescriptions);
		}
		else {
			propertyDescriptionScrollView.gameObject.SetActive(false);
		}
		
		StartCoroutine(RebuildDetailDescriptionPanel());
	}
	
	private void SetPropertyDescriptions(List<ResourcePropertyDescription> propertyDescriptions) {
		ClearPropertyDescriptionPanel();
		
		foreach (ResourcePropertyDescription propertyDescription in propertyDescriptions) {
			if (!propertyDescription.display) {
				continue;
			}
			GameObject propertyDescriptionItem = Instantiate(propertyDescriptionItemPrefab, propertyDescriptionItemParent);
			propertyDescriptionItem.transform.localScale = Vector3.one;
			propertyDescriptionItem.GetComponent<BuildPanelPropertyDescriptionItem>()
				.SetContent(propertyDescription.LocalizedPropertyName, propertyDescription.GetLocalizedDescription());
		}
		
		StartCoroutine(RefreshPropertyDescriptionPanelLayout());
	}

	private bool CheckIsAllEnough(PurchaseCostInfo costInfo, HashSet<PreparationSlot> ownedResources) {
		bool isAllEnough = true;

		if (costInfo != null) {
			//SpawnRequiredResourceDisplay(moneySprite, costInfo.MoneyCost, costInfo.MoneyCost <= currencyModel.Money.Value);
			if (costInfo.MoneyCost > currencyModel.Money.Value) {
				isAllEnough = false;
			}


			foreach (KeyValuePair<string,int> info in costInfo.ResourceCost) {
				Sprite sprite = InventorySpriteFactory.Singleton.GetSprite(info.Key);
				int ownedCount = GetTotalResourceCountInSlots(ownedResources, info.Key);
				bool isEnough = ownedCount >= info.Value;
				//SpawnRequiredResourceDisplay(sprite, info.Value, isEnough);
				if (!isEnough) {
					isAllEnough = false;
				}
			}
			
			StartCoroutine(RebuildLayout(requiredResourceSpawnParent));
			
		}
		
		return isAllEnough;
	}

	private void SpawnRequiredResourceDisplay(PurchaseCostInfo costInfo, HashSet<PreparationSlot> ownedResources) {
		if (costInfo != null) {
			SpawnRequiredResourceDisplay(moneySprite, costInfo.MoneyCost,
				costInfo.MoneyCost <= currencyModel.Money.Value);

			foreach (KeyValuePair<string, int> info in costInfo.ResourceCost) {
				Sprite sprite = InventorySpriteFactory.Singleton.GetSprite(info.Key);
				int ownedCount = GetTotalResourceCountInSlots(ownedResources, info.Key);
				bool isEnough = ownedCount >= info.Value;
				SpawnRequiredResourceDisplay(sprite, info.Value, isEnough);
			}
		}


		StartCoroutine(RebuildRequiredResourceLayout());
	}
	
	private IEnumerator RebuildRequiredResourceLayout() {
		requiredResourceScrollView.verticalNormalizedPosition = 1;
		requiredResourceScrollView.verticalScrollbar.value = 1;
		LayoutRebuilder.ForceRebuildLayoutImmediate(requiredResourceSpawnParent);
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(requiredResourceSpawnParent);
		requiredResourceScrollView.verticalNormalizedPosition = 1;
		requiredResourceScrollView.verticalScrollbar.value = 1;
	}
	
	
	private GameObject SpawnRequiredResourceDisplay(Sprite sprite, int count, bool isEnough) {
		GameObject obj = Instantiate(requiredResourceDisplayPrefab, requiredResourceSpawnParent);
		obj.transform.localScale = Vector3.one;
		obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
		obj.transform.Find("Text").GetComponent<TMP_Text>().text = count.ToString();
		obj.transform.Find("Text").GetComponent<TMP_Text>().color =
			isEnough ? Color.black : new Color(0.8274511f, 0.2666667f, 0.2196079f);
		
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
			if (costInfo != null) {
				currencySystem.RemoveMoney(costInfo.MoneyCost);
				foreach (KeyValuePair<string,int> valuePair in costInfo.ResourceCost) {
					string resourceName = valuePair.Key;
					int count = valuePair.Value;
					inventorySystem.RemoveResourceEntityFromBaseStock(ResourceCategory.RawMaterial, resourceName, count,
						true);
				}
			}
			
			

			if (entity.GetResourceCategory() is ResourceCategory.Skill) {
				inventoryModel.AddToBaseStock(entity);
				entitiesToRemoveWhenClear.Remove(entity);
				resourceBuildModel.RemoveFromBuild(category, entity.EntityName);
				
			}else if (entity.GetResourceCategory() is ResourceCategory.WeaponParts) {
				IWeaponPartsModel weaponPartsModel = this.GetModel<IWeaponPartsModel>();
				weaponPartsModel.AddToUnlockedParts(entity.EntityName);
				resourceBuildModel.RemoveFromBuild(category, entity.EntityName);
				
			}else if (entity.GetResourceCategory() is ResourceCategory.Weapon) {
				inventoryModel.AddToBaseStock(entity);
				entitiesToRemoveWhenClear.Remove(entity);
			}
			
			
			//skillModel.GetPurchasableSkillSlots()
		}
		Refresh();

	}


	public override void OnSwitchToOtherPanel() {
		base.OnSwitchToOtherPanel();
		Clear();
	}

	public void OnSetResourceCategory(ResearchCategory msgCategory) {
		category = msgCategory;
	}
}
