using System.Collections.Generic;
using System.Numerics;
using _02._Scripts.Runtime.Baits.Model.Property;
using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;
using Vector2 = UnityEngine.Vector2;

namespace Runtime.Enemies.Model {
	
	public interface IBossEntity : IEnemyEntity {
		public bool IsBossSpawnConditionSatisfied(Dictionary<CurrencyType, float> currencyPercentage);
	}
	public abstract class BossEntity<T> : EnemyEntity<T>, IBossEntity where T : BossEntity<T>, new()  {
	

		private IBossSpawnConditionProperty bossSpawnConditionProperty;
		protected override void OnEntityRegisterAdditionalProperties() {
			//RegisterInitialProperty<ITasteProperty>(new Taste());
			//RegisterInitialProperty<IVigilianceProperty>(new Vigiliance());
			RegisterInitialProperty<IBossSpawnConditionProperty>(new BossSpawnCondition());
			base.OnEntityRegisterAdditionalProperties();
		}
		
		public override void OnAwake() {
			base.OnAwake();
			bossSpawnConditionProperty = GetProperty<IBossSpawnConditionProperty>();
		}

		public bool IsBossSpawnConditionSatisfied(Dictionary<CurrencyType, float> currencyPercentage) {
			foreach (var pair in bossSpawnConditionProperty.RealValue.Value) {
				if (!currencyPercentage.ContainsKey(pair.Key)) {
					continue;
				}
				Vector2 range = pair.Value;
				float percentage = currencyPercentage[pair.Key];
				if (percentage < range.x || percentage > range.y) {
					return false;
				}
			}
			
			return true;
		}


		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.BossEntityConfigTable;
		}
		
		

	}
}