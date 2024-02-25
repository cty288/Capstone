using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Attachments {
	public class SunriseSurge : WeaponPartsEntity<SunriseSurge, SunriseSurgeBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "SunriseSurge";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("damage");
			int displayMultiplayer = (int) (multiplayer * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class SunriseSurgeBuff : WeaponPartsBuff<SunriseSurge, SunriseSurgeBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyDamageCount(OnWeaponEntityModifyDamageCount);
		}

		private int OnWeaponEntityModifyDamageCount(int damage) {
			ICanDealDamage owner = weaponEntity.GetRootDamageDealer();
			if (owner is IDamageable damagable && damagable.GetCurrentHealth() >= damagable.GetMaxHealth()) {
				float damageMultiplayer = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");
				return Mathf.RoundToInt(damage * (1 + damageMultiplayer));
			}

			return damage;
		}


		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		


		public override void OnRecycled() {
			base.OnRecycled();
			weaponEntity.UnregisterOnModifyDamageCount(OnWeaponEntityModifyDamageCount);
			
		}

		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					float damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");
					int displayDamage = (int) (damage * 100);
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("SunriseSurge_desc", displayDamage));
				})
			};
		}
	}
}