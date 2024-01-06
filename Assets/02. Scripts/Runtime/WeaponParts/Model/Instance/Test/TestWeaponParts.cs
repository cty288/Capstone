﻿using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Test {
	public class TestWeaponParts : WeaponPartsEntity<TestWeaponParts, TestWeaponPartBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TestWeaponParts";

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return new ICustomProperty[] {
				new AutoConfigCustomProperty("test")
			};
		}

		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return "Test Weapon Parts";
		}

		public override WeaponPartType WeaponPartType { get; } = WeaponPartType.Barrel;
	}

	public class TestWeaponPartBuff : WeaponPartsBuff<TestWeaponParts, TestWeaponPartBuff> {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = 0.5f;
		public override void OnInitialize() {
			
		}

		public override void OnStart() {
			weaponEntity.GetAttackSpeed().RealValue.Value += weaponPartsEntity.GetCustomDataValue<float>(
				"test", "attackSpeed");
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			weaponEntity.GetAttackSpeed().RealValue.Value -= weaponPartsEntity.GetCustomDataValue<int>(
				"test", "attackSpeed");
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}
		
		
	}
}