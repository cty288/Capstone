using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;
using Runtime.Spawning;
using Runtime.Spawning.Models.Properties;
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
		
		// public void SetDirectorOwner(IDirectorEntity directorEntity);
		// public IDirectorEntity GetDirectorOwner();
		
		public int SpawnedAreaIndex { get; set; }
	}

	public abstract class EnemyEntity<T> : AbstractCreature, IEnemyEntity, IHaveTags where T : EnemyEntity<T>, new() {
		protected IDangerProperty dangerProperty;
		protected IHealthProperty healthProperty;
		protected ISpawnCostProperty spawnCostProperty;
		protected ISpawnWeightProperty spawnWeightProperty;
		protected ILevelNumberProperty levelNumberProperty;
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;

		// protected IDirectorEntity directorOwner;
		public int SpawnedAreaIndex { get; set; }
		
		protected override void OnEntityRegisterAdditionalProperties() {
			base.OnEntityRegisterAdditionalProperties();
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
			if (HasProperty(new PropertyNameInfo(PropertyName.spawn_weight))) {
				return GetProperty<ISpawnWeightProperty>().BaseValue;
			}

			return 0;
		}

		//public abstract int OnGetRealSpawnWeight(int level, int baseWeight);

		public virtual float OnGetRealSpawnCost(int level, int rarity, float baseCost) {
			return GlobalLevelFormulas.GetSpawnCostModifier<float>(()=>rarity, ()=>level).Invoke(baseCost);
		}

		public float GetRealSpawnCost(int level, int rarity) {
			return OnGetRealSpawnCost(level, rarity, spawnCostProperty.BaseValue);
		}

		// public void SetDirectorOwner(IDirectorEntity directorEntity)
		// {
		// 	this.directorOwner = directorEntity;
		// }
		//
		// public IDirectorEntity GetDirectorOwner()
		// {
		// 	return directorOwner;
		// }


		protected abstract void OnEnemyRegisterAdditionalProperties();

		public override void OnDoRecycle() {
			SafeObjectPool<T>.Singleton.Recycle(this as T);
			OnModifyDamageCountCallbackList.Clear();
			_onDealDamageCallback = null;
			_onKillDamageableCallback = null;
		}


		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			Debug.Log($"Kill Damageable: {damageable.EntityName}");
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			
		}

		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

		Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}

		public ICanDealDamage ParentDamageDealer { get; } = null;


		/*public ICanDealDamageRootEntity RootDamageDealer => this;
		public ICanDealDamageRootViewController RootViewController => null;*/
	}
}