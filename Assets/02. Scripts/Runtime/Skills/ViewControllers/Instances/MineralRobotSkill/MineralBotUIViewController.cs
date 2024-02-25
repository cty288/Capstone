using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Pillars.Commands;
using _02._Scripts.Runtime.Rewards;
using _02._Scripts.Runtime.Skills.Model.Instance.MineralRobotSkill;
using _02._Scripts.Runtime.Skills.ViewControllers.Instances.MineralRobotSkill;
using AYellowpaper.SerializedCollections;
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
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MineralBotUIViewController : AbstractPanelContainer, IController, IGameUIPanel
{
	
	[SerializeField]
	private Button confirmButton;

	[SerializeField] private Toggle[] currencyToggles;
	[SerializeField] private Toggle offToggle;
	[SerializeField] private Slider lifeSlider;
	[SerializeField] private TMP_Text lifeSliderText;

	private MineralBotCollectResource selectedResource;
	private MineralRobotEntity mineralRobotEntity;
	

	public override void OnInit() {

		foreach (Toggle currencyToggle in currencyToggles) {
			currencyToggle.onValueChanged.AddListener((isOn) => {
				CurrencySelectElement element = currencyToggle.GetComponent<CurrencySelectElement>();
				CurrencyType currencyType = element.CurrencyType;
				if (isOn) {
					if (currencyToggle != offToggle) {
						SelectCurrency(currencyType.ConvertToMineralBotCollectResource());
					}
					else {
						SelectCurrency(MineralBotCollectResource.None);
					}
					
				}
			});
		}
		
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
	}

	private void OnConfirmButtonClicked() {
		mineralRobotEntity.CollectResourceType.Value = selectedResource;
		MainUI.Singleton.GetAndClose(this);
	}
	
	
	
	
	

	private void Update() {
		if (ClientInput.Singleton.GetSharedActions().Interact.WasPressedThisFrame()) {
			MainUI.Singleton.GetAndClose(this);
		}
	}

	

	public override void OnOpen(UIMsg msg) {
		var data = (MineralRobotUIMsg) msg;
		mineralRobotEntity = data.entity;
		if (mineralRobotEntity == null) {
			MainUI.Singleton.GetAndClose(this);
			return;
		}

		MineralBotCollectResource resource = mineralRobotEntity.CollectResourceType.Value;
		foreach (var currencyToggle in currencyToggles) {
			currencyToggle.isOn = false;
			if (currencyToggle == offToggle) {
				currencyToggle.isOn = resource == MineralBotCollectResource.None;
			}
			else {
				CurrencySelectElement element = currencyToggle.GetComponent<CurrencySelectElement>();
				if (element.CurrencyType.ConvertToMineralBotCollectResource() == resource) {
					currencyToggle.isOn = true;
				}
			}
		}
		
		
		int maxCount = mineralRobotEntity.Limit.Value;
		int remainCount = maxCount - mineralRobotEntity.CollectedCount.Value;
		lifeSliderText.text = remainCount.ToString();
		lifeSlider.value = (float) remainCount / maxCount;

		SelectCurrency(resource);
		
	}

	public override void OnClosed() {
		
	}
	
	public void SelectCurrency(MineralBotCollectResource resource) {
		selectedResource = resource;
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
