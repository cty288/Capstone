using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Entities.Enemies;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.ViewControllers.Entities;
using _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies;
using MikroFramework.Architecture;
using UnityEngine;

public abstract class AbstractEnemyViewController<T> : AbstractHaveCustomPropertyEntityViewController<T, IEnemyEntityModel>, IEnemyViewController 
	where T : class, IEnemyEntity, new() {
	IEnemyEntity IEnemyViewController.EnemyEntity => BindedEntity;
	
	public int Danger {  get; }
	
	public int MaxHealth { get; }
	
	//[Bind(PropertyName.health, nameof(GetCurrentHealth), nameof(OnCurrentHealthChanged))]
	public int CurrentHealth { get; }
	
	public float Vigiliance { get; }
	
	public float AttackRange { get;}

	protected IEnemyEntityModel enemyEntityModel;

	protected override void Awake() {
		base.Awake();
		enemyEntityModel = this.GetModel<IEnemyEntityModel>();
	}

	protected override void OnBindEntityProperty() {
		
		Bind("Danger", BindedEntity.GetDanger());
		Bind<HealthInfo, int>("MaxHealth", BindedEntity.GetHealth(), info => info.MaxHealth);
		
		Bind<HealthInfo, int>("CurrentHealth", BindedEntity.GetHealth(), info => info.CurrentHealth);
		
		Bind("Vigiliance", BindedEntity.GetVigiliance());
		Bind("AttackRange", BindedEntity.GetAttackRange());
		
	}

	protected override IEntity OnInitEntity() {
		EnemyBuilder<T> builder = enemyEntityModel.GetEnemyBuilder<T>(1);
		return OnInitEnemyEntity(builder);
	}

	protected abstract IEnemyEntity OnInitEnemyEntity(EnemyBuilder<T> builder);

	protected dynamic GetMaxHealth(dynamic info) {
		return info.MaxHealth;
	}
	
	protected dynamic GetCurrentHealth(dynamic info) {
		return info.CurrentHealth;
	}
	
	protected void OnCurrentHealthChanged(int oldValue, int newValue) {
		Debug.Log("CurrentHealth changed from " + oldValue + " to " + newValue);
	}
}
