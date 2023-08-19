using System;
using Framework;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Builder;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.GameResources.ViewControllers.Instances;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace Runtime.RunTimeTests.TestMaterial {
	

	public class TestBasicRawMaterial : RawMaterialEntity<TestBasicRawMaterial> {
		public override string EntityName { get; protected set; } = "TestRaw";

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}
		public override void OnRecycle() {
            
		}
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.RawMaterialEntityConfigTable_Test;
		}


		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	
	public class TestRawMaterial : AbstractRawMaterialViewController<TestBasicRawMaterial> {
		protected override void OnStart() {
			
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.S)) {
				ES3AutoSaveMgr.Current.Save();
				((MainGame) MainGame.Interface).SaveGame();
			}
		}

		protected override void OnDisplayNameUpdate(string displayName) {
			Debug.Log("display name update: " + displayName);
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override IEntity OnInitEnemyEntity(RawMaterialBuilder<TestBasicRawMaterial> builder) {
			return builder.FromConfig().AddBaitAdjective(BaitAdjective.Test_Adjective_3).Build();
		}
	}
}