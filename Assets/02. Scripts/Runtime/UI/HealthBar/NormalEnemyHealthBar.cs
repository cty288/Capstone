using System;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.HealthBar {
	public class NormalEnemyHealthBar : global::HealthBar {
		private Image bar;
		private TMP_Text nameText;
	
		[SerializeField] private Color healthyColor = Color.red;
		[SerializeField] private Color hurtColor = Color.red;
		
		private Material healthBGMaterial;
		
		private IDamageable entity;
		private IHealthProperty boundHealthProperty;

		private void Awake() {
			bar = transform.Find("Mask/Fill Area/Fill").GetComponent<Image>();
			healthBGMaterial = Instantiate(bar.material);
			bar.material = healthBGMaterial;
			nameText = transform.Find("NameText").GetComponent<TMP_Text>();
		}

		public override void OnSetEntity(IHealthProperty boundHealthProperty, IDamageable entity) {
			this.entity = entity;
			this.boundHealthProperty = boundHealthProperty;
			boundHealthProperty.RealValue.RegisterWithInitValue(OnHealthChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			nameText.text = "";
			if (!String.IsNullOrEmpty(entity.GetDisplayName())) {
				nameText.text = entity.GetDisplayName();
			}
		}

		public override void OnHealthBarDestroyed() {
			boundHealthProperty.RealValue.UnRegisterOnValueChanged(OnHealthChanged);
		}

		private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
			float healthPercentage = (float) newHealth.CurrentHealth / newHealth.MaxHealth;
			bar.DOFillAmount(healthPercentage, 0.2f);
		
			//lerp material color
			healthBGMaterial.DOColor(Color.Lerp(hurtColor, healthyColor, healthPercentage), 0.2f);
		}
	}
}