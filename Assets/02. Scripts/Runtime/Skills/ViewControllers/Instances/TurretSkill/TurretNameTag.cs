using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.UI.NameTags;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;


public class TurretNameTag : GeneralNameTag {
	private Transform rarityBar;
	[SerializeField] private GameObject rarityIndicatorPrefab;
	[SerializeField] private TMP_Text ammoText;
	[SerializeField] private  Image fillImage;
	
	private TurretEntity turretEntity;
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
		
		turretEntity = entity as TurretEntity;
		if (rarityIndicatorPrefab && entity.TryGetProperty(new PropertyNameInfo(PropertyName.rarity), out var rarityProperty)) {
			if (rarityProperty is IRarityProperty rarity) {
				for (int i = 0; i < rarity.RealValue.Value; i++) {
					Instantiate(rarityIndicatorPrefab, rarityBar);
				}
			}
		}
		
	}

	private void Update() {
		if (turretEntity == null || turretEntity.IsRecycled) {
			return;
		}
		
		float maxLastTime = turretEntity.GetCustomDataValue<float>("data", "last_time");
		float currentLastTime = turretEntity.LastTime.Value;
		fillImage.fillAmount = currentLastTime / maxLastTime;
		
		int maxAmmo = turretEntity.GetCustomDataValue<int>("data", "ammo_size");
		int currentAmmo = turretEntity.Ammo.Value;
		ammoText.text = $"{currentAmmo}/{maxAmmo}";

		if (currentLastTime == 0) {
			fillImage.fillAmount = 1;
		}
	}
}
