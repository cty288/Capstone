using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Instance.MineralRobotSkill;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using DG.Tweening;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.UI.NameTags;
using Runtime.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;


public class MineralBotNameTag : GeneralNameTag {
	private Transform rarityBar;
	[SerializeField] private GameObject rarityIndicatorPrefab;
	[SerializeField] private TMP_Text resourceText;
	[SerializeField] private TMP_Text remainResourceText;
	[SerializeField] private Image fillImage;
	
	private MineralRobotEntity targetEntity;
	protected override void Awake() {
		base.Awake();
		rarityBar = transform.Find("RarityBar");
	}
	
	private void ClearRarityIndicator() {
		for (int i = 0; i < rarityBar.childCount; i++) {
			Destroy(rarityBar.GetChild(i).gameObject);
		}
	}

	public override void SetEntity(IEntity entity) {
		base.SetEntity(entity);
		ClearRarityIndicator();

		targetEntity = entity as MineralRobotEntity;
		if (rarityIndicatorPrefab && entity.TryGetProperty(new PropertyNameInfo(PropertyName.rarity), 
			    out var rarityProperty)) {
			if (rarityProperty is IRarityProperty rarity) {
				float height = rarityBar.GetComponent<RectTransform>().rect.height;
				for (int i = 0; i < rarity.RealValue.Value; i++) {
					GameObject spawnedRarity = Instantiate(rarityIndicatorPrefab, rarityBar);
					spawnedRarity.GetComponent<RectTransform>().sizeDelta = new Vector2(height, height);
				}
			}
		}

		targetEntity?.CollectResourceType.RegisterWithInitValue(OnCollectResourceChanged)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		targetEntity?.CollectedCount.RegisterWithInitValue(OnResourceCollectCountChanged)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
	}

	private void OnResourceCollectCountChanged(int arg1, int count) {
		int maxCount = targetEntity.Limit.Value;
		int remainCount = maxCount - targetEntity.CollectedCount.Value;
		remainResourceText.text = remainCount.ToString();

		fillImage.DOFillAmount((float) remainCount / maxCount, 0.3f);
	}

	private void OnCollectResourceChanged(MineralBotCollectResource arg1, MineralBotCollectResource resource) {
		if (resource == MineralBotCollectResource.None) {
			resourceText.text = Localization.Get("MineralRobotName_NOT_COLLECTING");
		}
		else {
			CurrencyType currencyType = resource.ConvertToCurrencyType();
			resourceText.text = $"<sprite index={(int) currencyType}>";
		}
	}
}
