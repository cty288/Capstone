using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
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
    
    public override void OnInit() {
       originalDescriptionPanel = transform.Find("Mask/UpgradeGroup/DescriptionTag").GetComponent<SlotResourceDescriptionPanel>();
       upgradedDescriptionPanel = transform.Find("Mask/UpgradeGroup/DescriptionTag_Upgraded").GetComponent<SlotResourceDescriptionPanel>();
       levelNumText = transform.Find("Mask/UpgradeGroup/Arrow/LevelNum").GetComponent<TMP_Text>(); 
       fullyUpgradedText = transform.Find("Mask/UpgradeGroup/FullyUpgradedText").gameObject;
       requiredCurrencyText = transform.Find("RequiredCurrencyPanel/Content").GetComponent<TMP_Text>();
       upgradeButton = transform.Find("UpgradeButton").GetComponent<Button>();
       currencyModel = this.GetModel<ICurrencyModel>();
    }

    public override void OnOpen(UIMsg msg) {
        OnOpenSkillUpgradePanel ev = (OnOpenSkillUpgradePanel) msg;
        ShowSkillUpgradePanel(ev.skillEntity);
    }


    protected void ShowSkillUpgradePanel(ISkillEntity entity) {
        SetDescriptionPanel(entity, originalDescriptionPanel);
    }

    protected void SetDescriptionPanel(ISkillEntity entity, SlotResourceDescriptionPanel panel) {
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
