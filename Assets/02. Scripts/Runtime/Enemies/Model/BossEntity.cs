using _02._Scripts.Runtime.Baits.Model.Property;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties.TestOnly;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Enemies.Model {
	
	public interface IBossEntity : IEnemyEntity {
		public BindableList<TasteType> GetTaste();
		
		public BindableProperty<float> GetVigiliance();
	}
	public abstract class BossEntity<T> : EnemyEntity<T>, IBossEntity where T : BossEntity<T>, new()  {
		protected ITasteProperty tasteProperty;
		protected IVigilianceProperty vigilianceProperty;

		protected override void OnEntityRegisterAdditionalProperties() {
			RegisterInitialProperty<ITasteProperty>(new Taste());
			RegisterInitialProperty<IVigilianceProperty>(new Vigiliance());
			base.OnEntityRegisterAdditionalProperties();
		}
		
		public override void OnAwake() {
			base.OnAwake();
			tasteProperty = GetProperty<ITasteProperty>();
			vigilianceProperty = GetProperty<IVigilianceProperty>();
		}

		public BindableList<TasteType> GetTaste() {
			return this.tasteProperty.RealValues;
		}

		public BindableProperty<float> GetVigiliance() {
			return this.vigilianceProperty.RealValue;
		}


		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.BossEntityConfigTable;
		}
		
		

	}
}