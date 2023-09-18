using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : HealthBar {
	private Scrollbar bar;

	private void Awake() {
		bar = GetComponent<Scrollbar>();
	}

	public override void OnSetEntity(IDamageable entity) {
		entity.HealthProperty.RealValue.RegisterWithInitValue(OnHealthChanged)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
	}

	private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
		DOTween.To(() => bar.size, x => bar.size = x, 
			(float) newHealth.CurrentHealth / newHealth.MaxHealth, 0.2f);
	}
}
