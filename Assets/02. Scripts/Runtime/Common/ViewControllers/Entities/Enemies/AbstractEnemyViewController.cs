using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Common.ViewControllers.Entities;
using MikroFramework.Architecture;
using UnityEngine;

public class AbstractEnemyViewController<T> : AbstractEntityViewController<T>, IEnemyViewController 
	where T : class, IEnemyEntity, new() {
	IEnemyEntity IEnemyViewController.EnemyEntity => BindedEntity;
	
	public int Danger { get; set; }
	public int MaxHealth { get; set; }
	public int CurrentHealth { get; set; }
	public float Vigiliance { get; set; }
	public float AttackRange { get; set; }
	protected override void OnBindEntityProperty() {
		Bind<IDangerProperty>("Danger");
		Bind<int, HealthInfo>("MaxHealth", BindedEntity.GetHealth(), info => info.MaxHealth);
		Bind<int, HealthInfo>("CurrentHealth", BindedEntity.GetHealth(), info => info.CurrentHealth);
		Bind("Vigiliance", BindedEntity.GetVigiliance());
		Bind<IAttackRangeProperty>("AttackRange");
	}
}
