﻿using MikroFramework.Singletons;

namespace _02._Scripts.Runtime.Utilities.ConfigSheet {
	public class ConfigDatas : MikroSingleton<ConfigDatas> {
		private ConfigDatas(){}

		private ConfigTable enemyEntityConfigTable;

		public ConfigTable EnemyEntityConfigTable => enemyEntityConfigTable;
		public override void OnSingletonInit() {
			base.OnSingletonInit();
			enemyEntityConfigTable = new ConfigTable("11NQVroaWnwS4dTw0O7kHkJP-LuJmcF4TZFLSFrbjJYE",
				"1697603466", "data_enemies");
		}
	}
}