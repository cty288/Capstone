using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Test {
	public class TestWeaponParts : WeaponPartsEntity<TestWeaponParts, TestWeaponPartBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TestWeaponParts";

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		

		protected override void OnInitModifiers(int rarity) {
			
		}

		
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return "Test Weapon Parts";
		}

		public override WeaponPartType WeaponPartType { get; } = WeaponPartType.Barrel;
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class TestWeaponPartBuff : WeaponPartsBuff<TestWeaponParts, TestWeaponPartBuff> {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = 0.5f;

		private BuffedProperties<float> attackSpeedProperties;
		private BuffedProperties<Vector2Int> baseDamageProperties;

		public override void OnInitialize() {
			attackSpeedProperties = new BuffedProperties<float>(weaponEntity, true, BuffTag.Weapon_AttackSpeed);
			baseDamageProperties = new BuffedProperties<Vector2Int>(weaponEntity, true, BuffTag.Weapon_BaseDamage);
		}

		public override void OnStart() {
			foreach (IBuffedProperty<float> buffedProperty in attackSpeedProperties.Properties) {
				buffedProperty.RealValue.Value +=
					weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("attackSpeed");
			}
			
			foreach (IBuffedProperty<Vector2Int> buffedProperty in baseDamageProperties.Properties) {
				buffedProperty.RealValue.Value += new Vector2Int(10, 50);
			}
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			foreach (IBuffedProperty<float> buffedProperty in attackSpeedProperties.Properties) {
				buffedProperty.RealValue.Value -= 
					weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("attackSpeed");
			}

			foreach (IBuffedProperty<Vector2Int> buffedProperty in baseDamageProperties.Properties) {
				buffedProperty.RealValue.Value -= new Vector2Int(10, 50);
			}
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return new BuffedProperties[] {
				attackSpeedProperties,
				baseDamageProperties
			};
		}


		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters() {
			return null;
		}
	}
}