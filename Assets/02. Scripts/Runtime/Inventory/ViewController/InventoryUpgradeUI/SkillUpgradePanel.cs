using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Framework;
using MikroFramework.Architecture;
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
    
    public override void OnInit() {
       originalDescriptionPanel = transform.Find("Mask/UpgradeGroup/DescriptionTag").GetComponent<SlotResourceDescriptionPanel>();
       upgradedDescriptionPanel = transform.Find("Mask/UpgradeGroup/DescriptionTag_Upgraded").GetComponent<SlotResourceDescriptionPanel>();
       levelNumText = transform.Find("Mask/UpgradeGroup/Arrow/LevelNum").GetComponent<TMP_Text>(); 
       fullyUpgradedText = transform.Find("Mask/UpgradeGroup/FullyUpgradedText").gameObject;
       requiredCurrencyText = transform.Find("RequiredCurrencyPanel/Content").GetComponent<TMP_Text>();
       upgradeButton = transform.Find("UpgradeButton").GetComponent<Button>();
       requiredCurrencyPanel = transform.Find("RequiredCurrencyPanel").gameObject;
       currencyModel = this.GetModel<ICurrencyModel>();
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
        
        
        if (entity.GetLevel() >= entity.GetMaxLevel()) {
            fullyUpgradedText.SetActive(true);
        }
        else {
            requiredCurrencyPanel.SetActive(true);
            levelNumText.gameObject.SetActive(true);
            upgradedDescriptionPanel.gameObject.SetActive(true);

            levelNumText.text = Localization.GetFormat("PROPERTY_ICON_LEVEL", entity.GetLevel() + 1);
            ISkillEntity upgradedSkill =
                ResourceVCFactory.Singleton.SpawnNewResourceEntity(entity.EntityName, true, entity.GetLevel() + 1) as
                    ISkillEntity;
            
            SetDescriptionPanel(upgradedSkill, upgradedDescriptionPanel);
        }
    }

    protected void SetDescriptionPanel(ISkillEntity entity, SlotResourceDescriptionPanel panel) {
        panel.Clear();
        panel.SetContent(entity.GetDisplayName(), entity.GetDescription(),
            InventorySpriteFactory.Singleton.GetSprite(entity.IconSpriteName), true, entity.GetLevel(),
            ResourceVCFactory.GetLocalizedResourceCategory(entity.GetResourceCategory()),
            entity.GetResourcePropertyDescriptions());
    }

    public override void OnClosed() {
      
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }

    public IPanel GetClosePanel() {
        return this;
    }
}
