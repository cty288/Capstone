using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.Collision;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Enemies.ViewControllers.Base {
	[RequireComponent(typeof(AnimationSMBManager))]
	public abstract class AbstractEnemyViewController<T> : AbstractCreatureViewController<T>, IEnemyViewController, IHitResponder, ICanDealDamageViewController, ICanDealDamageRootViewController
		where T : class, IEnemyEntity, new() {
		IEnemyEntity IEnemyViewController.EnemyEntity => BoundEntity;
		


		public int Danger {  get; }
	
		public int MaxHealth { get; }
	
		//[Bind(PropertyName.health, nameof(GetCurrentHealth), nameof(OnCurrentHealthChanged))]
		public int CurrentHealth { get; }
		

		protected IEnemyEntityModel enemyModel;
		
		protected HealthBar currentHealthBar = null;

		
		protected List<GameObject> hitObjects = new List<GameObject>();
		
		protected ILevelModel levelModel;
		protected AnimationSMBManager animationSMBManager;
		protected override void Awake() {
			base.Awake();
			
			enemyModel = this.GetModel<IEnemyEntityModel>();
			animationSMBManager = GetComponent<AnimationSMBManager>();
			animationSMBManager.Event.AddListener(OnAnimationEvent);
			levelModel = this.GetModel<ILevelModel>();
		}

		protected abstract void OnAnimationEvent(string eventName);

		protected abstract HealthBar OnSpawnHealthBar();

		protected abstract void OnDestroyHealthBar(HealthBar healthBar);

		protected override void OnStart() {
			base.OnStart();
			currentHealthBar = OnSpawnHealthBar();
			if (currentHealthBar != null) {
				currentHealthBar.OnSetEntity(BoundEntity.HealthProperty.RealValue, BoundEntity);
			}
			
		}

		protected override void OnBindEntityProperty() {
			Bind("Danger", BoundEntity.GetDanger());
			Bind<HealthInfo, int>("MaxHealth", BoundEntity.GetHealth(), info => info.MaxHealth);
			Bind<HealthInfo, int>("CurrentHealth", BoundEntity.GetHealth(), info => info.CurrentHealth);
		}
		
		public override ICreature OnInitEntity(int level, int rarity){
			if (enemyModel == null) {
				enemyModel = this.GetModel<IEnemyEntityModel>();
			}

			EnemyBuilder<T> builder = enemyModel.GetEnemyBuilder<T>(rarity);
			builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level);

			return OnInitEnemyEntity(builder);
		}
		

		protected abstract IEnemyEntity OnInitEnemyEntity(EnemyBuilder<T> builder);

		protected dynamic GetMaxHealth(dynamic info) {
			return info.MaxHealth;
		}
	
		protected dynamic GetCurrentHealth(dynamic info) {
			return info.CurrentHealth;
		}

		protected override void OnEntityDie(ICanDealDamage damagedealer) {
			base.OnEntityDie(damagedealer);
			MikroAction action = WaitingForDeathCondition();
			if (action != null) {
				action.OnEndedCallback += () => {
					OnDieWaitEnd();
				};
				action.Execute();
			}
			else {
				OnDieWaitEnd();
			}
		}

		private void OnDieWaitEnd() {
			enemyModel.RemoveEntity(BoundEntity.UUID);
		}

		protected abstract MikroAction WaitingForDeathCondition();

		protected void OnCurrentHealthChanged(int oldValue, int newValue) {
			Debug.Log("CurrentHealth changed from " + oldValue + " to " + newValue);
			
		}

		public override void OnRecycled() {
			base.OnRecycled();
			
		}

		protected override void OnReadyToRecycle() {
			base.OnReadyToRecycle();
			if (currentHealthBar) {
				currentHealthBar.OnHealthBarDestroyed();
				OnDestroyHealthBar(currentHealthBar);
			}
			currentHealthBar = null;
		}

		protected override int GetSpawnedCombatCurrencyAmount() {
			int currentLevel = levelModel.CurrentLevelCount;
			float referenceCount = BoundEntity.GetRealSpawnCost(currentLevel, BoundEntity.GetRarity());
			//+- 10%
			float randomCount = UnityEngine.Random.Range(-referenceCount * 0.1f, referenceCount * 0.1f);
			int result = Mathf.RoundToInt(referenceCount + randomCount);
			result = Mathf.Clamp(result, 1, int.MaxValue);
			return result;
		}

		public virtual bool CheckHit(HitData data) {
			
			if (data.Hurtbox.Owner == gameObject) { return false; }
			else if (hitObjects.Contains(data.Hurtbox.Owner)) { return false; }
			else { return true; }
		}

		public virtual void HitResponse(HitData data) {
			hitObjects.Add(data.Hurtbox.Owner);
		}


		public void OnKillDamageable(IDamageable damageable) {
			BoundEntity?.OnKillDamageable(damageable);
		}

		public void OnDealDamage(IDamageable damageable, int damage) {
			BoundEntity?.OnDealDamage(damageable, damage);
		}

		public ICanDealDamageRootEntity RootDamageDealer => BoundEntity?.RootDamageDealer;
		public ICanDealDamageRootViewController RootViewController => this;

		public ICanDealDamage CanDealDamageEntity => BoundEntity;
		public Transform GetTransform() {
			return transform;
		}
	}
}
