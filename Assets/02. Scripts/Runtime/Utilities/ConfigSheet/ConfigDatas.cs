using MikroFramework.Singletons;

namespace Runtime.Utilities.ConfigSheet {
	public class ConfigDatas : MikroSingleton<ConfigDatas> {
		private ConfigDatas(){}

		private ConfigTable enemyEntityConfigTable;
		private ConfigTable enemyEntityConfigTable_test;

		public ConfigTable EnemyEntityConfigTable => enemyEntityConfigTable;
		public ConfigTable EnemyEntityConfigTable_Test => enemyEntityConfigTable_test;
		public override void OnSingletonInit() {
			base.OnSingletonInit();
			//Debug.Log("ConfigDatas Singleton Init");
			enemyEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1697603466", "data_enemies");
			
			enemyEntityConfigTable_test = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1757713118", "data_enemies_test");
		} 
	}
}