using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Trading;
using AYellowpaper.SerializedCollections;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPartsTradingAllocatePanel :  AbstractPanel, IController {
	[SerializeField] private WeaponPartsTradingPreviewPanel previewPanel;
	[SerializeField] private Transform previewPanelSpawnParent;
	[SerializeField] private GameObject fullyUpgradedText;
	
	[SerializeField] private TMP_Text remainingCostText;
	[SerializeField] private Button confirmButton;
	[SerializeField] private Button closeButton;
	[SerializeField] private TMP_Text confirmButtonText;
	[SerializeField] private TMP_Text titleText;
	
	[SerializeField] private GameObject sliderGroup;

	[SerializeField] [SerializedDictionary("CurrencyType", "Slider")]
	private SerializedDictionary<CurrencyType, CurrencySelectSlider> currencySliders;
	private ICurrencyModel currencyModel;
	private IWeaponPartsEntity currentlyPreviewedEntity;
	private  IWeaponPartsEntity tradedEntity;
	private bool isExchange;
	public override void OnInit() {
		currencyModel = this.GetModel<ICurrencyModel>();
		closeButton.onClick.AddListener(OnClosePanel);

		foreach (KeyValuePair<CurrencyType,CurrencySelectSlider> keyValuePair in currencySliders) {
			CurrencySelectSlider slider = keyValuePair.Value;
			slider.currentlySelectedCurrency.RegisterOnValueChanged(OnCurrencySliderValueChanged);
		}
		
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
	}

	private void OnConfirmButtonClicked() {
		Dictionary<CurrencyType, int> paidCost = new Dictionary<CurrencyType, int>();
		foreach (KeyValuePair<CurrencyType,CurrencySelectSlider> keyValuePair in currencySliders) {
			CurrencySelectSlider slider = keyValuePair.Value;
			paidCost.Add(keyValuePair.Key, slider.currentlySelectedCurrency.Value);
		}

		this.SendCommand<TradeWeaponPartCommand>(TradeWeaponPartCommand.Allocate(tradedEntity, currentlyPreviewedEntity,
			isExchange, paidCost));

		if (isExchange) {
			OnClosePanel();
		}
	}

	private void OnCurrencySliderValueChanged(int arg1, int arg2) {
		RecalculateCost();
	}

	private void RecalculateCost() {
		if(currentlyPreviewedEntity == null) return;

		int totalPay = 0;
		int requiredCost = isExchange
			? currentlyPreviewedEntity.GetInGamePurchaseCostOfLevel(currentlyPreviewedEntity.GetRarity())
			: currentlyPreviewedEntity.GetUpgradeCostOfLevel(currentlyPreviewedEntity.GetRarity());
		
		
		bool moneyEnough = false;
		foreach (KeyValuePair<CurrencyType,CurrencySelectSlider> keyValuePair in currencySliders) {
			CurrencySelectSlider slider = keyValuePair.Value;
			totalPay += slider.currentlySelectedCurrency.Value;
			
			if (totalPay >= requiredCost) {
				moneyEnough = true;
			}
		}
		
		confirmButton.gameObject.SetActive(moneyEnough);
		remainingCostText.text = Localization.GetFormat("PARTS_REMAINING_COST", Mathf.Max(0, requiredCost - totalPay).ToString());
		
		foreach (KeyValuePair<CurrencyType,CurrencySelectSlider> keyValuePair in currencySliders) {
			CurrencySelectSlider slider = keyValuePair.Value;
			slider.SetAddButtonActive(totalPay < requiredCost);
		}
	}

	private void OnClosePanel() {
		UIManager.Singleton.ClosePanel(this);
	}

	public override void OnOpen(UIMsg msg) {
		
	}

	public void OnRefresh(IWeaponPartsEntity tradedEntity, IWeaponPartsEntity previewedEntity, bool isExchange) {
		titleText.text = confirmButtonText.text = isExchange
			? Localization.Get("PARTS_TRADING_TITLE_EXCHANGE")
			: Localization.Get("PARTS_TRADING_TITLE_UPGRADE");
		
		sliderGroup.SetActive(true);
		fullyUpgradedText.gameObject.SetActive(false);
		previewPanelSpawnParent.gameObject.SetActive(true);
		remainingCostText.gameObject.SetActive(true);
		
		confirmButton.gameObject.SetActive(false);
		currentlyPreviewedEntity = previewedEntity;
		this.tradedEntity = tradedEntity;
		this.isExchange = isExchange;
		
		if ((tradedEntity.GetRarity() == tradedEntity.GetMaxRarity() && !isExchange) || previewedEntity == null) { //special case
			fullyUpgradedText.gameObject.SetActive(true);
			previewPanelSpawnParent.gameObject.SetActive(false);
			sliderGroup.SetActive(false);
			remainingCostText.gameObject.SetActive(false);
			return;
		}

		foreach (KeyValuePair<CurrencyType,CurrencySelectSlider> keyValuePair in currencySliders) {
			CurrencyType currencyType = keyValuePair.Key;
			CurrencySelectSlider slider = keyValuePair.Value;

			
			int maxCurrency = currencyModel.GetCurrencyAmountProperty(currencyType);
			slider.ResetInfo(maxCurrency);
		}
		
		
		previewPanel.ShowPreviewPanel(previewedEntity, isExchange, false);
	}

	public override void OnClosed() {
		
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
}
