using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using AYellowpaper.SerializedCollections;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct HazardLevelDisplayInfo {
	public Sprite bgSprite;
	public Color textColor;
}
public class HazardLevelIndicator : AbstractMikroController<MainGame> {
	[SerializedDictionary("Hazard Level", "Info")] [SerializeField]
	private SerializedDictionary<SubAreaDangerLevel, HazardLevelDisplayInfo> hazardLevelDisplayInfo;

	[SerializeField] private float updateInterval = 1f;
	private float updateTimer = 0f;
	
	private Image bgImage;
	private TMP_Text text;
	private GameObject contentPanel;
	
	private ILevelModel levelModel;
	private GlobalLevelManager levelManager;
	private Color targetColor;
	

	private void Awake() {
		contentPanel = transform.Find("Content").gameObject;
		bgImage = transform.Find("Content/BG").GetComponent<Image>();
		text = transform.Find("Content/Text").GetComponent<TMP_Text>();
		levelManager = GlobalLevelManager.Singleton;
		levelModel = this.GetModel<ILevelModel>();
		
		levelModel.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnCurrentLevelChanged(ILevelEntity oldLevel, ILevelEntity level) {
		UpdateUI();
		level?.IsInBossFight.RegisterWithInitValue(OnIsInBossFightChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
		oldLevel?.IsInBossFight.UnRegisterOnValueChanged(OnIsInBossFightChanged);
	}

	private void OnIsInBossFightChanged(bool arg1, bool arg2) {
		UpdateUI();
	}


	private void Update() {
		updateTimer += Time.deltaTime;
		if (updateTimer >= updateInterval) {
			updateTimer = 0f;
			UpdateUI();
		}

		text.color = Color.Lerp(text.color, targetColor, Time.deltaTime * 5f);
	}

	private void UpdateUI() {
		ILevelViewController currentLevelViewController = levelManager.CurrentLevelViewController;
		contentPanel.SetActive(false);
		if(currentLevelViewController == null) {
			return;
		}

		ISubAreaLevelEntity subArea = currentLevelViewController.GetCurrentActiveSubArea();
		if (subArea == null) {
			return;
		}
		
		if(levelModel.CurrentLevel?.Value.IsInBossFight) {
			return;
		}
		
		contentPanel.SetActive(true);
		SubAreaDangerLevel dangerLevel = subArea.GetSpawnStatus();
		HazardLevelDisplayInfo info = hazardLevelDisplayInfo[dangerLevel];
		bgImage.sprite = info.bgSprite;
		targetColor = info.textColor;
		text.text = Localization.Get($"HazardLevel_{dangerLevel.ToString()}");
	}
}