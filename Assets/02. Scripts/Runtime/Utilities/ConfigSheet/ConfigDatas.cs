using MikroFramework.Singletons;
using Polyglot;
using UnityEngine;

namespace Runtime.Utilities.ConfigSheet
{
	public class ConfigDatas : MikroSingleton<ConfigDatas>
	{
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
        
		public override void OnSingletonInit()
		{
			base.OnSingletonInit();
			//Debug.Log("ConfigDatas Singleton Init");
			bossEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1697603466", "data_enemies");

			enemyEntityConfigTable_test = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1757713118", "data_enemies_test");

			rawMaterialEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1100243990", "data_raw_material");

			rawMaterialEntityConfigTable_test = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1292551269", "data_raw_material_test");
			
			weaponEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"2055187826", "data_weapons");
			
			weaponEntityConfigTable_test = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1228926880", "data_weapons_test");
			
			playerEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"231118994", "data_player");
			
			normalEnemyEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"642644287", "data_normal_enemies");
			
			globalDataTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1266085510", "data_global");
			
			skillEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"509422807", "data_skills");
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