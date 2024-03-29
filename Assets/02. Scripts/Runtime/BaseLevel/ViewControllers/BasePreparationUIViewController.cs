using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Commands;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BasePreparationUIViewController  : AbstractPanel, IController, IGameUIPanel {
	private bool canClose = true;
	private BasicSlotLayoutViewController weaponSlotLayout;
	private BasicSlotLayoutViewController skillSlotLayout;
	private IInventoryModel inventoryModel;
	
	private Dictionary<ResourceCategory, int> occupiedGeneralSlotCountDict = new Dictionary<ResourceCategory, int>();
	private Dictionary<ResourceCategory, int> emptyHotbarSlotCountDict = new Dictionary<ResourceCategory, int>();
	
	
	private int maxGeneralSlotCount = 0;
	private Dictionary<ResourceCategory, int> maxHotbarSlotCountDict = new Dictionary<ResourceCategory, int>();
	
	private TMP_Text remainingWeaponSlotCountText;
	private TMP_Text remainingSkillSlotCountText;
	private GameObject errorText;
	private GameObject enterButton;
	private IInventorySystem inventorySystem;

	public override void OnInit() {
		inventoryModel = this.GetModel<IInventoryModel>();
		weaponSlotLayout = transform.Find("WeaponPanel").GetComponent<BasicSlotLayoutViewController>();
		skillSlotLayout = transform.Find("SkillPanel").GetComponent<BasicSlotLayoutViewController>();
		emptyHotbarSlotCountDict.Add(ResourceCategory.Weapon, 0);
		emptyHotbarSlotCountDict.Add(ResourceCategory.Skill, 0);
		occupiedGeneralSlotCountDict.Add(ResourceCategory.Weapon, 0);
		occupiedGeneralSlotCountDict.Add(ResourceCategory.Skill, 0);
		maxHotbarSlotCountDict.Add(ResourceCategory.Weapon, 0);
		maxHotbarSlotCountDict.Add(ResourceCategory.Skill, 0);
		
		remainingSkillSlotCountText = transform.Find("RemainingSkillSpaceText").GetComponent<TMP_Text>();
		remainingWeaponSlotCountText = transform.Find("RemainingWeaponSpaceText").GetComponent<TMP_Text>();
		inventorySystem = this.GetSystem<IInventorySystem>();
		errorText = transform.Find("ErrorText").gameObject;
		enterButton = transform.Find("EnterButton").gameObject;
		
		enterButton.GetComponent<Button>().onClick.AddListener(OnEnterButtonClicked);
		
	}

	

	public override void OnOpen(UIMsg msg) {
		//canClose = false;
		CalculateEmptySlotCount();
		
		
		weaponSlotLayout.OnShowItems(new HashSet<ResourceSlot>().Union(inventoryModel.GetBaseStock(ResourceCategory.Weapon)).ToList());
		skillSlotLayout.OnShowItems(new HashSet<ResourceSlot>()
			.Union(inventoryModel.GetBaseStock(ResourceCategory.Skill)).ToList());
		
		weaponSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
		skillSlotLayout.RegisterOnSlotClicked(OnSlotClicked);
		
		UpdateRemainingSlotCount();
	}

	private void CalculateEmptySlotCount() {
		
		var allGeneralSlots = inventoryModel.GetAllSlots();
		foreach (var generalSlot in allGeneralSlots) {
			if (generalSlot.IsEmpty()) {
				maxGeneralSlotCount++;
			}
		}

		var allWeaponSlots = inventoryModel.GetHotBarSlots(HotBarCategory.Right);
		foreach (var weaponSlot in allWeaponSlots) {
			if (weaponSlot.IsEmpty()) {
				emptyHotbarSlotCountDict[ResourceCategory.Weapon]++;
				maxHotbarSlotCountDict[ResourceCategory.Weapon]++;
			}
		}
		
		var allSkillSlots = inventoryModel.GetHotBarSlots(HotBarCategory.Left);
		foreach (var skillSlot in allSkillSlots) {
			if (skillSlot.IsEmpty()) {
				emptyHotbarSlotCountDict[ResourceCategory.Skill]++;
				maxHotbarSlotCountDict[ResourceCategory.Skill]++;
			}
		}
	}
	private void OnSlotClicked(ResourceSlotViewController slotVC, BasicSlotLayoutViewController layout, bool originallySelected) {
		ResourceSlot slot = slotVC.Slot;

		if (originallySelected)
		{
			AudioSystem.Singleton.Play2DSound("item_deselect");
			layout.SetSelected(slotVC, false);
			ResourceCategory category = layout == weaponSlotLayout ? ResourceCategory.Weapon : ResourceCategory.Skill;
			
			if (occupiedGeneralSlotCountDict[category] > 0) {
				occupiedGeneralSlotCountDict[category]--;
			}
			else {
				emptyHotbarSlotCountDict[category]++;
			}
		}
		else {
			ResourceCategory category = layout == weaponSlotLayout ? ResourceCategory.Weapon : ResourceCategory.Skill;
			if (emptyHotbarSlotCountDict[category] <= 0 && occupiedGeneralSlotCountDict[ResourceCategory.Weapon] + occupiedGeneralSlotCountDict[ResourceCategory.Skill] >= maxGeneralSlotCount) {
				return;
			}
			
			if (emptyHotbarSlotCountDict[category] > 0) {
				emptyHotbarSlotCountDict[category]--;
			}
			else {
				occupiedGeneralSlotCountDict[category]++;
			}
			
			AudioSystem.Singleton.Play2DSound("item_select");
			layout.SetSelected(slotVC, true);
		}
		
		UpdateRemainingSlotCount();

	}

	private void UpdateRemainingSlotCount() {
		int totalWeaponSlotCount = maxHotbarSlotCountDict[ResourceCategory.Weapon] + maxGeneralSlotCount;
		int totalSkillSlotCount = maxHotbarSlotCountDict[ResourceCategory.Skill] + maxGeneralSlotCount;

		int emptyGeneralSlotCount = maxGeneralSlotCount - occupiedGeneralSlotCountDict[ResourceCategory.Weapon] -
		                            occupiedGeneralSlotCountDict[ResourceCategory.Skill];
		
		int remainingWeaponSlotCount = emptyHotbarSlotCountDict[ResourceCategory.Weapon] + emptyGeneralSlotCount;
		int remainingSkillSlotCount = emptyHotbarSlotCountDict[ResourceCategory.Skill] + emptyGeneralSlotCount;
		
		remainingWeaponSlotCountText.text = Localization.GetFormat("BASE_PREPARE_WEAPON_REMAIN", remainingWeaponSlotCount, totalWeaponSlotCount);
		remainingSkillSlotCountText.text = Localization.GetFormat("BASE_PREPARE_SKILL_REMAIN", remainingSkillSlotCount, totalSkillSlotCount);
		
		bool hasAtLeastOneWeapon = occupiedGeneralSlotCountDict[ResourceCategory.Weapon] > 0 ||
		                           emptyHotbarSlotCountDict[ResourceCategory.Weapon] < maxHotbarSlotCountDict[ResourceCategory.Weapon];
		
		errorText.SetActive(!hasAtLeastOneWeapon);
		enterButton.SetActive(hasAtLeastOneWeapon);
	}

	public override void OnClosed() {
		weaponSlotLayout.UnRegisterOnSlotClicked(OnSlotClicked);
		skillSlotLayout.UnRegisterOnSlotClicked(OnSlotClicked);

		weaponSlotLayout.ClearLayout();
		skillSlotLayout.ClearLayout();
		
		occupiedGeneralSlotCountDict[ResourceCategory.Weapon] = 0;
		occupiedGeneralSlotCountDict[ResourceCategory.Skill] = 0;
		emptyHotbarSlotCountDict[ResourceCategory.Weapon] = 0;
		emptyHotbarSlotCountDict[ResourceCategory.Skill] = 0;
		
		
		maxGeneralSlotCount = 0;
		maxHotbarSlotCountDict[ResourceCategory.Weapon] = 0;
		maxHotbarSlotCountDict[ResourceCategory.Skill] = 0;
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
	
	private void OnEnterButtonClicked() {
		AudioSystem.Singleton.Play2DSound("interact_button");

		LoadingCanvas.Singleton.Show(() => {
			canClose = true;
			List<PreparationSlot> slots = weaponSlotLayout.ClearLayout();
			foreach (PreparationSlot slot in slots) {
				inventorySystem.MoveItemFromBaseStockToInventory(ResourceCategory.Weapon, slot);
			}

			slots = skillSlotLayout.ClearLayout();
			foreach (PreparationSlot slot in slots) {
				inventorySystem.MoveItemFromBaseStockToInventory(ResourceCategory.Skill, slot);
			}

			
			MainUI.Singleton.GetAndClose(this);
			this.SendCommand<NextLevelCommand>(NextLevelCommand.Allocate());
		});
		
	}

	public IPanel GetClosePanel() {
		if (canClose) {
			return this;
		}

		return null;
	}
}
