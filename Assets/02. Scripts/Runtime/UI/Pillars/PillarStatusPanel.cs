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
using Runtime.Spawning;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class PillarStatusPanel : AbstractMikroController<MainGame> {
	private Transform layoutGroup;
	private Dictionary<IPillarEntity, PillarStatusElement> 
		spawnedPillarStatusElements = new Dictionary<IPillarEntity, PillarStatusElement>();
	
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
		this.RegisterEvent<OnPillarCurrencyReset>(OnPillarCurrencyReset)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		skullColor = skullImage.color;
		ClearLayoutGroup();
	}

	private void OnPillarCurrencyReset(OnPillarCurrencyReset e) {
		ClearLayoutGroup();
	}

	private void OnPillarActivated(OnPillarActivated e) {
		if (spawnedPillarStatusElements.Count == 0) {
			layoutGroup.gameObject.SetActive(true);
		}
		SpawnPillarStatusElements(e.Info);
		
		foreach (var info in e.Info) {
			spawnedPillarStatusElements[info.Key].SetProgress(info.Value.level, info.Value.pillarCurrencyType,
				currencyColors[info.Value.pillarCurrencyType], info.Value.currencyPercentage);
		}


		skullImage.transform.parent.gameObject.SetActive(e.isAllPillarsActivated);
		if (e.isAllPillarsActivated) {
			skullImage.DOColor(Color.red, 0.5f).SetLoops(-1, LoopType.Yoyo);
		}

		StartCoroutine(RebuildLayout(0.35f));
	}

	private IEnumerator RebuildLayout(float lastTime) {
		RectTransform rectTransform = layoutGroup.GetComponent<RectTransform>();
		
		float timer = 0;
		while (timer < lastTime) {
			timer += Time.deltaTime;
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			yield return null;
		}
		
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
	}

	private void SpawnPillarStatusElements(Dictionary<IPillarEntity,PillarActivateInfo> currencyInfo) {
		//find all entries in currencyInfo that are not in spawnedPillarStatusElements
		//spawn them
		foreach (var info in currencyInfo) {
			if (!spawnedPillarStatusElements.ContainsKey(info.Key)) {
				//if there are more than 1 spawned status element that has the same currency type, 
				//reposition the spawned status elements so that it is placed next to each other
				int currentSublingIndex = 0;
				int lastElementWithSameCurrencyTypeIndex = -1;
				foreach (Transform child in layoutGroup.transform) {
					if(child.TryGetComponent(out PillarStatusElement element)) {
						if (element.CurrencyType == info.Value.pillarCurrencyType) {
							lastElementWithSameCurrencyTypeIndex = currentSublingIndex;
						}
						currentSublingIndex++;
					}
				}

				lastElementWithSameCurrencyTypeIndex++;
				
				GameObject pillarStatusElement = Instantiate(pillarStatusElementPrefab, layoutGroup);

				pillarStatusElement.GetComponent<PillarStatusElement>().SetProgress(info.Value.level,
					info.Value.pillarCurrencyType,
					currencyColors[info.Value.pillarCurrencyType], info.Value.currencyPercentage);

				spawnedPillarStatusElements.Add(info.Key,
					pillarStatusElement.GetComponent<PillarStatusElement>());

				pillarStatusElement.transform.SetSiblingIndex(lastElementWithSameCurrencyTypeIndex);

			}
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
