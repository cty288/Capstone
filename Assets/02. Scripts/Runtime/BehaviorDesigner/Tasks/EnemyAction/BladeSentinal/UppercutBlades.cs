using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Spawning;

using UnityEngine;
using UnityEngine.AI;

public class UppercutBlades : EnemyAction<BladeSentinelEntity> {
	[SerializeField] private GameObject indicatorPrefab;
	[SerializeField] private GameObject uppercutBladePrefab;
	
	
	private TaskStatus taskStatus;
	private float bladeInterval;
	private float bladeCount;
	private float bladeWaitTime;
	private float bladeDamage;
	private SafeGameObjectPool bladePool;
	
	private Animator animator;
	//private NavMeshAgent agent;
	//private Rigidbody rb;
	public override void OnAwake() {
		base.OnAwake();
		
		bladePool = GameObjectPoolManager.Singleton.CreatePool(uppercutBladePrefab, 10, 20);
		
		animator = gameObject.GetComponentInChildren<Animator>(true);
		//agent = gameObject.GetComponent<NavMeshAgent>();
		//rb = gameObject.GetComponent<Rigidbody>();
	}

	public override void OnStart() {
		base.OnStart();
		animator.CrossFadeInFixedTime("Skill_SingleHand_Start", 0.2f);
		
		bladeInterval = enemyEntity.GetCustomDataValue<float>("uppercutBlades", "bladeInterval");
		bladeCount = enemyEntity.GetCustomDataValue<int>("uppercutBlades", "bladeCount");
		bladeWaitTime = enemyEntity.GetCustomDataValue<float>("uppercutBlades", "bladeWaitTime");
		bladeDamage = enemyEntity.GetCustomDataValue<int>("uppercutBlades", "bladeDamage");
		
		taskStatus = TaskStatus.Running;

		SkillExecute();
	}

	public override TaskStatus OnUpdate() {
		Transform playerTr = GetPlayer().transform;
		Vector3 direction = playerTr.position - gameObject.transform.position;
		direction.y = 0;
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		//look rotation.y - 48
		
		lookRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y - 48, 0);
		
		gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, lookRotation, Time.deltaTime * 10f);
		
		return taskStatus;
	}

	public async UniTask SkillExecute() {

		await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_SingleHand_Hold"),
			PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());


		List<UniTask> tasks = new List<UniTask>();
		//start spawning blades
		for (int i = 0; i < bladeCount; i++) {
			UniTask task = SpawnBlade();
			tasks.Add(task);
			await UniTask.WaitForSeconds(bladeInterval, false, PlayerLoopTiming.Update,
				gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		}

		await UniTask.WhenAll(tasks);
		anim.SetTrigger("SkillEnd");
		await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),
			PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		taskStatus = TaskStatus.Success;
	}

	public async UniTask SpawnBlade() {
		UnityEngine.Transform playerTr = GetPlayer().transform;
		if(!SpawningUtility.IsSlopeTooSteepAtPoint(playerTr.transform.position, 90f, out Quaternion rotation, out Vector3 groundPos)) {
			GameObject indicator = GameObject.Instantiate(indicatorPrefab, groundPos, rotation);

			await UniTask.WaitForSeconds(bladeWaitTime, false, PlayerLoopTiming.Update,
				gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
			GameObject.Destroy(indicator);
				
			//spawn blade
			GameObject blade = bladePool.Allocate();
			blade.transform.position = groundPos;
			
			enemyEntity.RemoveBlades(1);
			
			//randomly rotate in y axis
			Vector3 rot = rotation.eulerAngles;
			rot.y = Random.Range(-45, 45);

			blade.transform.rotation = Quaternion.Euler(rot);
				
			blade.GetComponent<UpperCutBladeViewController>().Init(enemyEntity.CurrentFaction, (int) bladeDamage,
				gameObject, enemyEntity);

		}
	}

	public override void OnEnd() {
		base.OnEnd();
		StopAllCoroutines();
		//rb.isKinematic = false;
	}
}
