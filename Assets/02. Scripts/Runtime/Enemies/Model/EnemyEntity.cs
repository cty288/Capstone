using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Faction;
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
		protected override void OnEntityRegisterAdditionalProperties() {
			
			RegisterInitialProperty<IDangerProperty>(new Danger());
			RegisterInitialProperty<ITasteProperty>(new Taste());
			//RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
			//RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
			
			OnEnemyRegisterAdditionalProperties();
		}

		protected override Faction GetDefaultFaction() {
			return Faction.Hostile;
		}

		public BindableProperty<int> GetDanger() {
			return GetProperty<IDangerProperty>().RealValue;
		}

		public BindableProperty<HealthInfo> GetHealth() {
			return GetProperty<IHealthProperty>().RealValue;
		}

		public BindableList<TasteType> GetTaste() {
			return GetProperty<ITasteProperty>().RealValues;
		}

		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.EnemyEntityConfigTable;
		}

		public int GetRarity() {
			return GetProperty<IRarityProperty>().RealValue;
		}


		protected abstract void OnEnemyRegisterAdditionalProperties();

		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
		}


	}
}