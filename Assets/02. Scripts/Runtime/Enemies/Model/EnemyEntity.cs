using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Enemies.Model {
	public interface IEnemyEntity : ICreature, IHaveCustomProperties, IHaveTags {
		public BindableProperty<int> GetDanger();
		public BindableProperty<HealthInfo> GetHealth();
		public BindableList<TasteType> GetTaste();
		//public BindableProperty<float> GetVigiliance();

		//public BindableProperty<float> GetAttackRange();
	
		public int GetRarity();
	}

	public abstract class EnemyEntity<T> : AbstractCreature, IEnemyEntity, IHaveTags where T : EnemyEntity<T>, new() {
		protected IDangerProperty dangerProperty;
		protected IHealthProperty healthProperty;
		protected ITasteProperty tasteProperty;
		
		protected override void OnEntityRegisterAdditionalProperties() {
			
			RegisterInitialProperty<IDangerProperty>(new Danger());
			RegisterInitialProperty<ITasteProperty>(new Taste());
			//RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
			//RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
			
			OnEnemyRegisterAdditionalProperties();
		}
		
		

		protected override void OnEntityStart() {
			base.OnEntityStart();
			dangerProperty = GetProperty<IDangerProperty>();
			healthProperty = GetProperty<IHealthProperty>();
			tasteProperty = GetProperty<ITasteProperty>();
		}

		protected override Faction GetDefaultFaction() {
			return Faction.Hostile;
		}

		public BindableProperty<int> GetDanger() {
			return this.dangerProperty.RealValue;
		}

		public BindableProperty<HealthInfo> GetHealth() {
			return this.healthProperty.RealValue;
		}

		public BindableList<TasteType> GetTaste() {
			return this.tasteProperty.RealValues;
		}

		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.EnemyEntityConfigTable;
		}
	

		protected abstract void OnEnemyRegisterAdditionalProperties();

		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
		}


	}
}