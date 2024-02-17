using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Barrels.FatesEdgeBarrel {
	public class FatesEdgeBarrel : WeaponPartsEntity<FatesEdgeBarrel, FatesEdgeBarrelBuff> {
		public override string EntityName { get; set; } = "FatesEdgeBarrel";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			float healthThreshold = GetCustomDataValueOfCurrentLevel<float>("health");
			int displayMultiplayer = (int) (multiplayer * 100);
			int displayHealthThreshold = (int) (healthThreshold * 100);
			
			return Localization.GetFormat(defaultLocalizationKey, displayHealthThreshold, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class FatesEdgeBarrelBuff : WeaponPartsBuff<FatesEdgeBarrel, FatesEdgeBarrelBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		private HitData OnModifyHitData(HitData data, IWeaponEntity weapon) {
			if (data.Hurtbox == null) {
				return data;
			}

			if(data.Hurtbox.Owner.TryGetComponent<IDamageableViewController>(out var damageableViewController)) {
				IDamageable damageable = damageableViewController.DamageableEntity;
				if (damageable == null) {
					return data;
				}

				int maxHealth = damageable.GetMaxHealth();
				int currentHealth = damageable.GetCurrentHealth();
				float healthPercentage = (float) currentHealth / maxHealth;
				float healthThreshold = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("health");

				if (healthPercentage >= healthThreshold) {
					float multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
					data.Damage = Mathf.CeilToInt(data.Damage * (1 + multiplier));
				}
			}

			return data;
		}

		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					int healthThreshold = (int) (weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("health") * 100);
					int displayedMultiplier =
						(int) (weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier") * 100);
					
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("FatesEdgeBarrel_desc", healthThreshold, displayedMultiplier));
				})
			};
		}
	}
}