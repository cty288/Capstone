using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialCompensator {
	public class SpecialCompensator  : WeaponPartsEntity<SpecialCompensator, SpecialCompensatorBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "SpecialCompensator";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int displayMultiplayer = (int) (multiplayer);
			
			float chance = GetCustomDataValueOfCurrentLevel<float>("chance");
			int displayChance = (int) (chance * 100);

			return Localization.GetFormat(defaultLocalizationKey, displayChance, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class SpecialCompensatorBuff : WeaponPartsBuff<SpecialCompensator, SpecialCompensatorBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		[ES3Serializable]
		private float multiplier;
		[ES3Serializable]
		private float chance;
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		private HitData OnModifyHitData(HitData data, IWeaponEntity weapon) {
			if (data.Hurtbox == null) {
				return data;
			}

			if (!data.IsCritical) {
				if (Random.value < chance) {
					data.Damage = Mathf.RoundToInt(data.Damage * multiplier);
					data.IsCritical = true;
				}
			}

			return data;
		}

		public override void OnStart() {
			base.OnStart();
			multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
			chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int displayChance = (int) (chance * 100);
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("SpecialCompensator_desc", displayChance, (int) multiplier));
				})
			};
		}
	}
}