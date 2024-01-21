using _02._Scripts.Runtime.Currency.Model;
using Runtime.DataFramework.Properties;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Enemies.Model.Properties {
	public interface IBossSpawnConditionProperty : IDictionaryProperty<CurrencyType, Vector2> {
		
	}
	
	
	public class BossSpawnCondition: LoadFromConfigDictProperty<CurrencyType, Vector2>, IBossSpawnConditionProperty  {
		protected override PropertyName GetPropertyName() {
			return PropertyName.boss_spawn_condition;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}
	}
}