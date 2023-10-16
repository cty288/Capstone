using _02._Scripts.Runtime.Levels;
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
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Enemies.Model {
	public interface IEnemyEntity : ICreature, IHaveCustomProperties, IHaveTags, ICanDealDamage {
		public BindableProperty<int> GetDanger();
		public BindableProperty<HealthInfo> GetHealth();
		
		//public BindableProperty<float> GetVigiliance();

		//public BindableProperty<float> GetAttackRange();
	
		public int GetRarity();

		public int GetRealSpawnWeight(int level);
		
		public float GetRealSpawnCost(int level, int rarity);
	}

	public abstract class EnemyEntity<T> : AbstractCreature, IEnemyEntity, IHaveTags where T : EnemyEntity<T>, new() {
		protected IDangerProperty dangerProperty;
		protected IHealthProperty healthProperty;
		protected ISpawnCostProperty spawnCostProperty;
		protected ISpawnWeightProperty spawnWeightProperty;
		protected ILevelNumberProperty levelNumberProperty;
		
		protected override void OnEntityRegisterAdditionalProperties() {
			
			RegisterInitialProperty<IDangerProperty>(new Danger());
			RegisterInitialProperty<ISpawnWeightProperty>(new SpawnWeight());
			RegisterInitialProperty<ISpawnCostProperty>(new SpawnCost());
			RegisterInitialProperty<ILevelNumberProperty>(new LevelNumber());
			//RegisterInitialProperty<IVigilianceProperty>(new TestVigiliance());
			//RegisterInitialProperty<IAttackRangeProperty>(new TestAttackRange());
			OnEnemyRegisterAdditionalProperties();
		}


		protected override void OnInitModifiers(int rarity) {
			int level = levelNumberProperty.BaseValue;
			OnInitModifiers(rarity, level);
			SetGeneralEnemyAbilityModifier<HealthInfo>(new PropertyNameInfo(PropertyName.health), rarity, level);
		}

		protected void SetGeneralEnemyAbilityModifier<T>(PropertyNameInfo propertyName, int rarity, int level, bool inverse = false) {
			SetPropertyModifier<T>(propertyName,
				GlobalLevelFormulas.GetGeneralEnemyAbilityModifier<T>(() => rarity, () => level, inverse));
		}

		protected abstract void OnInitModifiers(int rarity, int level);


		public override void OnAwake() {
			base.OnAwake();
			dangerProperty = GetProperty<IDangerProperty>();
			healthProperty = GetProperty<IHealthProperty>();
			spawnCostProperty = GetProperty<ISpawnCostProperty>();
			spawnWeightProperty = GetProperty<ISpawnWeightProperty>();
			levelNumberProperty = GetProperty<ILevelNumberProperty>();
			
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

		public int GetRealSpawnWeight(int level) {
			return OnGetRealSpawnWeight(level, GetProperty<ISpawnWeightProperty>().BaseValue);
		}

		public abstract int OnGetRealSpawnWeight(int level, int baseWeight);

		public virtual float OnGetRealSpawnCost(int level, int rarity, float baseCost) {
			return GlobalLevelFormulas.GetSpawnCostModifier<float>(()=>rarity, ()=>level).Invoke(baseCost);
		}

		public float GetRealSpawnCost(int level, int rarity) {
			return OnGetRealSpawnCost(level, rarity, spawnCostProperty.BaseValue);
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