using System.Collections.Generic;
using MikroFramework.Singletons;
using Polyglot;
using UnityEngine;

namespace Runtime.Utilities.ConfigSheet
{
	public class ConfigDatas : MikroSingleton<ConfigDatas> {

		private Dictionary<string, string> versionDict = new Dictionary<string, string>() {
			{"develop", "11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE"},
			{"current", "1gQFTY2zuD_P7t-xrYqOckwoYCPp-KAAjcwzcKFCN6jI"}
		};
		private ConfigDatas() { }

		private ConfigTable bossEntityConfigTable;
		private ConfigTable enemyEntityConfigTable_test;
		private ConfigTable rawMaterialEntityConfigTable;
		private ConfigTable rawMaterialEntityConfigTable_test;
		private ConfigTable weaponEntityConfigTable;
		private ConfigTable weaponEntityConfigTable_test;
		private ConfigTable playerEntityConfigTable;
		private ConfigTable normalEnemyEntityConfigTable;
		private ConfigTable globalDataTable;
		private ConfigTable skillEntityConfigTable;
		private ConfigTable collectableResourceConfigTable;
		private ConfigTable weaponPartsConfigTable;

		public ConfigTable BossEntityConfigTable => bossEntityConfigTable;
		public ConfigTable EnemyEntityConfigTable_Test => enemyEntityConfigTable_test;
		
		public ConfigTable NormalEnemyEntityConfigTable => normalEnemyEntityConfigTable;

		public ConfigTable RawMaterialEntityConfigTable => rawMaterialEntityConfigTable;
		public ConfigTable RawMaterialEntityConfigTable_Test => rawMaterialEntityConfigTable_test;

		public ConfigTable WeaponEntityConfigTable => weaponEntityConfigTable;
		public ConfigTable WeaponEntityConfigTable_Test => weaponEntityConfigTable_test;
		
		public ConfigTable PlayerEntityConfigTable => playerEntityConfigTable;
		
		public ConfigTable GlobalDataTable => globalDataTable;

		public ConfigTable SkillEntityConfigTable => skillEntityConfigTable;
		
		public ConfigTable CollectableResourceConfigTable => collectableResourceConfigTable;
		
		public ConfigTable WeaponPartsConfigTable => weaponPartsConfigTable;

		private string GetDocID() {
			if (Application.isEditor) {
				return versionDict["develop"];
			}
			else {
				return versionDict["current"];
			}
		}
        
		public override void OnSingletonInit() {
			base.OnSingletonInit();
			string docID = GetDocID();
			
			globalDataTable = new ConfigTable(docID,
				"1266085510", "data_global", true);

			bool isDownload = (globalDataTable.Get<string>("LOCK_CONFIG_TABLE", "Value1") != "1");
			
			//Debug.Log("ConfigDatas Singleton Init");
			bossEntityConfigTable = new ConfigTable(docID,
				"1697603466", "data_enemies", isDownload);

			enemyEntityConfigTable_test = new ConfigTable(docID,
				"1757713118", "data_enemies_test", isDownload);

			rawMaterialEntityConfigTable = new ConfigTable(docID,
				"1100243990", "data_raw_material", isDownload);

			rawMaterialEntityConfigTable_test = new ConfigTable(docID,
				"1292551269", "data_raw_material_test", isDownload);
			
			weaponEntityConfigTable = new ConfigTable(docID,
				"2055187826", "data_weapons", isDownload);
			
			weaponEntityConfigTable_test = new ConfigTable(docID,
				"1228926880", "data_weapons_test", isDownload);
			
			playerEntityConfigTable = new ConfigTable(docID,
				"231118994", "data_player", isDownload);
			
			normalEnemyEntityConfigTable = new ConfigTable(docID,
				"642644287", "data_normal_enemies", isDownload);
			
			
			
			skillEntityConfigTable = new ConfigTable(docID,
				"509422807", "data_skills", isDownload);
			
			collectableResourceConfigTable = new ConfigTable(docID,
				"1707497374", "data_collectable_resources", isDownload);
			
			weaponPartsConfigTable = new ConfigTable(docID,
				"1363196473", "data_weapon_parts", isDownload);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void OnDownloadCustomSheet()
		{
			Debug.Log("Loading Localization Sheet...");
#if UNITY_EDITOR
			LocalizationInspector.DownloadCustomSheet();
#else
			var enumerator = LocalizationImporter.DownloadCustomSheet();
			while (enumerator.MoveNext()) {
				
			}
#endif

		}
	}
}