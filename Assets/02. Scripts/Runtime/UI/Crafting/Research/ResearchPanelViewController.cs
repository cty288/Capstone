using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.RawMaterials.Model.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchPanelViewController : SwitchableSubPanel {
	[SerializeField] private ResearchPanelSlotLayoutViewController ownedSlotLayoutViewController;
	[SerializeField] private ResearchPanelSlotLayoutViewController selectedSlotLayoutViewController;
	[SerializeField] private GameObject costPanel;
	[SerializeField] private GameObject allresearchedHint;
	[SerializeField] private GameObject noResourceSelectedHint;
	[SerializeField] private TMP_Text costText;
	[SerializeField] private TMP_Text daysText;
	[SerializeField] private GameObject[] rewardPreviewIcons;
	[SerializeField] private GameObject rewardMoreIcon;
	[SerializeField] private Button researchButton;
	
	
	private ResourceCategory category;
	private IInventoryModel inventoryModel;
	private IResourceResearchModel resourceResearchModel;
	private ICurrencyModel currencyModel;
	
	private void Awake() {
		inventoryModel = this.GetModel<IInventoryModel>();
		resourceResearchModel = this.GetModel<IResourceResearchModel>();
		currencyModel = this.GetModel<ICurrencyModel>();
	}


	public override void OnSwitchToPanel() {
		base.OnSwitchToPanel();
		InitPanel();
	}


	public override void OnSwitchToOtherPanel() {
		base.OnSwitchToOtherPanel();
		ownedSlotLayoutViewController.OnUIClosed();
		selectedSlotLayoutViewController.OnUIClosed();
		ownedSlotLayoutViewController.UnRegisterOnSlotClicked(OnOwnedSlotClicked);
		selectedSlotLayoutViewController.UnRegisterOnSlotClicked(OnSelectedSlotClicked);
		//TODO: move selected back to owned
	}


	private void InitPanel() {
		allresearchedHint.SetActive(false);
		costPanel.SetActive(false);
		noResourceSelectedHint.SetActive(false);
		
		if (resourceResearchModel.IsAllResearched(category)) {
			allresearchedHint.SetActive(true);
			return;
		}

		
		List<ResourceSlot> resourceSlots  = new List<ResourceSlot>();
		var rawMaterials = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);
		if (rawMaterials != null) {
			foreach (var rawMaterial in rawMaterials) {
				resourceSlots.Add(rawMaterial);
			}
		}
		


		ownedSlotLayoutViewController.OnShowItems(resourceSlots);
		ownedSlotLayoutViewController.RegisterOnSlotClicked(OnOwnedSlotClicked);


		List<ResourceSlot> selectedSlots = new List<ResourceSlot>();
		int slotCount = resourceSlots.Count;
		
		for (int i = 0; i < slotCount; i++) {
			selectedSlots.Add(new PreparationSlot());
		}
		
		selectedSlotLayoutViewController.OnShowItems(selectedSlots);
		selectedSlotLayoutViewController.RegisterOnSlotClicked(OnSelectedSlotClicked);
		
		UpdateCost();
	}

	private void OnSelectedSlotClicked(ResourceSlotViewController slotVC) {
		if (slotVC.Slot.IsEmpty()) {
			return;
		}
		
		ResourceSlot slot = slotVC.Slot;
		string lastItemID = slot.GetLastItemUUID();
		IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(lastItemID);
		if (entity == null) {
			return;
		}
		
		slot.RemoveLastItem();
		ownedSlotLayoutViewController.AddItem(entity);
		UpdateCost();
	}

	private void OnOwnedSlotClicked(ResourceSlotViewController slotVC) {
		if (slotVC.Slot.IsEmpty()) {
			return;
		}
		
		ResourceSlot slot = slotVC.Slot;
		string lastItemID = slot.GetLastItemUUID();
		IResourceEntity entity = GlobalGameResourceEntities.GetAnyResource(lastItemID);
		if (entity == null) {
			return;
		}
		
		slot.RemoveLastItem();
		selectedSlotLayoutViewController.AddItem(entity);
		UpdateCost();
	}

	private void UpdateCost() {
		costPanel.SetActive(false);
		noResourceSelectedHint.SetActive(false);
		
		if (selectedSlotLayoutViewController.IsAllEmpty()) {
			noResourceSelectedHint.SetActive(true);
			return;
		}
		costPanel.SetActive(true);

		HashSet<IHaveExpResourceEntity> selectedResources = selectedSlotLayoutViewController.GetAllExpResources();
		int totalExp = 0;
		foreach (var resource in selectedResources) {
			totalExp += resource.GetExpProperty().RealValue.Value;
		}

		int totalCost = Mathf.CeilToInt(ResourceResearchModel.CostPerExp * totalExp);
		
		int resultLevel = resourceResearchModel.GetLevelIfExpAdded(category, totalExp,
			out ResearchLevelInfo[] addedResearchLevelInfos, out int researchDays);

		bool moneyEnough = currencyModel.Money >= totalCost;
		string color = moneyEnough ? "#1D9F00" : "red";
		costText.text = Localization.GetFormat("RESEARCH_COST", $"<color={color}>{totalCost}<sprite index=6></color>");
		daysText.text = Localization.GetFormat("RESEARCH_DAYS", researchDays, researchDays > 1 ? "s" : "");

		int potentialRewardCount = 0;
		foreach (ResearchLevelInfo researchLevelInfo in addedResearchLevelInfos) {
			potentialRewardCount += researchLevelInfo.ResearchedEntityNames?.Length ?? 0;
		}
		
		
		foreach (GameObject rewardPreviewIcon in rewardPreviewIcons) {
			rewardPreviewIcon.SetActive(false);
		}

		rewardMoreIcon.SetActive(false);
		
		for (int i = 0; i < potentialRewardCount; i++) {
			if (i < rewardPreviewIcons.Length) {
				rewardPreviewIcons[i].SetActive(true);
			}
			else {
				rewardMoreIcon.SetActive(true);
				break;
			}
		}

		researchButton.interactable = moneyEnough;

	}

	
	public void OnSetResourceCategory(ResourceCategory category) {
		this.category = category;
		
	}
}
