using System;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Base {
	public abstract class AbstractEnemyViewController<T> : AbstractCreatureViewController<T>, IEnemyViewController, IHitResponder
		where T : class, IEnemyEntity, new() {
		IEnemyEntity IEnemyViewController.EnemyEntity => BoundEntity;
	
		public int Danger {  get; }
	
		public int MaxHealth { get; }
	
		//[Bind(PropertyName.health, nameof(GetCurrentHealth), nameof(OnCurrentHealthChanged))]
		public int CurrentHealth { get; }
		

		protected IEnemyEntityModel enemyModel;
		
		protected HealthBar currentHealthBar = null;

		public int Damage => GetCurrentHitDamage();
		protected List<GameObject> hitObjects = new List<GameObject>();
		
		
		protected abstract int GetCurrentHitDamage();

		protected override void Awake() {
			base.Awake();
			enemyModel = this.GetModel<IEnemyEntityModel>();
		}

		protected abstract HealthBar OnSpawnHealthBar();

		protected abstract void OnDestroyHealthBar(HealthBar healthBar);

		protected override void OnStart() {
			base.OnStart();
			currentHealthBar = OnSpawnHealthBar();
			if (currentHealthBar != null) {
				currentHealthBar.OnSetEntity(BoundEntity);
			}
		}

		protected override void OnBindEntityProperty() {
			Bind("Danger", BoundEntity.GetDanger());
			Bind<HealthInfo, int>("MaxHealth", BoundEntity.GetHealth(), info => info.MaxHealth);
			Bind<HealthInfo, int>("CurrentHealth", BoundEntity.GetHealth(), info => info.CurrentHealth);
		}

		protected override IEntity OnBuildNewEntity() {
			EnemyBuilder<T> builder = enemyModel.GetEnemyBuilder<T>(1);
			return OnInitEnemyEntity(builder);
		}

		protected abstract IEnemyEntity OnInitEnemyEntity(EnemyBuilder<T> builder);

		protected dynamic GetMaxHealth(dynamic info) {
			return info.MaxHealth;
		}
	
		protected dynamic GetCurrentHealth(dynamic info) {
			return info.CurrentHealth;
		}

		protected override void OnEntityDie(IBelongToFaction damagedealer) {
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
			if (currentHealthBar) {
				currentHealthBar.OnHealthBarDestroyed();
				OnDestroyHealthBar(currentHealthBar);
				
			}
			
		}

		public virtual bool CheckHit(HitData data) {
			
			if (data.Hurtbox.Owner == gameObject) { return false; }
			else if (hitObjects.Contains(data.Hurtbox.Owner)) { return false; }
			else { return true; }
		}

		public virtual void HitResponse(HitData data) {
			hitObjects.Add(data.Hurtbox.Owner);
		}
	}
}
