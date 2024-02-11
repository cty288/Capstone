﻿using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Test {
	public class TestWeaponParts2 : WeaponPartsEntity<TestWeaponParts2, TestWeaponPartBuff2> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TestWeaponParts2";

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		
		public override bool Collectable { get; } = true;
		protected override void OnInitModifiers(int rarity) {
			
		}

		

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}

		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return "Test Weapon Parts 2";
		}

		public override WeaponPartType WeaponPartType { get; } = WeaponPartType.Magazine;
	}

	public class TestWeaponPartBuff2 : WeaponPartsBuff<TestWeaponParts2, TestWeaponPartBuff2> {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = 0.5f;

		
		private BuffedProperties<Vector2Int> baseDamageProperties;

		public override void OnInitialize() {
		
			baseDamageProperties = new BuffedProperties<Vector2Int>(weaponEntity, true, BuffTag.Weapon_BaseDamage);
		}

		public override void OnStart() {

			foreach (IBuffedProperty<Vector2Int> buffedProperty in baseDamageProperties.Properties) {
				buffedProperty.RealValue.Value += new Vector2Int(30, 80);
			}
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			foreach (IBuffedProperty<Vector2Int> buffedProperty in baseDamageProperties.Properties) {
				buffedProperty.RealValue.Value -= new Vector2Int(30, 80);
			}
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return new BuffedProperties[] {
				baseDamageProperties
			};
		}


		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						"This is an additional description from TestWeaponPartBuff2");
				})
			};
		}
	}
}