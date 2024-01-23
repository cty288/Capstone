using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.AdaptedCompensator {
	public class AdaptedCompensator : WeaponPartsEntity<AdaptedCompensator, AdaptedCompensatorBuff> {
		public override string EntityName { get; set; } = "AdaptedCompensator";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int displayMultiplayer = (int) (multiplayer * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class AdaptedCompensatorBuff : WeaponPartsBuff<AdaptedCompensator, AdaptedCompensatorBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;

		private float multiplier;
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		private HitData OnModifyHitData(HitData data) {
			if (data.Hurtbox == null) {
				return data;
			}

			if (data.Hurtbox.DamageMultiplier > 1) {
				data.Damage = Mathf.RoundToInt(data.Damage * (1 + multiplier));
			}

			return data;
		}

		public override void OnStart() {
			multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
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

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters() {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int percentage = (int) (multiplier * 100);
					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						Localization.GetFormat("AdaptedCompensator_BUFF_PROPERTY", percentage));
				})
			};
		}
	}
}