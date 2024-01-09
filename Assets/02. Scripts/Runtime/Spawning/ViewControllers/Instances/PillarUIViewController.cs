using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using _02._Scripts.Runtime.Currency.Model;
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
	private Button lastRarityButton;
	private Button nextRarityButton;
	private TMP_Text displayRarityText;
	private Button summonButton;
	private TMP_Text displayCostText;
	private static int maxRarity = 4;
	private int currentSelectedRarity = 1;
	private ICurrencyModel currencyModel;
	public override void OnInit() {
		currencyModel = this.GetModel<ICurrencyModel>();
		lastRarityButton = transform.Find("SelectRarityPanel/ButtonLast").GetComponent<Button>();
		nextRarityButton = transform.Find("SelectRarityPanel/ButtonNext").GetComponent<Button>();
		displayRarityText = transform.Find("SelectRarityPanel/RarityNumber").GetComponent<TMP_Text>();
		displayCostText = transform.Find("RequiredCurrencyPanel/Content").GetComponent<TMP_Text>();
		summonButton = transform.Find("SummonButton").GetComponent<Button>();
		lastRarityButton.onClick.AddListener(OnLastRarityButtonClicked);
		nextRarityButton.onClick.AddListener(OnNextRarityButtonClicked);
		summonButton.onClick.AddListener(OnSummonButtonClicked);
		
		this.RegisterEvent<OnCurrencyAmountChangedEvent>(OnCurrencyAmountChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		
	}

	private void OnCurrencyAmountChanged(OnCurrencyAmountChangedEvent e) {
		if (IsOpening) {
			SetRarity(currentSelectedRarity);
		}
	
	}

	private void Update() {
		if (ClientInput.Singleton.GetSharedActions().Interact.WasPressedThisFrame()) {
			MainUI.Singleton.GetAndClose(this);
		}
	}

	private void OnLastRarityButtonClicked() { 
		int targetRarity = currentSelectedRarity <= 1 ? maxRarity : currentSelectedRarity - 1;
		SetRarity(targetRarity);
	}

	private void OnNextRarityButtonClicked() {
		int targetRarity = currentSelectedRarity >= maxRarity ? 1 : currentSelectedRarity + 1;
		SetRarity(targetRarity);
	}

	public override void OnOpen(UIMsg msg) {
		data = (OnOpenPillarUI) msg;
		SetRarity(1);
	}

	public override void OnClosed() {
		
	}
	private void OnSummonButtonClicked() {
		//this.SendCommand(PillarSpawnBossCommand.Allocate(data.pillar, GetRequiredCurrency(), currentSelectedRarity));
		MainUI.Singleton.OpenOrGetClose<PillarUIViewController>(MainUI.Singleton, null);
	}
	public void SetRarity(int rarity) {
		currentSelectedRarity = rarity;
		displayRarityText.text = rarity.ToString();

		StringBuilder sb = new StringBuilder();
		bool canSummon = true;
		
		int requiredCurrency = GetRequiredCurrency();

		int cost = data.rewardCosts.GetCostOfLevel(rarity);
		CurrencyType currencyType = data.pillarCurrencyType;
		
		sb.Append($"<sprite index={(int) currencyType}>");
		
		bool isEnough = currencyModel.GetCurrencyAmountProperty(currencyType) >= requiredCurrency;
		if (!isEnough) {
			canSummon = false;
		}
		sb.Append(isEnough
			? $"<color=black>{requiredCurrency}</color>"
			: $"<color=#FF0000>{requiredCurrency}</color>");
		sb.Append("    ");

		displayCostText.text = sb.ToString();

		summonButton.interactable = canSummon;
	}
	
	private int GetRequiredCurrency() {
		return data.rewardCosts.GetCostOfLevel(currentSelectedRarity);
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}

	public IPanel GetClosePanel() {
		return this;
	}
}
