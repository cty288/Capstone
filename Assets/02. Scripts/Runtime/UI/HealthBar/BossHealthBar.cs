using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : HealthBar {
	private Scrollbar bar;
	
	[SerializeField] private Color healthyColor = Color.green;
	[SerializeField] private Color hurtColor = Color.red;

	private Image healthBG;
	private Material healthBGMaterial;
	
	private TMP_Text bossNameText;
	private IDamageable entity;

	private void Awake() {
		bar = transform.Find("BarParent").GetComponent<Scrollbar>();
		healthBG = transform.Find("BarParent/Handle/Sliding Area/Handle").GetComponent<Image>();
		healthBGMaterial = Instantiate(healthBG.material);
		healthBG.material = healthBGMaterial;
		bossNameText = transform.Find("NameText").GetComponent<TMP_Text>();
	}

	public override void OnSetEntity(IDamageable entity) {
		this.entity = entity;
		entity.HealthProperty.RealValue.RegisterWithInitValue(OnHealthChanged)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);

		bossNameText.text = "";
		if (!String.IsNullOrEmpty(entity.GetDisplayName())) {
			bossNameText.text = entity.GetDisplayName();
		}
	}

	public override void OnHealthBarDestroyed() {
		entity.HealthProperty.RealValue.UnRegisterOnValueChanged(OnHealthChanged);
	}

	private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
		float healthPercentage = (float) newHealth.CurrentHealth / newHealth.MaxHealth;
		DOTween.To(() => bar.size, x => bar.size = x, 
			healthPercentage, 0.2f);
		
		//lerp material color
		healthBGMaterial.DOColor(Color.Lerp(hurtColor, healthyColor, healthPercentage), 0.2f);

	}
}
