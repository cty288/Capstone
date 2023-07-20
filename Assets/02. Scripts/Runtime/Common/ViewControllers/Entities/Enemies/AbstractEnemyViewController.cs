using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Entities.Enemies;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.ViewControllers.Entities;
using _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies;
using MikroFramework.Architecture;
using UnityEngine;

public abstract class AbstractEnemyViewController<T> : AbstractEntityViewController<T>, IEnemyViewController 
	where T : class, IEnemyEntity, new() {
	IEnemyEntity IEnemyViewController.EnemyEntity => BindedEntity;
	
	public int Danger {  get; }
	//[BindableProperty(PropertyName.health, nameof(GetMaxHealth))]
	public int MaxHealth { get; }
	//[BindableProperty(PropertyName.health, nameof(GetCurrentHealth))]
	public int CurrentHealth { get; }
	
	public float Vigiliance { get; }
	
	public float AttackRange { get;}
	
	
	
	protected override void OnBindEntityProperty() {
		Bind("Danger", BindedEntity.GetDanger());
		Bind<int, HealthInfo>("MaxHealth", BindedEntity.GetHealth(), info => info.MaxHealth);
		Bind<int, HealthInfo>("CurrentHealth", BindedEntity.GetHealth(), info => info.CurrentHealth);
		Bind("Vigiliance", BindedEntity.GetVigiliance());
		Bind("AttackRange", BindedEntity.GetAttackRange());
	}

	protected override IEntity OnInitEntity() {
		EnemyBuilder<T> builder = entityModel.GetEnemyBuilder<T>(1).FromConfig();
		return OnInitEnemyEntity(builder);
	}

	protected abstract IEnemyEntity OnInitEnemyEntity(EnemyBuilder<T> builder);

	protected object GetMaxHealth(object info) {
		return ((HealthInfo) info).MaxHealth;
	}
	
	protected object GetCurrentHealth(object info) {
		return ((HealthInfo) info).CurrentHealth;
	}
}
