using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Properties;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Inventory;
using Runtime.Inventory.Model;
using Runtime.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcePreviewPanel : AbstractMikroController<MainGame>
{
    [Header("Description Panel")] 
    [SerializeField] private GameObject requiredResourceDisplayPrefab;
    [SerializeField] private Sprite moneySprite;
    [SerializeField] private GameObject rarityStarFilled;
    [SerializeField] private GameObject rarityStarEmpty;
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
    [SerializeField] private GameObject previewSelectionObject;
    [SerializeField] private Button lastRarityButton;
    [SerializeField] private Button nextRarityButton;
    [SerializeField] private RectTransform previewRarityBar;
    [SerializeField] private GameObject requiredResourcePanel;
    private RectTransform requiredResourcePanelRect;
    private float requiredResourcePanelRectHeight;
    private int selectedRarity = 1;
    private IResourceEntity currentPreviewEntity;
    private IResourceEntity currentPreviewMainEntity;
    private IInventoryModel inventoryModel;
    private Button purchaseButton;
    private bool currentPreviewPurchaseable;
    private ICurrencyModel currencyModel;
    private bool inited = false;
    private void Awake() {
	    if (inited) {
		    return;
	    }
	    inited = true;
	    inventoryModel = this.GetModel<IInventoryModel>();
        requiredResourcePanelRect = requiredResourceScrollView.GetComponent<RectTransform>();
        requiredResourcePanelRectHeight = requiredResourcePanelRect.sizeDelta.y;
        purchaseButton = transform.Find("PurchaseButton").GetComponent<Button>();
        currencyModel = this.GetModel<ICurrencyModel>();
        lastRarityButton.onClick.AddListener(OnLastRarityClicked);
        nextRarityButton.onClick.AddListener(OnNextRarityClicked);
    }
    
    private void OnLastRarityClicked() {
	    SetPreviewedRarity(selectedRarity - 1);
    }

    private void OnNextRarityClicked() {
	    SetPreviewedRarity(selectedRarity + 1);
    }
    private void ClearDescriptionPanel() {
	    foreach (Transform child in useCostElementSpawnParent.transform) {
		    Destroy(child.gameObject);
	    }
	    useCostPanel.SetActive(false);
		
	    ClearPropertyDescriptionPanel();
	    if (gameObject.activeInHierarchy) {
		    StartCoroutine(RebuildDetailDescriptionPanel());
	    }
	   
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

    private IEnumerator RebuildDetailDescriptionPanel() {
	    LayoutRebuilder.ForceRebuildLayoutImmediate(detailPanel);
	    yield return new WaitForEndOfFrame();
	    LayoutRebuilder.ForceRebuildLayoutImmediate(detailPanel);
    }

    public void ClearPreview() {
	    if (!inited) {
		    Awake();
	    }
	   
	    detailPanel.gameObject.SetActive(true);
	    ClearDescriptionPanel();
	    foreach (Transform obj in previewRarityBar) {
		    Destroy(obj.gameObject);
	    }
	    purchaseButton.gameObject.SetActive(false);
	    purchaseButton.interactable = false;
	    currentPreviewEntity = null;
	    //currentPreviewMainEntity = null;
	    foreach (Transform obj in requiredResourceSpawnParent) {
		    Destroy(obj.gameObject);
	    }
	    if (currentPreviewEntity != null) {
		    GlobalEntities.GetEntityAndModel(currentPreviewEntity.UUID).Item2.RemoveEntity(currentPreviewEntity.UUID);
		    currentPreviewEntity = null;
	    }
	    requiredResourcePanel.SetActive(false);
    }
    

    public void SetPreviewedEntity(IResourceEntity entity, bool purchasable) {
	    if (entity == null) {
		    return;
	    }
	    currentPreviewMainEntity = entity;
	    currentPreviewPurchaseable = purchasable;
	    SetPreviewedRarity(1);
    }
    
	private void SetPreviewedRarity(int rarity) {
		ClearPreview();
		if(currentPreviewMainEntity == null) return;


		IResourceEntity templateEntity = currentPreviewMainEntity;
		
		if (templateEntity == null) {
			return;
		}

		int maxRarity = templateEntity.GetRarityProperty().RealValue.Value;
		int minRarity = maxRarity;
		PurchaseCostInfo costInfo = null;
		if (templateEntity is IBuildableResourceEntity buildableResourceEntity) {
			minRarity = buildableResourceEntity.GetMinRarity();
			maxRarity = buildableResourceEntity.GetMaxRarity();
			costInfo = buildableResourceEntity.GetPurchaseCost();
		}
		
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
			.Invoke(true, rarity);
		
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
		for (int i = 1; i <= maxRarity; i++) {
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

		
		
		if (costInfo != null && currentPreviewPurchaseable) {
			requiredResourcePanel.SetActive(true);
			HashSet<PreparationSlot> ownedResources =
				inventoryModel.GetBaseStock(ResourceCategory.RawMaterial, ResourceCategory.Scrap);


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

		int activeChildrenCount = 0;
		bool allChildrenInactive = true;
		foreach (Transform child in detailPanel) {
			if (child.gameObject.activeSelf) {
				activeChildrenCount++;
				allChildrenInactive = false;
			}
		}
		
		if (allChildrenInactive) {
			detailPanel.gameObject.SetActive(false);
		}
		
		
		if(activeChildrenCount <= 2 && requiredResourcePanel.activeSelf) {
			requiredResourcePanelRect.sizeDelta = new Vector2(requiredResourcePanelRect.sizeDelta.x, requiredResourcePanelRectHeight * 2);
		}else {
			requiredResourcePanelRect.sizeDelta = new Vector2(requiredResourcePanelRect.sizeDelta.x, requiredResourcePanelRectHeight);
		}
		
		StartCoroutine(RebuildDetailDescriptionPanel());
		
	}
	private void SetDescriptionPanel(IResourceEntity resourceEntity, 
		int rarity, Dictionary<CurrencyType,int> skillUseCost, CurrencyType? currencyType) {
		
		descriptionIcon.sprite = InventorySpriteFactory.Singleton.GetSprite(resourceEntity);
		itemNameText.text = resourceEntity.GetDisplayName();
		descriptionText.text = resourceEntity.GetDescription();
		
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

		var propertyDescriptions = resourceEntity.GetResourcePropertyDescriptions();
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
	
	
	public static bool CheckIsAllEnough(PurchaseCostInfo costInfo, HashSet<PreparationSlot> ownedResources) {
		bool isAllEnough = true;

		ICurrencyModel currencyModel = MainGame.Interface.GetModel<ICurrencyModel>();
		if (costInfo != null) {
			//SpawnRequiredResourceDisplay(moneySprite, costInfo.MoneyCost, costInfo.MoneyCost <= currencyModel.Money.Value);
			if (costInfo.MoneyCost > currencyModel.Money.Value) {
				isAllEnough = false;
			}


			foreach (KeyValuePair<string,int> info in costInfo.ResourceCost) {
				int ownedCount = GetTotalResourceCountInSlots(ownedResources, info.Key);
				bool isEnough = ownedCount >= info.Value;
				//SpawnRequiredResourceDisplay(sprite, info.Value, isEnough);
				if (!isEnough) {
					isAllEnough = false;
				}
			}
		}
		
		return isAllEnough;
	}
	private void SpawnRequiredResourceDisplay(PurchaseCostInfo costInfo, HashSet<PreparationSlot> ownedResources) {
		if (costInfo != null) {
			SpawnRequiredResourceDisplay(moneySprite, costInfo.MoneyCost,
				costInfo.MoneyCost <= currencyModel.Money.Value, 0);

			foreach (KeyValuePair<string, int> info in costInfo.ResourceCost) {
				Sprite sprite = InventorySpriteFactory.Singleton.GetSprite(info.Key);
				int ownedCount = GetTotalResourceCountInSlots(ownedResources, info.Key);
				bool isEnough = ownedCount >= info.Value;
			
			
				int rarity = ResourceTemplates.Singleton.GetResourceTemplates(info.Key).TemplateEntity.GetRarityProperty().RealValue
					.Value;
				
				SpawnRequiredResourceDisplay(sprite, info.Value, isEnough, rarity);
			}
		}


		StartCoroutine(RebuildRequiredResourceLayout());
	}
	
	private GameObject SpawnRequiredResourceDisplay(Sprite sprite, int count, bool isEnough, int rarity) {
		GameObject obj = Instantiate(requiredResourceDisplayPrefab, requiredResourceSpawnParent);
		obj.transform.localScale = Vector3.one;
		obj.transform.Find("Image").GetComponent<Image>().sprite = sprite;
		obj.transform.Find("Text").GetComponent<TMP_Text>().text = count.ToString();
		obj.transform.Find("Text").GetComponent<TMP_Text>().color =
			isEnough ? Color.black : new Color(0.8274511f, 0.2666667f, 0.2196079f);
		RectTransform rarityBar = obj.transform.Find("RarityBar").GetComponent<RectTransform>();
		float height = rarityBar.rect.height;
		for (int i = 0; i < rarity; i++) {
			RectTransform tr = Instantiate(rarityStarFilled, rarityBar).GetComponent<RectTransform>();
			tr.sizeDelta = new Vector2(height, height);
		}
		
		StartCoroutine(RebuildLayout(obj.GetComponent<RectTransform>()));
		return obj;
	}
	private static int GetTotalResourceCountInSlots(HashSet<PreparationSlot> slots, string resourceName) {
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
	private IEnumerator RebuildLayout(RectTransform rectTransform) {
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
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
}
