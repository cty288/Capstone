using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Pillars.Commands;
using AYellowpaper.SerializedCollections;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.Controls;
using Runtime.Spawning.Commands;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public interface IGameUIPanel {
	public IPanel GetClosePanel();
}
public class PillarUIViewController : AbstractPanel, IController, IGameUIPanel {
	private OnOpenPillarUI data;

	[SerializedDictionary("CurrencyType", "Icon")]
	[SerializeField]
	private SerializedDictionary<CurrencyType, Sprite> currencySprites;
	
	[SerializeField]
	private Button confirmButton;

	[SerializeField] private Slider currencySlider;
	[SerializeField] private TMP_Text currencySliderText;
	[SerializeField] private PressHoldButton addCurrencyButton;
	[SerializeField] private PressHoldButton minusCurrencyButton;
	[SerializeField] private Image currencyIcon;
	[SerializeField] private TMP_Text requiredEnergyText;
	[SerializeField] private TMP_Text levelText;

	
	private ICurrencyModel currencyModel;
	private int currentSelectedCurrency = 0;
	private int maxCurrencyPossible = 100;
	private ICurrencySystem currencySystem;
	public override void OnInit() {
		
		currencyModel = this.GetModel<ICurrencyModel>();
		currencySystem = this.GetSystem<ICurrencySystem>();
		addCurrencyButton.RegisterCallback(OnAddCurrencyButtonClicked);
		minusCurrencyButton.RegisterCallback(OnMinusCurrencyButtonClicked);

		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		this.RegisterEvent<OnCurrencyAmountChangedEvent>(OnCurrencyAmountChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		
	}

	private void OnConfirmButtonClicked() {
		currencySystem.RemoveCurrency(data.pillarCurrencyType, currentSelectedCurrency);
		this.SendCommand(ActivatePillarCommand.Allocate(data.pillarCurrencyType, currentSelectedCurrency,
			data.rewardCosts.GetLevel(currentSelectedCurrency)));
	}

	private void OnMinusCurrencyButtonClicked() {
		SelectCurrency(currentSelectedCurrency - 1);
	}

	private void OnAddCurrencyButtonClicked() {
		SelectCurrency(currentSelectedCurrency + 1);
	}

	private void OnCurrencyAmountChanged(OnCurrencyAmountChangedEvent e) {
		if (IsOpening) {
			if (e.CurrencyType == data.pillarCurrencyType)
				SelectCurrency(currentSelectedCurrency);
		}
	
	}

	private void Update() {
		if (ClientInput.Singleton.GetSharedActions().Interact.WasPressedThisFrame()) {
			MainUI.Singleton.GetAndClose(this);
		}
	}

	

	public override void OnOpen(UIMsg msg) {
		data = (OnOpenPillarUI) msg;
		currencyIcon.sprite = currencySprites[data.pillarCurrencyType];
		maxCurrencyPossible = data.rewardCosts.GetHighestCost() * 2;
		SelectCurrency(0);
		
	}

	public override void OnClosed() {
		
	}
	private void OnSummonButtonClicked() {
		//this.SendCommand(PillarSpawnBossCommand.Allocate(data.pillar, GetRequiredCurrency(), currentSelectedRarity));
		//MainUI.Singleton.OpenOrGetClose<PillarUIViewController>(MainUI.Singleton, null);
	}
	public void SelectCurrency(int currency) {
		if (currency < 0) {
			currency = 0;
		}else if (currency > maxCurrencyPossible) {
			currency = maxCurrencyPossible;
		}
		CurrencyType currencyType = data.pillarCurrencyType;
		currentSelectedCurrency = currency;
		currencySliderText.text = currency.ToString() + $"\n<sprite index={(int)currencyType}>";

		float ratio = (float) currency / maxCurrencyPossible;
		currencySlider.value = ratio;

		
		bool isEnough = currencyModel.GetCurrencyAmountProperty(currencyType) >= currency;
		
		int levelNumber = data.rewardCosts.GetLevel(currency);
		levelText.text = Localization.GetFormat("PILLAR_LEVEL_TEXT", levelNumber);
		levelText.color = Color.black;
		
		isEnough = isEnough && levelNumber > 0;
		
		bool canSummon = isEnough;
		
		requiredEnergyText.text = Localization.GetFormat("PILLAR_HINT_ERROR", (int) currencyType);
		requiredEnergyText.gameObject.SetActive(false);
		levelText.gameObject.SetActive(true);
		
		if (currency <= 0 || levelNumber <= 0) {
			canSummon = false;
			requiredEnergyText.gameObject.SetActive(true);
			levelText.text = Localization.Get("PILLAR_LEVEL_TEXT_ERROR");
			levelText.color = Color.red;
			
		}

		if (currencyModel.GetCurrencyAmountProperty(currencyType) < currency) {
			requiredEnergyText.gameObject.SetActive(true);
			requiredEnergyText.text = Localization.GetFormat("PILLAR_HINT_ERROR_2", (int) currencyType);
		}

		currencySliderText.color = isEnough ? Color.black : Color.red;
		confirmButton.interactable = canSummon;
	}
	

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}

	public IPanel GetClosePanel() {
		//TODO: can't close when selecting rewards
		return this;
	}
}
