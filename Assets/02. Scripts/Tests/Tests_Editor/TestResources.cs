using System.Collections.Generic;
using Framework;
using NUnit.Framework;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Instances;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Tests.Tests_Editor {
	
	internal class TestBasicRawMaterial : RawMaterialEntity<TestBasicRawMaterial> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TestRaw";
		public override bool Collectable { get; } = false;
		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}
		public override IResourceEntity GetReturnToBaseEntity() {
			return this;
		}
		public override void OnRecycle() {
            
		}
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.RawMaterialEntityConfigTable_Test;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
                
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.RawMaterial;
		}
	}
	
	internal class TestEmptyRawMaterial : RawMaterialEntity<TestBasicRawMaterial> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TestRaw2";

		public override bool Collectable { get; } = false;

		public override IResourceEntity GetReturnToBaseEntity() {
			return this;
		}
		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}
		public override void OnRecycle() {
            
		}
		
		protected override void OnInitModifiers(int rarity) {
                
		}
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.RawMaterialEntityConfigTable_Test;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}


		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.RawMaterial;
		}
	}
	
	//=======================================================================================
	
	public class TestResources {
		[Test]
		public void TestBasicRawMaterialProperties() {
			IRawMaterialModel model = MainGame_Test.Interface.GetModel<IRawMaterialModel>();


			TestBasicRawMaterial ent = model.GetRawMaterialBuilder<TestBasicRawMaterial>()
				.FromConfig()
				.Build();
			
			Assert.AreEqual(5, ent.GetRarity());
			Assert.IsTrue(ent.GetDisplayName() == "???" || ent.GetDisplayName() == "Test Raw Material");
			Assert.AreEqual(20, ent.GetMaxStackProperty().RealValue.Value);
			Assert.AreEqual(2, ent.GetBaitAdjectivesProperty().RealValues.Count);
			
			ES3.Save("test_save_raw_material_1", ent, "test_save");
			model.RemoveEntity(ent.UUID);
            
			ent = ES3.Load<TestBasicRawMaterial>("test_save_raw_material_1", "test_save");
			ent.OnLoadFromSave();
			
			Assert.IsNotNull(ent);
            
			Assert.AreEqual(5, ent.GetRarity());
			Assert.IsTrue(ent.GetDisplayName() == "???" || ent.GetDisplayName() == "Test Raw Material");
			Assert.AreEqual(20, ent.GetMaxStackProperty().RealValue.Value);
			Assert.AreEqual(2, ent.GetBaitAdjectivesProperty().RealValues.Count);
            
            
			ES3.DeleteKey("test_save_raw_material_1", "test_save");
		}
		
		
		[Test]
		public void TestEmptyRawMaterialProperties() {
			IRawMaterialModel model = MainGame_Test.Interface.GetModel<IRawMaterialModel>();


			TestEmptyRawMaterial ent = model.GetRawMaterialBuilder<TestEmptyRawMaterial>()
				.FromConfig()
				.Build();
			
			Assert.AreEqual(10, ent.GetRarity());
			Assert.AreEqual("???", ent.GetDisplayName());
			Assert.AreEqual(20, ent.GetMaxStackProperty().RealValue.Value);
			Assert.AreEqual(0, ent.GetBaitAdjectivesProperty().RealValues.Count);
			
			ES3.Save("test_save_raw_material_2", ent, "test_save");
			model.RemoveEntity(ent.UUID);
            
			ent = ES3.Load<TestEmptyRawMaterial>("test_save_raw_material_2", "test_save");
			ent.OnLoadFromSave();
			
			Assert.IsNotNull(ent);
            
			Assert.AreEqual(10, ent.GetRarity());
			Assert.AreEqual("???", ent.GetDisplayName());
			Assert.AreEqual(20, ent.GetMaxStackProperty().RealValue.Value);
			Assert.AreEqual(0, ent.GetBaitAdjectivesProperty().RealValues.Count);
            
            
			ES3.DeleteKey("test_save_raw_material_2", "test_save");
		}
		
		
		[Test]
		public void TestCommonRawMaterialEntityData() {
			IRawMaterialModel model = MainGame_Test.Interface.GetModel<IRawMaterialModel>();


			CommonRawMaterialEntity ent = model.GetRawMaterialBuilder<CommonRawMaterialEntity>()
				.OverrideName("TestRaw")
				.FromConfig(ConfigDatas.Singleton.RawMaterialEntityConfigTable_Test)
				.Build();
			
			Assert.AreEqual(5, ent.GetRarity());
			Assert.AreEqual("???", ent.GetDisplayName());
			Assert.AreEqual(20, ent.GetMaxStackProperty().RealValue.Value);
			Assert.AreEqual(2, ent.GetBaitAdjectivesProperty().RealValues.Count);
			
			ES3.Save("test_save_raw_material_1", ent, "test_save");
			model.RemoveEntity(ent.UUID);
            
			ent = ES3.Load<CommonRawMaterialEntity>("test_save_raw_material_1", "test_save");
			ent.OnLoadFromSave();
			
			Assert.IsNotNull(ent);
            
			Assert.AreEqual(5, ent.GetRarity());
			Assert.AreEqual("???", ent.GetDisplayName());
			Assert.AreEqual(20, ent.GetMaxStackProperty().RealValue.Value);
			Assert.AreEqual(2, ent.GetBaitAdjectivesProperty().RealValues.Count);
            
            
			ES3.DeleteKey("test_save_raw_material_1", "test_save");
		}
	}
}