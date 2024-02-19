using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Barrels.EMPAdaptedBarrel {
	public class EMPAdaptedBarrel : WeaponPartsEntity<EMPAdaptedBarrel, EMPAdaptedBarrelBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "EMPAdaptedBarrel";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("damage");
			int displayMultiplayer = (int) (multiplayer * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer + "%");
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class EMPAdaptedBarrelBuff : WeaponPartsBuff<EMPAdaptedBarrel, EMPAdaptedBarrelBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		private BuffedProperties<Vector2Int> baseDamageProperties;
		[ES3Serializable]
		private Vector2Int addedDamage;
		public override void OnInitialize() {
			baseDamageProperties = new BuffedProperties<Vector2Int>(weaponEntity, true, BuffTag.Weapon_BaseDamage);
		}
		
		

		public override void OnStart() {
			base.OnStart();
			float multiplayer = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("damage");

			IBuffedProperty<Vector2Int> baseAttackSpeedProperty = baseDamageProperties.Properties.First();

			addedDamage = new Vector2Int(
				Mathf.RoundToInt((baseAttackSpeedProperty.BaseValue.x * multiplayer)),
				Mathf.RoundToInt((baseAttackSpeedProperty.BaseValue.y * multiplayer)));


			baseAttackSpeedProperty.RealValue.Value += addedDamage;
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			IBuffedProperty<Vector2Int> baseAttackSpeedProperty = baseDamageProperties.Properties.First();
			baseAttackSpeedProperty.RealValue.Value -= addedDamage;
		}


		public override void OnRecycled() {
			base.OnRecycled();
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return new[] {baseDamageProperties};
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return null;
		}
	}
}