using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Spawning;
using UnityEditor.VersionControl;
using UnityEngine;

public class UppercutBlades : EnemyAction<BladeSentinelEntity> {
	private Animator animator;
	private TaskStatus taskStatus;

	private float bladeInterval;
	private float bladeCount;
	private float bladeWaitTime;
	private float bladeDamage;
	public override void OnAwake() {
		base.OnAwake();
		animator = gameObject.GetComponentInChildren<Animator>(true);
	}

	public override void OnStart() {
		base.OnStart();
		animator.CrossFadeInFixedTime("Skill_Start", 0.2f);
		
		bladeInterval = enemyEntity.GetCustomDataValue<float>("uppercutBlades", "bladeInterval");
		bladeCount = enemyEntity.GetCustomDataValue<int>("uppercutBlades", "bladeCount");
		bladeWaitTime = enemyEntity.GetCustomDataValue<float>("uppercutBlades", "bladeWaitTime");
		bladeDamage = enemyEntity.GetCustomDataValue<int>("uppercutBlades", "bladeDamage");
		
		taskStatus = TaskStatus.Running;
	}

	public override TaskStatus OnUpdate() {
		return taskStatus;
	}

	public async UniTask SkillExecute() {
		await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_Hold"));
		//start spawning blades
		for (int i = 0; i < bladeCount; i++) {
			SpawningUtility.SpawnBossPillars()
			await UniTask.Delay((int) (bladeWaitTime * 1000));
			//spawn blade
		}
	}
}
