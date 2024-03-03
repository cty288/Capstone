using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using _02._Scripts.Runtime.ResourceCrafting.Models.Build;
using _02._Scripts.Runtime.Skills.Model.Properties;
using _02._Scripts.Runtime.WeaponParts.Model;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using UnityEngine;
using UnityEngine.UI;

public class BaseInventorySubPanel : SwitchableSubPanel
{
	private PreparationSlotLayoutViewController ownedResourcePanel;
	
	//private GameObject previewPanel;
	//private ISkillModel skillModel;
	private IInventoryModel inventoryModel;
	//private SlotResourceDescriptionPanel previewDescriptionPanel;
	
	private ResourceSlot currentPreviewSlot;
	
	
	private ResourceCategory category;
	private IResourceBuildModel resourceBuildModel;
	private IWeaponPartsModel weaponPartsModel;
	
	//private HashSet<IResourceEntity> entitiesToRemoveWhenClear = new HashSet<IResourceEntity>();
	private IBuildableResourceEntity currentPreviewEntity = null;
	
	//[SerializeField] private TMP_Text previewLevelText;
	
	[SerializeField] private ResourcePreviewPanel previewPanel;
	
	private HashSet<IResourceEntity> entitiesToRemoveWhenClear = new HashSet<IResourceEntity>();

	private void Awake() {
		
		ownedResourcePanel = transform.Find("OwnedMaterialPanel").GetComponent<PreparationSlotLayoutViewController>();
		resourceBuildModel = this.GetModel<IResourceBuildModel>();
		inventoryModel = this.GetModel<IInventoryModel>();
		weaponPartsModel = this.GetModel<IWeaponPartsModel>();
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
		
		ownedResourcePanel.OnUIClosed();
		ownedResourcePanel.UnRegisterOnSlotClicked(OnSlotClicked);
		ClearPreview();
		
	}

	private void ClearPreview() {
		previewPanel.ClearPreview();
		previewPanel.gameObject.SetActive(false);
		currentPreviewSlot = null;
	}


	

	private void Refresh() {
		Clear();

		//for materials, skills, weapons -> get from base stock
		HashSet<PreparationSlot> slots = new HashSet<PreparationSlot>();
		if (category != ResourceCategory.WeaponParts) {
			slots = inventoryModel.GetBaseStock(category);
		}else {
			HashSet<string> unlockedParts = weaponPartsModel.GetUnlockedParts();
			if (unlockedParts == null || unlockedParts.Count == 0) {
				return;
			}
			
			foreach (string resourceName in unlockedParts) {
				ResourceTemplateInfo templateInfo = ResourceTemplates.Singleton.GetResourceTemplates(resourceName);
				IBuildableResourceEntity templateEntity = templateInfo.TemplateEntity as IBuildableResourceEntity;

				IResourceEntity entity = templateInfo.EntityCreater
					.Invoke(true, templateEntity.GetMinRarity());
			
				entitiesToRemoveWhenClear.Add(entity);
				PreparationSlot slot = new PreparationSlot();
				slot.TryAddItem(entity);
				slots.Add(slot);
			}
		}
		if (slots == null || slots.Count == 0) {
			return;
		}
		ownedResourcePanel.OnShowItems(slots);
		ownedResourcePanel.RegisterOnSlotClicked(OnSlotClicked);
	}
	
	

	private void OnSlotClicked(ResourceSlotViewController slotVC, PreparationSlotLayoutViewController layout, bool isSelectedAlready) {
		currentPreviewSlot = slotVC.Slot;

		if (currentPreviewSlot != null && !currentPreviewSlot.IsEmpty()) {
			resourceBuildModel.SetIsNew(slotVC.Slot.EntityKey, false);
			slotVC.GetComponent<PreparationSlotViewController>().SetNewItemHint(false);

			IResourceEntity templateEntity =
				GlobalGameResourceEntities.GetAnyResource(currentPreviewSlot.GetLastItemUUID());
			
			previewPanel.gameObject.SetActive(true);

			previewPanel.SetPreviewedEntity(templateEntity, false);
		}

		//SetPreviewedRarity(1);
		
	}
	
	public override void OnSwitchToOtherPanel() {
		base.OnSwitchToOtherPanel();
		Clear();
	}

	public void OnSetResourceCategory(ResourceCategory msgCategory) {
		category = msgCategory;
	}
}
