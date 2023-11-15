using System.Collections;
using System.Collections.Generic;
using System.Text;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Commands;
using _02._Scripts.Runtime.Skills.Model.Base;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.GameResources;
using Runtime.Inventory;
using Runtime.Inventory.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradePanel : AbstractPanel, IController, IGameUIPanel {

    private SlotResourceDescriptionPanel originalDescriptionPanel;
    private SlotResourceDescriptionPanel upgradedDescriptionPanel;
    private TMP_Text levelNumText;
    private GameObject fullyUpgradedText;
    private TMP_Text requiredCurrencyText;
    private Button upgradeButton;
    private ICurrencyModel currencyModel;
    private GameObject requiredCurrencyPanel;
    private ISkillEntity spawnedUpgradedSkill;
    private ISkillModel skillModel;
    private ISkillEntity originalSkill;
    private RectTransform upgradeGroup;
    
    public override void OnInit() {
       originalDescriptionPanel = transform.Find("Mask/UpgradeGroup/DescriptionTag").GetComponent<SlotResourceDescriptionPanel>();
       upgradedDescriptionPanel = transform.Find("Mask/UpgradeGroup/DescriptionTag_Upgraded").GetComponent<SlotResourceDescriptionPanel>();
       levelNumText = transform.Find("Mask/UpgradeGroup/Arrow/LevelNum").GetComponent<TMP_Text>(); 
       fullyUpgradedText = transform.Find("Mask/UpgradeGroup/FullyUpgradedText").gameObject;
       requiredCurrencyText = transform.Find("RequiredCurrencyPanel/Content").GetComponent<TMP_Text>();
       upgradeButton = transform.Find("UpgradeButton").GetComponent<Button>();
       requiredCurrencyPanel = transform.Find("RequiredCurrencyPanel").gameObject;
       currencyModel = this.GetModel<ICurrencyModel>();
       skillModel = this.GetModel<ISkillModel>();
       upgradeButton.onClick.AddListener(OnUpgradeClicked);
       upgradeGroup = transform.Find("Mask/UpgradeGroup") as RectTransform;
       this.RegisterEvent<OnCurrencyAmountChangedEvent>(OnCurrencyAmountChanged)
           .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnCurrencyAmountChanged(OnCurrencyAmountChangedEvent e) {
        UpdateCurrency();
    }

    private void UpdateCurrency() {
        if (spawnedUpgradedSkill == null) {
            return;
        }
        StringBuilder sb = new StringBuilder();
        bool canSummon = true;
        Dictionary<CurrencyType, int> requiredCurrency = GetRequiredCurrency(spawnedUpgradedSkill);
		
        foreach (CurrencyType currencyType in requiredCurrency.Keys) {
            sb.Append($"<sprite index={(int) currencyType}>");
            int currencyAmount = requiredCurrency[currencyType];
            bool isEnough = currencyModel.GetCurrencyAmountProperty(currencyType) >= currencyAmount;
            if (!isEnough) {
                canSummon = false;
            }
            sb.Append(isEnough
                ? $"<color=white>{currencyAmount}</color>"
                : $"<color=#FF0000>{currencyAmount}</color>");
            sb.Append("    ");
        }

        requiredCurrencyText.text = sb.ToString();
        upgradeButton.interactable = canSummon;
        
        StartCoroutine(UpdateLayout());
    }

    private IEnumerator UpdateLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(upgradeGroup);
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(upgradeGroup);
    }


    public override void OnOpen(UIMsg msg) {
        OnOpenSkillUpgradePanel ev = (OnOpenSkillUpgradePanel) msg;
        ShowSkillUpgradePanel(ev.skillEntity);
    }


    protected void ShowSkillUpgradePanel(ISkillEntity entity) {
        SetDescriptionPanel(entity, originalDescriptionPanel);
        requiredCurrencyPanel.SetActive(false);
        upgradeButton.interactable = false;
        levelNumText.gameObject.SetActive(false);
        upgradedDescriptionPanel.gameObject.SetActive(false);
        fullyUpgradedText.SetActive(false);
        
        originalSkill = entity;
        
        if (entity.GetLevel() >= entity.GetMaxLevel()) {
            fullyUpgradedText.SetActive(true);
        }
        else {
            requiredCurrencyPanel.SetActive(true);
            levelNumText.gameObject.SetActive(true);
            upgradedDescriptionPanel.gameObject.SetActive(true);

            levelNumText.text = Localization.GetFormat("PROPERTY_ICON_LEVEL", entity.GetLevel() + 1);
            spawnedUpgradedSkill =
                ResourceVCFactory.Singleton.SpawnNewResourceEntity(entity.EntityName, true, entity.GetLevel() + 1) as
                    ISkillEntity;
            
            SetDescriptionPanel(spawnedUpgradedSkill, upgradedDescriptionPanel);
            
            
            UpdateCurrency();
        }
    }
    private void OnUpgradeClicked() {
        if (originalSkill == null) {
            return;
        }

        this.SendCommand<UpgradeSkillCommand>(UpgradeSkillCommand.Allocate(originalSkill,
            originalSkill.GetLevel() + 1, OnUpgradeSuccess));
        
    }

    private void OnUpgradeSuccess(ISkillEntity upgradedSkill) {
        originalSkill = null;
        if (spawnedUpgradedSkill != null) {
            skillModel.RemoveEntity(spawnedUpgradedSkill.UUID);
        }
        ShowSkillUpgradePanel(upgradedSkill);
    }

    private Dictionary<CurrencyType, int> GetRequiredCurrency(ISkillEntity entity) {
        Dictionary<CurrencyType, int> requiredCurrency = new Dictionary<CurrencyType, int>();
        Dictionary<CurrencyType, int> allCurrency = entity.GetSkillUpgradeCostOfCurrentLevel();
        foreach (CurrencyType currencyType in allCurrency.Keys) {
            if (allCurrency[currencyType] <= 0) {
                continue;
            }
            requiredCurrency.Add(currencyType, allCurrency[currencyType]);
        }

        return requiredCurrency;
    }

    protected void SetDescriptionPanel(ISkillEntity entity, SlotResourceDescriptionPanel panel) {
        panel.Clear();
        panel.SetContent(entity.GetDisplayName(), entity.GetDescription(),
            InventorySpriteFactory.Singleton.GetSprite(entity.EntityName), true, entity.GetLevel(),
            ResourceVCFactory.GetLocalizedResourceCategory(entity.GetResourceCategory()),
            entity.GetResourcePropertyDescriptions(), entity.GetSkillUseCostOfCurrentLevel());
    }

    public override void OnClosed() {
        if (spawnedUpgradedSkill != null) {
            skillModel.RemoveEntity(spawnedUpgradedSkill.UUID);
        }
        
        originalSkill = null;
        spawnedUpgradedSkill = null;
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }

    public IPanel GetClosePanel() {
        return this;
    }
}
