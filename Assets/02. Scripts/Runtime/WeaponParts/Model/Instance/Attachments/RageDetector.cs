using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Attachments {
public class RageDetector : WeaponPartsEntity<RageDetector, RageDetectorBuff> {
		public override string EntityName { get; set; } = "RageDetector";
		
		public float multiplayer => GetCustomDataValueOfCurrentLevel<float>("multiplayer");
		public float healthBound => GetCustomDataValueOfCurrentLevel<float>("health_bound");

		
	
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int displayMultiplayer = (int) (multiplayer * 100);
			int displayHealthBound = (int) (healthBound * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayHealthBound, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class RageDetectorBuff : WeaponPartsBuff<RageDetector, RageDetectorBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyHitData(OnWeaponModifyHitData);
		}

		private HitData OnWeaponModifyHitData(HitData hit, IWeaponEntity weapon) {
			ICanDealDamageRootEntity rootEntity = weaponEntity.RootDamageDealer;
			if (rootEntity == null) {
				return hit;
			}

			if (rootEntity is IDamageable damageable) {
				int maxHealth = damageable.GetMaxHealth();
				int currentHealth = damageable.GetCurrentHealth();
				float healthPercentage = (float) currentHealth / maxHealth;
				if (healthPercentage <= weaponPartsEntity.healthBound) {
					hit.Damage = Mathf.RoundToInt(hit.Damage * (1 + weaponPartsEntity.multiplayer));
				}
			}

			return hit;
		}
		

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			weaponEntity.UnRegisterOnModifyHitData(OnWeaponModifyHitData);
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters() {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int displayMultiplayer = (int) (weaponPartsEntity.multiplayer * 100);
					int displayHealthBound = (int) (weaponPartsEntity.healthBound * 100);

					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						Localization.GetFormat("RageDetector_PEOPERTY_desc", displayHealthBound,  displayMultiplayer));
				})
			};
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}

}