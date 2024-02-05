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
		
		protected IDamageable entity;
		protected BindableProperty<HealthInfo> boundHealthProperty;
		private ResLoader resLoader;
		private Transform buffSpawnParent;
   
		private Dictionary<IBuff, BuffIconViewController> buffToGameObject = new Dictionary<IBuff, BuffIconViewController>();
		private void Awake() {
			bar = transform.Find("Mask/Fill Area/Fill").GetComponent<Image>();
			healthBGMaterial = Instantiate(bar.material);
			bar.material = healthBGMaterial;
			nameText = transform.Find("NameText").GetComponent<TMP_Text>();
			rarityBar = transform.Find("RarityBar");
			
			resLoader = this.GetUtility<ResLoader>();
			buffSpawnParent = transform.Find("BuffPanel");
		}
		
		private void ClearRarityIndicator() {
			for (int i = 0; i < rarityBar.childCount; i++) {
				Destroy(rarityBar.GetChild(i).gameObject);
			}
		}

		public override void OnSetEntity(BindableProperty<HealthInfo> healthProperty, IDamageable entity) {
			ClearRarityIndicator();
			this.entity = entity;
			this.boundHealthProperty = healthProperty;
			boundHealthProperty.RegisterWithInitValue(OnHealthChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			this.entity.RegisterOnBuffUpdate(OnBuffUpdate);
			
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

		private void OnBuffUpdate(IBuff buff, BuffUpdateEventType updateType) {
			BuffDisplayInfo buffDisplayInfo = buff.OnGetBuffDisplayInfo();
			switch (updateType) {
				case BuffUpdateEventType.OnStart:
					BuffIconViewController buffIcon = Instantiate(resLoader.LoadSync<GameObject>(buffDisplayInfo.IconPrefab))
						.GetComponent<BuffIconViewController>();
            
					buffIcon.transform.SetParent(buffSpawnParent);
					buffIcon.transform.localScale = Vector3.one;
					
					//get height of the spawn parent
					float height = buffSpawnParent.GetComponent<RectTransform>().rect.height;
					//set width/height of the buff icon
					buffIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(height, height);
					
					buffIcon.SetBuff(buff);
					buffToGameObject.Add(buff, buffIcon);
					buffIcon.OnRefresh();
					break;
				case BuffUpdateEventType.OnUpdate:
					if (buffToGameObject.TryGetValue(buff, out var value)) {
						value.OnRefresh();
					}
            
					break;
				case BuffUpdateEventType.OnEnd:
					if (buffToGameObject.TryGetValue(buff, out var buffIconViewController)) {
						buffToGameObject.Remove(buff);
						Destroy(buffIconViewController.gameObject);
					}
					break;
			}
		}

		public override void OnHealthBarDestroyed() {
			boundHealthProperty.UnRegisterOnValueChanged(OnHealthChanged);
			entity.UnregisterOnBuffUpdate(OnBuffUpdate);
			
			
			foreach (var buffIcon in buffToGameObject.Values) {
				Destroy(buffIcon.gameObject);
			}
			
			buffToGameObject.Clear();
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