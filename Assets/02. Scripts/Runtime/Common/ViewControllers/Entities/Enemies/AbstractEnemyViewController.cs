using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.ViewControllers.Entities;
using _02._Scripts.Runtime.Common.ViewControllers.Entities.Enemies;
using MikroFramework.Architecture;
using UnityEngine;

public class AbstractEnemyViewController<T> : AbstractEntityViewController<T>, IEnemyViewController 
	where T : class, IEnemyEntity, new() {
	IEnemyEntity IEnemyViewController.EnemyEntity => BindedEntity;
	
	
	
	public int Danger { get; set; }
	//[BindableProperty(PropertyName.health, nameof(GetMaxHealth))]
	public int MaxHealth { get; set; }
	//[BindableProperty(PropertyName.health, nameof(GetCurrentHealth))]
	public int CurrentHealth { get; set; }
	
	public float Vigiliance { get; set; }
	
	public float AttackRange { get; set; }
	
	protected override void OnBindEntityProperty() {
		Bind("Danger", BindedEntity.GetDanger());
		Bind<int, HealthInfo>("MaxHealth", BindedEntity.GetHealth(), info => info.MaxHealth);
		Bind<int, HealthInfo>("CurrentHealth", BindedEntity.GetHealth(), info => info.CurrentHealth);
		Bind("Vigiliance", BindedEntity.GetVigiliance());
		Bind("AttackRange", BindedEntity.GetAttackRange());
	}

	protected object GetMaxHealth(object info) {
		return ((HealthInfo) info).MaxHealth;
	}
	
	protected object GetCurrentHealth(object info) {
		return ((HealthInfo) info).CurrentHealth;
	}
}
