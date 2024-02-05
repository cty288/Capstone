using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Properties;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPurchaseUI  :  AbstractPanel, IController, IGameUIPanel 
{
	/*private PreparationSlotLayoutViewController skillPanel;
	private PreparationSlotLayoutViewController materialPanel;
	private GameObject previewPanel;
	private ISkillModel skillModel;
	private IInventoryModel inventoryModel;
	private SlotResourceDescriptionPanel previewDescriptionPanel;
	private Button purchaseButton;
	private ResourceSlot currentPreviewSlot;
	private Transform previewLayout;
	private ICurrencyModel currencyModel;
	private ICurrencySystem currencySystem;
	private ISkillSystem skillSystem;
	private IInventorySystem inventorySystem;
	
	[SerializeField] private GameObject requiredResourceDisplayPrefab;
	[SerializeField] private Sprite moneySprite;
	
	public override void OnInit() {
		skillPanel = transform.Find("SkillPanel").GetComponent<PreparationSlotLayoutViewController>();
		materialPanel = transform.Find("OwnedMaterialPanel").GetComponent<PreparationSlotLayoutViewController>();
		previewPanel = transform.Find("PreviewPanel").gameObject;
		skillModel = this.GetModel<ISkillModel>();
		inventoryModel = this.GetModel<IInventoryModel>();
		previewDescriptionPanel = previewPanel.transform.Find("Mask/UpgradeGroup/DescriptionTag")
			.GetComponent<SlotResourceDescriptionPanel>();
		purchaseButton = previewPanel.transform.Find("PurchaseButton").GetComponent<Button>();
		purchaseButton.onClick.AddListener(OnPurchaseClicked);
		
		previewLayout = previewPanel.transform.Find("RequiredResourcesLayout");
		currencyModel = this.GetModel<ICurrencyModel>();
		currencySystem = this.GetSystem<ICurrencySystem>();
		skillSystem = this.GetSystem<ISkillSystem>();
		inventorySystem = this.GetSystem<IInventorySystem>();
	}

	

	public override void OnOpen(UIMsg msg) {
		Refresh();
	}

	private void Clear() {
		skillPanel.OnUIClosed();
		materialPanel.OnUIClosed();
		skillPanel.UnRegisterOnSlotClicked(OnSlotClicked);
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
	}
	
	private void Refresh() {
		Clear();
		skillPanel.OnShowItems(skillModel.GetPurchasableSkillSlots());
		materialPanel.OnShowItems(inventoryModel.GetBaseStock(ResourceCategory.RawMaterial));
		skillPanel.RegisterOnSlotClicked(OnSlotClicked);
	}

	private void OnSlotClicked(ResourceSlotViewController slotVC, PreparationSlotLayoutViewController layout, bool isSelectedAlready) {
		ClearPreview();
		
		previewPanel.SetActive(true);
		currentPreviewSlot = slotVC.Slot;
		ISkillEntity entity = GlobalGameResourceEntities.GetAnyResource(slotVC.Slot.GetLastItemUUID()) as ISkillEntity;
		
		
		if (entity == null) {
			return;
		}
		
		previewDescriptionPanel.SetContent(entity.GetDisplayName(), entity.GetDescription(),
			InventorySpriteFactory.Singleton.GetSprite(entity.EntityName), true, entity.GetLevel(),
			ResourceVCFactory.GetLocalizedResourceCategory(entity.GetResourceCategory()),
			entity.GetResourcePropertyDescriptions(), entity.GetSkillUseCostOfCurrentLevel());

		PurchaseCostInfo costInfo = entity.GetPurchaseCost();
		HashSet<PreparationSlot> ownedResources = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);


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
		ISkillEntity entity = GlobalGameResourceEntities.GetAnyResource(currentPreviewSlot.GetLastItemUUID()) as ISkillEntity;
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
			
			skillSystem.RemoveSlot(currentPreviewSlot as PreparationSlot);
			inventoryModel.AddToBaseStock(entity);
			
			//skillModel.GetPurchasableSkillSlots()
		}
		Refresh();

	}
	
	
	public override void OnClosed() {
		Clear();
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}

	public IPanel GetClosePanel() {
		return this;
	}*/
	public override void OnInit() {
		throw new System.NotImplementedException();
	}

	public override void OnOpen(UIMsg msg) {
		throw new System.NotImplementedException();
	}

	public override void OnClosed() {
		throw new System.NotImplementedException();
	}

	public IArchitecture GetArchitecture() {
		throw new System.NotImplementedException();
	}

	public IPanel GetClosePanel() {
		throw new System.NotImplementedException();
	}
}
