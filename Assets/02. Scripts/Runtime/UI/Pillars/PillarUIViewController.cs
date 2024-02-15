using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Pillars.Commands;
using _02._Scripts.Runtime.Rewards;
using AYellowpaper.SerializedCollections;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.Controls;
using Runtime.Spawning.Commands;
using Runtime.Temporary;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public interface IGameUIPanel {
	public IPanel GetClosePanel();
}
public class PillarUIViewController : AbstractPanelContainer, IController, IGameUIPanel {
	private OnOpenPillarUI data;

	[SerializedDictionary("CurrencyType", "Icon")]
	[SerializeField]
	private SerializedDictionary<CurrencyType, Sprite> currencySprites;
	
	[SerializeField]
	private Button confirmButton;

	[SerializeField] private Toggle[] currencyToggles;
	
	[SerializeField] private Slider currencySlider;
	[SerializeField] private TMP_Text currencySliderText;
	[SerializeField] private PressHoldButton addCurrencyButton;
	[SerializeField] private PressHoldButton minusCurrencyButton;
	[SerializeField] private Image currencyIcon;
	[SerializeField] private TMP_Text requiredEnergyText;
	[SerializeField] private TMP_Text levelText;
	
	private ICurrencyModel currencyModel;
	private int currentSelectedCurrency = 0;
	private CurrencyType currentSelectedCurrencyType;
	private int maxCurrencyPossible = 100;
	private ICurrencySystem currencySystem;
	public override void OnInit() {
		
		currencyModel = this.GetModel<ICurrencyModel>();
		currencySystem = this.GetSystem<ICurrencySystem>();
		addCurrencyButton.RegisterCallback(OnAddCurrencyButtonClicked);
		minusCurrencyButton.RegisterCallback(OnMinusCurrencyButtonClicked);
		//nextCurrencyButton.onClick.AddListener(OnNextCurrencyButtonClicked);
		//previousCurrencyButton.onClick.AddListener(OnPreviousCurrencyButtonClicked);

		foreach (Toggle currencyToggle in currencyToggles) {
			currencyToggle.onValueChanged.AddListener((isOn) => {
				CurrencySelectElement element = currencyToggle.GetComponent<CurrencySelectElement>();
				CurrencyType currencyType = element.CurrencyType;
				if (isOn) {
					SelectCurrency(currencyType);
				}
			});
		}
		
		

		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		this.RegisterEvent<OnCurrencyAmountChangedEvent>(OnCurrencyAmountChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
		
	}

	private void OnConfirmButtonClicked() {
		OnConfirmButtonClickedAsync();
	}

	private void OnPreviousCurrencyButtonClicked() {
		int index = (int) currentSelectedCurrencyType;
		index--;
		if (index < 0) {
			index = Enum.GetValues(typeof(CurrencyType)).Length - 1;
		}
		SelectCurrency((CurrencyType) index);
	}

	private void OnNextCurrencyButtonClicked() {
		int index = (int) currentSelectedCurrencyType;
		index++;
		if (index >= Enum.GetValues(typeof(CurrencyType)).Length) {
			index = 0;
		}
		SelectCurrency((CurrencyType) index);
	}
	
	private void SelectCurrency(CurrencyType currencyType) {
		currentSelectedCurrencyType = currencyType;
		currencyIcon.sprite = currencySprites[currencyType];
		maxCurrencyPossible = data.rewardCosts[currentSelectedCurrencyType].GetHighestCost() * 2;
		SelectCurrency(0);
	}

	private async UniTask OnConfirmButtonClickedAsync() {
		currencySystem.RemoveCurrency(currentSelectedCurrencyType, currentSelectedCurrency);

		int level = data.rewardCosts[currentSelectedCurrencyType].GetLevel(currentSelectedCurrency);

		List<GameObject> spawnedResources = await RewardDisperser.Singleton.DisperseRewards(
			data.pillar.RewardsInfo.GetRewards(level),
			this, currentSelectedCurrencyType);

		Transform spawnTr = PlayerController.GetClosestPlayer(data.rewardSpawnPos.position).transform;
		Vector3 spawnPos = spawnTr.position;
		if(Physics.Raycast(spawnTr.position, spawnTr.forward, out RaycastHit hit, 10,
			   LayerMask.GetMask("Ground", "Default", "Wall"))) {

			spawnPos = hit.point;
		}

		foreach (var resource in spawnedResources) {
			resource.transform.position = spawnPos;
			resource.transform.rotation = Quaternion.identity;
			Rigidbody rb = resource.GetComponent<Rigidbody>();
			if (rb) {
				//45 degree upword, random direction
				//rb.AddForce(Quaternion.Euler(45, Random.Range(0, 360), 0) * Vector3.up * 5, ForceMode.Impulse);
			}
		}
		
		this.SendCommand(ActivatePillarCommand.Allocate(data.pillar, currentSelectedCurrencyType,
			currentSelectedCurrency,
			level));

		MainUI.Singleton.GetAndClose(this);
	}

	private void OnMinusCurrencyButtonClicked() {
		SelectCurrency(currentSelectedCurrency - 1);
	}

	private void OnAddCurrencyButtonClicked() {
		SelectCurrency(currentSelectedCurrency + 1);
	}

	private void OnCurrencyAmountChanged(OnCurrencyAmountChangedEvent e) {
		if (IsOpening) {
			if (e.CurrencyType == currentSelectedCurrencyType)
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
		//SelectCurrency(CurrencyType.Combat);
		currencyToggles[0].isOn = true;
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

		CurrencyType currencyType = currentSelectedCurrencyType;
		currentSelectedCurrency = currency;
		currencySliderText.text = currency.ToString();

		float ratio = (float) currency / maxCurrencyPossible;
		currencySlider.value = ratio;

		
		bool isEnough = currencyModel.GetCurrencyAmountProperty(currencyType) >= currency;
		
		int levelNumber = data.rewardCosts[currencyType].GetLevel(currency);
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
		IPanel openedChild = GetTopChild();
		if (openedChild == null) {
			return this;
		}

		return null;

	}
}
