using System;
using Framework;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Builder;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;
using Runtime.RawMaterials.ViewControllers;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace Runtime.RunTimeTests.TestMaterial {
	

	public class TestBasicRawMaterial : RawMaterialEntity<TestBasicRawMaterial> {
		public override string EntityName { get; set; } = "TestRaw";

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
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
	
	public class TestPickableRawMaterial : AbstractPickableRawMaterialViewController<TestBasicRawMaterial> {
		private void Update() {
			if (Input.GetKeyDown(KeyCode.S)) {
				ES3AutoSaveMgr.Current.Save();
				((MainGame) MainGame.Interface).SaveGame();
			}
		}
		
		

		protected override void OnBindEntityProperty() {
			
		}

		protected override IEntity OnInitResourceEntity(RawMaterialBuilder<TestBasicRawMaterial> builder) {
			return builder.FromConfig().AddBaitAdjective(BaitAdjective.Test_Adjective_3).Build();
		}

		protected override void OnStartAbsorb() {
			
		}
	}
}