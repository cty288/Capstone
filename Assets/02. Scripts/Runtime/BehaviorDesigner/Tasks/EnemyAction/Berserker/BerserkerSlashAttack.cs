using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using Runtime.Temporary;
using Runtime.Weapons.ViewControllers.Base;
using Runtime.Weapons.ViewControllers.Instances.WormBoss;
using UnityEngine;

public class BerserkerSlashAttack : EnemyAction<BerserkerEntity> {
	[SerializeField] private GameObject slashPrefab;
	private TaskStatus taskStatus; 
	private SafeGameObjectPool pool;

	public override void OnAwake() {
		base.OnAwake();
		pool = GameObjectPoolManager.Singleton.CreatePool(slashPrefab, 5, 10);
	}

	public override void OnStart() {
		base.OnStart();
		taskStatus = TaskStatus.Running;
		SkillExecute();
	}
	public async UniTask SkillExecute() {
		int slashTime = enemyEntity.GetCustomDataValue<int>("slash", "slash_time");
		float slashSpeed = enemyEntity.GetCustomDataValue<float>("slash", "slash_speed");
		float slashScaleMultiplier = enemyEntity.GetCustomDataValue<float>("slash", "scale_multiplier");
		float slashInterval = enemyEntity.GetCustomDataValue<float>("slash", "slash_interval");
		int slashDamage = enemyEntity.GetCustomDataValue<int>("slash", "slash_damage");

		for (int i = 0; i < slashTime; i++) {
			await UniTask.WaitForSeconds(2, false, PlayerLoopTiming.Update,
				gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie()); //wait for animation or sth
			
			SpawnSlash(slashSpeed, slashScaleMultiplier, slashDamage);
			
			await UniTask.WaitForSeconds(slashInterval, false, PlayerLoopTiming.Update,
				gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		}

		taskStatus = TaskStatus.Success;
	}
	
	
	private void SpawnSlash(float slashSpeed, float slashScaleMultiplier, int slashDamage) {
		UnityEngine.GameObject b = pool.Allocate();
		b.transform.position = enemyViewController.GetTransform().position;
		b.transform.localScale = new Vector3(slashScaleMultiplier, slashScaleMultiplier, slashScaleMultiplier);


		b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
			slashDamage,
			gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);


		b.GetComponent<BerserkerSlashBullet>().SetData(slashSpeed,
			GetPlayer().transform.position - enemyViewController.GetTransform().position);
	}
	
	public override TaskStatus OnUpdate() {
		return taskStatus;
	}
}
