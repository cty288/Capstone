using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Pillars.Systems;
using AYellowpaper.SerializedCollections;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using DG.Tweening;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class PillarStatusPanel : AbstractMikroController<MainGame> {
	private Transform layoutGroup;
	private Dictionary<CurrencyType, PillarStatusElement> spawnedPillarStatusElements = new Dictionary<CurrencyType, PillarStatusElement>();
	private ILevelModel levelModel;
	
	[SerializeField]
	private GameObject pillarStatusElementPrefab;

	[SerializedDictionary("CurrencyType", "Color")] [SerializeField]
	private SerializedDictionary<CurrencyType, Color> currencyColors;
	
	[SerializeField]
	private Image skullImage;
	private Color skullColor;
	private void Awake() {
		layoutGroup = transform.Find("LayoutGroup");
		levelModel = this.GetModel<ILevelModel>();
		levelModel.CurrentLevel.RegisterOnValueChanged(OnCurrentLevelChanged)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		this.RegisterEvent<OnPillarActivated>(OnPillarActivated)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		this.RegisterEvent<OnBossSpawned>(OnBossSpawned)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		skullColor = skullImage.color;
		ClearLayoutGroup();
	}

	private void OnBossSpawned(OnBossSpawned obj) {
		ClearLayoutGroup();
	}

	private void OnPillarActivated(OnPillarActivated e) {
		if (spawnedPillarStatusElements.Count == 0) {
			layoutGroup.gameObject.SetActive(true);
			SpawnPillarStatusElements(e.Info);
		}
		
		foreach (var info in e.Info) {
			spawnedPillarStatusElements[info.Key].SetProgress(info.Value.level, info.Value.pillarCurrencyType,
				currencyColors[info.Value.pillarCurrencyType], info.Value.currencyPercentage);
		}

		if (e.isAllPillarsActivated) {
			skullImage.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo);
		}
	}

	private void SpawnPillarStatusElements(Dictionary<CurrencyType,PillarActivateInfo> currencyInfo) {
		foreach (var info in currencyInfo) {
			GameObject pillarStatusElement = Instantiate(pillarStatusElementPrefab, layoutGroup);

			pillarStatusElement.GetComponent<PillarStatusElement>().SetProgress(info.Value.level, info.Value.pillarCurrencyType,
				currencyColors[info.Value.pillarCurrencyType], info.Value.currencyPercentage);


			spawnedPillarStatusElements.Add(info.Value.pillarCurrencyType,
				pillarStatusElement.GetComponent<PillarStatusElement>());
		}
		skullImage.transform.parent.SetAsLastSibling();
	}

	private void ClearLayoutGroup() {
		foreach (var element in spawnedPillarStatusElements) {
			Destroy(element.Value.gameObject);
		}

		spawnedPillarStatusElements.Clear();
		skullImage.DOKill();
		skullImage.color = skullColor;
		layoutGroup.gameObject.SetActive(false);
	}
	
	
	private void OnCurrentLevelChanged(ILevelEntity e) {
		ClearLayoutGroup();
	}
}
