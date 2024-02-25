using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.ResourceCrafting.Commands.Research;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.RawMaterials.Model.Base;
using Runtime.UI.Crafting.Research;
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
	[SerializeField] private GameObject[] rewardPreviewIcons_researching;
	[SerializeField] private GameObject rewardMoreIcon;
	[SerializeField] private GameObject rewardMoreIcon_researching;
	[SerializeField] private Button researchButton;
	[SerializeField] private GameObject researchAvailablePanel;
	[SerializeField] private GameObject researchUnavailablePanel;
	[SerializeField] private TMP_Text remainingDaysText;
	[SerializeField] private  TMP_Text counterRemainingDaysText;
	[SerializeField] private Image researchingFillBar;
	[SerializeField] private GameObject noResourceHint;
	
 	private ResearchLevelInfo[] researchResults;
	
	private ResearchCategory category;
	private IInventoryModel inventoryModel;
	private IResourceResearchModel resourceResearchModel;
	private ICurrencyModel currencyModel;
	int totalCost = 0;
	int totalExp = 0;
	int researchDays = 0;
	private IResearchSystem researchSystem;
	private void Awake() {
		inventoryModel = this.GetModel<IInventoryModel>();
		resourceResearchModel = this.GetModel<IResourceResearchModel>();
		currencyModel = this.GetModel<ICurrencyModel>();
		researchSystem = this.GetSystem<IResearchSystem>();
		researchButton.onClick.AddListener(OnResearchButtonClicked);
	}

	


	public override void OnSwitchToPanel() {
		base.OnSwitchToPanel();
		InitPanel();
	}


	public override void OnSwitchToOtherPanel() {
		base.OnSwitchToOtherPanel();

		var resources = selectedSlotLayoutViewController.GetAllExpResources();
		foreach (var resource in resources) {
			selectedSlotLayoutViewController.RemoveItem(resource);
			ownedSlotLayoutViewController.AddItem(resource);
		}
		
		DestroySlots();
		
	}
	
	private void DestroySlots() {
		researchAvailablePanel.SetActive(true);
		ownedSlotLayoutViewController.OnUIClosed();
		selectedSlotLayoutViewController.OnUIClosed();
		ownedSlotLayoutViewController.UnRegisterOnSlotClicked(OnOwnedSlotClicked);
		selectedSlotLayoutViewController.UnRegisterOnSlotClicked(OnSelectedSlotClicked);
		researchAvailablePanel.SetActive(false);
	}


	private void InitPanel() {
		researchAvailablePanel.SetActive(true);
		allresearchedHint.SetActive(false);
		costPanel.SetActive(false);
		noResourceSelectedHint.SetActive(false);
		noResourceHint.SetActive(false);
		if (resourceResearchModel.IsAllResearched(category)) {
			allresearchedHint.SetActive(true);
			return;
		}

		
		List<ResourceSlot> resourceSlots  = new List<ResourceSlot>();
	
		
		var rawMaterials = inventoryModel.GetBaseStock(ResourceCategory.RawMaterial);
		noResourceHint.SetActive(rawMaterials == null || rawMaterials.Count == 0);
		
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
		
		UpdatePanel();
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
		UpdatePanel();
	}

	private void OnOwnedSlotClicked(ResourceSlotViewController slotVC) {
		if (slotVC.Slot.IsEmpty() || researchSystem.IsResearching(category)) {
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
		UpdatePanel();
	}


	private void UpdatePanel() {
		
		if (researchSystem.IsResearching(category)) {
			researchAvailablePanel.SetActive(false);
			researchUnavailablePanel.SetActive(true);
			UpdateResearchingPanel();
		}
		else {
			researchAvailablePanel.SetActive(true);
			researchUnavailablePanel.SetActive(false);
			UpdateCost();
		}
	
	}

	private void UpdateResearchingPanel() {
		ResearchEvent researchEvent = researchSystem.GetResearchEvent(category);
		if(researchEvent == null) {
			return;
		}

		researchingFillBar.fillAmount = (float) (researchEvent.TotalMinutes - researchEvent.RemainingMinutesToTrigger) / researchEvent.TotalMinutes;
		
		float days = researchEvent.RemainingMinutesToTrigger / 60f / 24f;
		counterRemainingDaysText.text = Mathf.CeilToInt(days).ToString();
		remainingDaysText.text = Localization.GetFormat("RESEARCH_DOING_RESEARCH_REMAINING_DAY",
			$"<color=#1D9F00>{Mathf.CeilToInt(days)}</color>", days > 1 ? "s" : "");
		
		foreach (GameObject rewardPreviewIcon in rewardPreviewIcons_researching) {
			rewardPreviewIcon.SetActive(false);
		}

		rewardMoreIcon_researching.SetActive(false);
		int potentialRewardCount = 0;
		foreach (ResearchLevelInfo researchLevelInfo in researchEvent.ResearchResults) {
			potentialRewardCount += researchLevelInfo.ResearchedEntityNames?.Length ?? 0;
		}
		
		
		for (int i = 0; i < potentialRewardCount; i++) {
			if (i < rewardPreviewIcons_researching.Length) {
				rewardPreviewIcons_researching[i].SetActive(true);
			}else {
				rewardMoreIcon_researching.SetActive(true);
				break;
			}
		}
	}

	private void UpdateCost() {
		costPanel.SetActive(false);
		noResourceSelectedHint.SetActive(false);
		noResourceHint.SetActive(false);

		noResourceHint.SetActive(ownedSlotLayoutViewController.IsAllEmpty());
		
		if (selectedSlotLayoutViewController.IsAllEmpty()) {
			noResourceSelectedHint.SetActive(true);
			return;
		}
		costPanel.SetActive(true);

		HashSet<IHaveExpResourceEntity> selectedResources = selectedSlotLayoutViewController.GetAllExpResources();
		totalExp = 0;
		foreach (var resource in selectedResources) {
			totalExp += resource.GetExpProperty().RealValue.Value;
		}

		totalCost = Mathf.CeilToInt(ResourceResearchModel.CostPerExp * totalExp);
		
		int resultLevel = resourceResearchModel.GetLevelIfExpAdded(category, totalExp,
			out researchResults, out researchDays);

		bool moneyEnough = currencyModel.Money >= totalCost;
		string color = moneyEnough ? "green" : "#FF4400";
		costText.text = $"<color={color}>{totalCost}<sprite index=6></color>";
		daysText.text = Localization.GetFormat("RESEARCH_DAYS", researchDays, researchDays > 1 ? "s" : "");

		int potentialRewardCount = 0;
		foreach (ResearchLevelInfo researchLevelInfo in researchResults) {
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

	private void OnResearchButtonClicked() {
		//send commands
		this.SendCommand<ResearchCommand>(ResearchCommand.Allocate(totalCost, totalExp, researchDays, category,
			selectedSlotLayoutViewController.GetAllNonEmptySlots(), researchResults));
		
		UpdatePanel();
	}
	
	
	public void OnSetResourceCategory(ResearchCategory category) {
		this.category = category;
		
	}
}
