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

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.LongBarrel {
		public class LongBarrel  : WeaponPartsEntity<LongBarrel, LongBarrelBuff> {
		public override string EntityName { get; set; } = "LongBarrel";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int displayMultiplayer = (int) (multiplayer * 100);
			
			float distance = GetCustomDataValueOfCurrentLevel<float>("distance");
			int displayDistance = (int) (distance);

			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer, displayDistance);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class LongBarrelBuff : WeaponPartsBuff<LongBarrel, LongBarrelBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;

		private float multiplier;
		private float distance;
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		private HitData OnModifyHitData(HitData data, IWeaponEntity weapon) {
			if (data.Hurtbox == null) {
				return data;
			}

			Transform attacker = data.Attacker?.RootViewController?.GetTransform();
			Transform target = data.Hurtbox?.Transform;
			if (attacker == null || target == null) {
				return data;
			}
			
			float distance = Vector3.Distance(attacker.position, target.position);
			if (distance < this.distance) {
				return data;
			}

			data.Damage = Mathf.RoundToInt(data.Damage * (1 + multiplier));
			return data;
		}

		public override void OnStart() {
			multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
			distance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("distance");
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
					int displayDistance = (int) (distance);
					int displayMultiplayer = (int) (multiplier * 100);

					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						Localization.GetFormat("LongBarrel_desc", displayMultiplayer, displayDistance));
				})
			};
		}
	}
}