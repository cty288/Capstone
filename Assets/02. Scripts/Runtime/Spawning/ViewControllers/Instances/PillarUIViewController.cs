using System.Collections;
using System.Collections.Generic;
using System.Text;
using _02._Scripts.Runtime.Currency.Model;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.Spawning.Commands;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;

public class PillarUIViewController : AbstractPanel, IController {
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
		this.SendCommand(PillarSpawnBossCommand.Allocate(data.pillar, GetRequiredCurrency(), currentSelectedRarity));
		MainUI.Singleton.OpenOrClose<PillarUIViewController>(MainUI.Singleton, null);
	}
	public void SetRarity(int rarity) {
		currentSelectedRarity = rarity;
		displayRarityText.text = rarity.ToString();

		StringBuilder sb = new StringBuilder();
		bool canSummon = true;
		Dictionary<CurrencyType, int> requiredCurrency = GetRequiredCurrency();
		
		foreach (CurrencyType currencyType in data.bossSpawnCosts.Keys) {
			sb.Append($"{Localization.Get($"CURRENCY_{currencyType.ToString()}_name")}: ");
			int currencyAmount = requiredCurrency[currencyType];
			bool isEnough = currencyModel.GetCurrencyAmountProperty(currencyType) >= currencyAmount;
			if (!isEnough) {
				canSummon = false;
			}
			sb.Append(isEnough
				? $"<color=black>{currencyAmount}</color>"
				: $"<color=#FF0000>{currencyAmount}</color>");
			sb.Append("\n");
		}

		displayCostText.text = sb.ToString();

		summonButton.interactable = canSummon;
	}
	
	private Dictionary<CurrencyType, int> GetRequiredCurrency() {
		Dictionary<CurrencyType, int> requiredCurrency = new Dictionary<CurrencyType, int>();
		foreach (CurrencyType currencyType in data.bossSpawnCosts.Keys) {
			requiredCurrency.Add(currencyType, data.bossSpawnCosts[currencyType].Cost * currentSelectedRarity);
		}

		return requiredCurrency;
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
}
