﻿using Framework;
using NUnit.Framework;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.RawMaterials.Model.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Base;

namespace Tests.Tests_Editor {
	internal class TestBasicWeapon : WeaponEntity<TestBasicWeapon> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TestWeapon1";

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}
		public override void OnRecycle() {
            
		}
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.WeaponEntityConfigTable_Test;
		}


		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return new[] {
				new AutoConfigCustomProperty("explosion"),
				new AutoConfigCustomProperty("shield"),
			};
		}
	}
	
	//=====================================================================
	
	
	
	
	
	public class TestWeapons {
		[Test]
		public void TestWeaponProperties() {
			IWeaponModel model = MainGame_Test.Interface.GetModel<IWeaponModel>();


			TestBasicWeapon ent = model.GetWeaponBuilder<TestBasicWeapon>()
				.FromConfig()
				.Build();
			Assert.AreEqual(10, ent.GetBaseDamage().RealValue.Value);
			Assert.AreEqual(0.5f, ent.GetAttackSpeed().RealValue.Value);
			Assert.AreEqual(-60.0f, ent.GetCustomDataValue<dynamic>("shield", "angle").Value[0]);

			ES3.Save("test_save_weapon_1", ent, "test_save");
			model.RemoveEntity(ent.UUID);
            
			ent = ES3.Load<TestBasicWeapon>("test_save_weapon_1", "test_save");
			ent.OnLoadFromSave();
			
			Assert.IsNotNull(ent);
            
			Assert.AreEqual(10, ent.GetBaseDamage().RealValue.Value);
			Assert.AreEqual(0.5f, ent.GetAttackSpeed().RealValue.Value);
			Assert.AreEqual(-60.0f, ent.GetCustomDataValue<dynamic>("shield", "angle").Value[0]);
            
            
			ES3.DeleteKey("test_save_weapon_1", "test_save");
		}
	}
}