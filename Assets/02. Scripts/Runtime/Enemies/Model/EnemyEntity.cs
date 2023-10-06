using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace Runtime.Enemies.Model {
	public interface IEnemyEntity : ICreature, IHaveCustomProperties, IHaveTags, ICanDealDamage {
		public BindableProperty<int> GetDanger();
		public BindableProperty<HealthInfo> GetHealth();
		
		//public BindableProperty<float> GetVigiliance();

		//public BindableProperty<float> GetAttackRange();
	
		public int GetRarity();
	}

	public abstract class EnemyEntity<T> : AbstractCreature, IEnemyEntity, IHaveTags where T : EnemyEntity<T>, new() {
		protected IDangerProperty dangerProperty;
		protected IHealthProperty healthProperty;
		
		
		protected override void OnEntityRegisterAdditionalProperties() {
			
			RegisterInitialProperty<IDangerProperty>(new Danger());
			//RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
			//RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
			OnEnemyRegisterAdditionalProperties();
		}
		
		

	

		public override void OnAwake() {
			base.OnAwake();
			dangerProperty = GetProperty<IDangerProperty>();
			healthProperty = GetProperty<IHealthProperty>();
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

		

		

		protected abstract void OnEnemyRegisterAdditionalProperties();

		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
		}


		public void OnKillDamageable(IDamageable damageable) {
			Debug.Log($"Kill Damageable: {damageable.EntityName}");
		}

		public void OnDealDamage(IDamageable damageable, int damage) {
			
		}
	}
}