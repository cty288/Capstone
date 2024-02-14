using System;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;


namespace Runtime.UI.HealthBar {
	public class NormalEnemyHealthBar : global::HealthBar {
		protected Image bar;
		protected TMP_Text nameText;
	
		[SerializeField] private Color healthyColor = Color.red;
		[SerializeField] private Color hurtColor = Color.red;
		[SerializeField] private GameObject rarityIndicatorPrefab;
		private Transform rarityBar;
		
		protected Material healthBGMaterial;
		
		
		protected BindableProperty<HealthInfo> boundHealthProperty;
		
		
   
		
		protected override void Awake() {
			base.Awake();
			bar = transform.Find("Mask/Fill Area/Fill").GetComponent<Image>();
			healthBGMaterial = Instantiate(bar.material);
			bar.material = healthBGMaterial;
			nameText = transform.Find("NameText").GetComponent<TMP_Text>();
			rarityBar = transform.Find("RarityBar");
			
			
		
		}
		
		private void ClearRarityIndicator() {
			for (int i = 0; i < rarityBar.childCount; i++) {
				Destroy(rarityBar.GetChild(i).gameObject);
			}
		}

		protected override void OnSetEntity(BindableProperty<HealthInfo> healthProperty, IDamageable entity) {
			ClearRarityIndicator();
			
			this.boundHealthProperty = healthProperty;
			boundHealthProperty.RegisterWithInitValue(OnHealthChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			
			
			nameText.text = "";
			if (!String.IsNullOrEmpty(entity.GetDisplayName())) {
				nameText.text = entity.GetDisplayName();
			}
			
			if (rarityIndicatorPrefab && entity.TryGetProperty(new PropertyNameInfo(PropertyName.rarity), out var rarityProperty)) {
				if (rarityProperty is IRarityProperty rarity) {
					for (int i = 0; i < rarity.RealValue.Value; i++) {
						Instantiate(rarityIndicatorPrefab, rarityBar);
					}
				}
			}
		}

		
		protected override void OnHealthBarDestroyed() {
			boundHealthProperty.UnRegisterOnValueChanged(OnHealthChanged);
			entity.UnregisterOnBuffUpdate(OnBuffUpdate);
			ClearRarityIndicator();
		}

		private void OnHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
			float healthPercentage = (float) newHealth.CurrentHealth / newHealth.MaxHealth;
			bar.DOFillAmount(healthPercentage, 0.2f);
		
			//lerp material color
			healthBGMaterial.DOColor(Color.Lerp(hurtColor, healthyColor, healthPercentage), 0.2f);
		}
	}
}