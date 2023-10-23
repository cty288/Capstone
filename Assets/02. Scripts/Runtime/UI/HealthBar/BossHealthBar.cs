using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

public class BossHealthBar : HealthBar {
	private Scrollbar bar;
	
	[SerializeField] private Color healthyColor = Color.green;
	[SerializeField] private Color hurtColor = Color.red;
	[SerializeField] private GameObject rarityIndicatorPrefab;

	private Image healthBG;
	private Material healthBGMaterial;
	
	private TMP_Text bossNameText;
	private IDamageable entity;
	private BindableProperty<HealthInfo> boundHealthProperty;

	private Transform rarityBar;
	

	private void Awake() {
		bar = transform.Find("BarParent").GetComponent<Scrollbar>();
		healthBG = transform.Find("BarParent/Handle/Sliding Area/Handle").GetComponent<Image>();
		rarityBar = transform.Find("RarityBar");
		healthBGMaterial = Instantiate(healthBG.material);
		healthBG.material = healthBGMaterial;
		bossNameText = transform.Find("NameText").GetComponent<TMP_Text>();
	}

	private void ClearRarityIndicator() {
		for (int i = 0; i < rarityBar.childCount; i++) {
			Destroy(rarityBar.GetChild(i).gameObject);
		}
	}
	public override void OnSetEntity(BindableProperty<HealthInfo> boundHealthProperty, IDamageable entity) {
		ClearRarityIndicator();
		this.entity = entity;
		this.boundHealthProperty = boundHealthProperty;
		boundHealthProperty.RegisterWithInitValue(OnHealthChanged)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);

		bossNameText.text = "";
		if (!String.IsNullOrEmpty(entity.GetDisplayName())) {
			bossNameText.text = entity.GetDisplayName();
		}

		if (rarityIndicatorPrefab && entity.TryGetProperty(new PropertyNameInfo(PropertyName.rarity), out var rarityProperty)) {
			if (rarityProperty is IRarityProperty rarity) {
				for (int i = 0; i < rarity.RealValue.Value; i++) {
					Instantiate(rarityIndicatorPrefab, rarityBar);
				}
			}
		}
	}
	
	

	public override void OnHealthBarDestroyed() {
		boundHealthProperty.UnRegisterOnValueChanged(OnHealthChanged);
		ClearRarityIndicator();
	}

	private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
		float healthPercentage = (float) newHealth.CurrentHealth / newHealth.MaxHealth;
		DOTween.To(() => bar.size, x => bar.size = x, 
			healthPercentage, 0.2f);
		
		//lerp material color
		healthBGMaterial.DOColor(Color.Lerp(hurtColor, healthyColor, healthPercentage), 0.2f);

	}
}
