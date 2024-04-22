using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPartsTradingPreviewPanel : AbstractMikroController<MainGame>
{
    [Header("Preview Panel")]
    [SerializeField] private RectTransform rarityBar;
    [SerializeField] private GameObject rarityIconPrefab;
    [SerializeField] private GameObject nextLevelText;
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text itemCostText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text purchaseButtonText;
    [SerializeField] private GameObject fullyUpgradedText;
    [SerializeField] private Transform previewPanelSpawnParent;
    
    private ICurrencyModel currencyModel;

    private void Awake() {
        currencyModel = this.GetModel<ICurrencyModel>();
    }

    public void ResetPreviewPanel() {
        previewPanelSpawnParent.gameObject.SetActive(false);
        fullyUpgradedText.gameObject.SetActive(false);
        foreach (Transform child in rarityBar) {
            if (child.GetSiblingIndex() == 0) continue;
            Destroy(child.gameObject);
        }
    }
	
    public void ShowPreviewPanel(IWeaponPartsEntity entity, bool isExchange, bool showButton) {
        ResetPreviewPanel();
        previewPanelSpawnParent.gameObject.SetActive(true);
        nextLevelText.gameObject.SetActive(!isExchange);
        

        itemNameText.text = entity.GetDisplayName();
		
        //rarity bar
        float height = rarityBar.rect.height;
        for (int i = 0; i < entity.GetRarity(); i++) {
            GameObject star = Instantiate(rarityIconPrefab, rarityBar);
            RarityIndicator rarityIndicator = star.GetComponent<RarityIndicator>();
            rarityIndicator.SetCurrency(entity.GetBuildType());
            RectTransform starRect = star.GetComponent<RectTransform>();
            starRect.sizeDelta = new Vector2(height, height);
        }

        itemIconImage.sprite = InventorySpriteFactory.Singleton.GetSprite(entity);
        descriptionText.text = entity.GetDescription();

        int cost = isExchange
            ? entity.GetInGamePurchaseCostOfLevel(entity.GetRarity())
            : entity.GetUpgradeCostOfLevel(entity.GetRarity());

        int totalMoney = 0;
        foreach (var val in Enum.GetValues(typeof(CurrencyType))) {
            totalMoney += currencyModel.GetCurrencyAmountProperty((CurrencyType) val);
        }
        bool canPurchase = totalMoney >= cost;
        string color = canPurchase ? "<color=#00C612>" : "<color=red>";

        if (itemCostText) {
            itemCostText.text = Localization.GetFormat("PARTS_PREVIEW_COST", $"{color}{cost}</color>");
        }
       
        if (showButton) {
            purchaseButton.gameObject.SetActive(canPurchase);
            purchaseButtonText.text = isExchange
                ? Localization.Get("PARTS_TRADING_TITLE_EXCHANGE")
                : Localization.Get("PARTS_TRADING_TITLE_UPGRADE");
        }
        else {
            purchaseButton.gameObject.SetActive(false);
        }
       
    }

}
