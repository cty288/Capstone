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
	//private GameObject previewPanel;
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
	
	private HashSet<IResourceEntity> entitiesToRemoveWhenClear = new HashSet<IResourceEntity>();
	private IBuildableResourceEntity currentPreviewEntity = null;
	
	//[SerializeField] private TMP_Text previewLevelText;

	[SerializeField] private GameObject noBuildableResourceHint;
	[SerializeField] private GameObject noResourceHint;
	[SerializeField] private ResourcePreviewPanel previewPanel;
	


	private void Awake() {
		buildableResourcePanel = transform.Find("BuildableResourcePanel").GetComponent<PreparationSlotLayoutViewController>();
		materialPanel = transform.Find("OwnedMaterialPanel").GetComponent<PreparationSlotLayoutViewController>();
		
		resourceBuildModel = this.GetModel<IResourceBuildModel>();
		
		inventoryModel = this.GetModel<IInventoryModel>();
		purchaseButton = transform.Find("PreviewPanel/PurchaseButton").gameObject.GetComponent<Button>();
		purchaseButton.onClick.AddListener(OnPurchaseClicked);
		
		currencyModel = this.GetModel<ICurrencyModel>();
		currencySystem = this.GetSystem<ICurrencySystem>();
		inventorySystem = this.GetSystem<IInventorySystem>();
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
		previewPanel.ClearPreview();
		previewPanel.gameObject.SetActive(false);
		currentPreviewSlot = null;
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

			IResourceEntity templateEntity =
				GlobalGameResourceEntities.GetAnyResource(currentPreviewSlot.GetLastItemUUID());
			
			previewPanel.gameObject.SetActive(true);
			
			previewPanel.SetPreviewedEntity(templateEntity, true);
		}

		//SetPreviewedRarity(1);
		
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
		HashSet<PreparationSlot> ownedResources =
			inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);
		bool isAllEnough = ResourcePreviewPanel.CheckIsAllEnough(costInfo, ownedResources);

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
