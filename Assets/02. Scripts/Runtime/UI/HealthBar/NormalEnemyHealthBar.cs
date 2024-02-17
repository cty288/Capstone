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
using Runtime.Enemies.Model;
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
		[SerializeField] private GameObject eliteIndicatorPrefab;
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

			if (entity is INormalEnemyEntity normalEnemy) {
				normalEnemy.IsElite.RegisterOnValueChanged(OnEliteChanged)
					.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			}
			
			
			this.boundHealthProperty = healthProperty;
			boundHealthProperty.RegisterWithInitValue(OnHealthChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			
			
			nameText.text = "";
			if (!String.IsNullOrEmpty(entity.GetDisplayName())) {
				nameText.text = entity.GetDisplayName();
			}
			
			if (rarityIndicatorPrefab && entity.TryGetProperty(new PropertyNameInfo(PropertyName.rarity), out var rarityProperty)) {
				if (rarityProperty is IRarityProperty rarity) {
					float parentHeight = rarityBar.GetComponent<RectTransform>().rect.height;
					for (int i = 0; i < rarity.RealValue.Value; i++) {
						RectTransform transfrorm = Instantiate(rarityIndicatorPrefab, rarityBar).GetComponent<RectTransform>();
						transfrorm.sizeDelta = new Vector2(parentHeight, parentHeight);
					}
				}
			}
		}

		private void OnEliteChanged(bool arg1, bool isElite) {
			ClearRarityIndicator();
			GameObject prefab = isElite ? eliteIndicatorPrefab : rarityIndicatorPrefab;
			float parentHeight = rarityBar.GetComponent<RectTransform>().rect.height;

			if (isElite) {
				RectTransform transfrorm = Instantiate(prefab, rarityBar).GetComponent<RectTransform>();
				//transfrorm.sizeDelta = new Vector2(parentHeight, parentHeight);
			}
			else {
				if (entity.TryGetProperty(new PropertyNameInfo(PropertyName.rarity), out var rarityProperty)) {
					if (rarityProperty is IRarityProperty rarity) {
						for (int i = 0; i < rarity.RealValue.Value; i++) {
							RectTransform transfrorm = Instantiate(rarityIndicatorPrefab, rarityBar)
								.GetComponent<RectTransform>();
							transfrorm.sizeDelta = new Vector2(parentHeight, parentHeight);
						}
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