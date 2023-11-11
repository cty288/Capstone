using System;
using _02._Scripts.Runtime.Currency.Model;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Levels.Models.Properties {

	[Serializable]
	public struct LevelBossSpawnCostInfo {
		public CurrencyType CurrencyType;
		public int Cost;
	}
	
	public interface  ISpawnBossCost: IDictionaryProperty<CurrencyType, LevelBossSpawnCostInfo> {
		
	}
	public class SpawnBossCost : DictionaryProperty<CurrencyType, LevelBossSpawnCostInfo>, ISpawnBossCost {
		protected override PropertyName GetPropertyName() {
			return PropertyName.spawn_boss_cost;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return new[] {new PropertyNameInfo(PropertyName.rarity)};
		}
	}
}