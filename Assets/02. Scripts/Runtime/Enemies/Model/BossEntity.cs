using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Enemies.Model {
	
	public interface IBossEntity : IEnemyEntity {
		public BindableList<TasteType> GetTaste();
	}
	public abstract class BossEntity<T> : EnemyEntity<T>, IBossEntity where T : BossEntity<T>, new()  {
		protected ITasteProperty tasteProperty;

		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<ITasteProperty>(new Taste());
			base.OnEntityRegisterAdditionalProperties();
		}
		
		public override void OnAwake() {
			base.OnAwake();
			tasteProperty = GetProperty<ITasteProperty>();
		}

		public BindableList<TasteType> GetTaste() {
			return this.tasteProperty.RealValues;
		}
		
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.BossEntityConfigTable;
		}

	}
}