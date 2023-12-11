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
	
	private Animator animator;
	private TaskStatus taskStatus;
	private float bladeInterval;
	private float bladeCount;
	private float bladeWaitTime;
	private float bladeDamage;
	private SafeGameObjectPool bladePool;
	private NavMeshAgent agent;
	private Rigidbody rb;
	public override void OnAwake() {
		base.OnAwake();
		animator = gameObject.GetComponentInChildren<Animator>(true);
		bladePool = GameObjectPoolManager.Singleton.CreatePool(uppercutBladePrefab, 10, 20);
		agent = gameObject.GetComponent<NavMeshAgent>();
		rb = gameObject.GetComponent<Rigidbody>();
	}

	public override void OnStart() {
		base.OnStart();
		agent.enabled = false;
		rb.isKinematic = true;
		animator.CrossFadeInFixedTime("Skill_Start", 0.2f);
		
		bladeInterval = enemyEntity.GetCustomDataValue<float>("uppercutBlades", "bladeInterval");
		bladeCount = enemyEntity.GetCustomDataValue<int>("uppercutBlades", "bladeCount");
		bladeWaitTime = enemyEntity.GetCustomDataValue<float>("uppercutBlades", "bladeWaitTime");
		bladeDamage = enemyEntity.GetCustomDataValue<int>("uppercutBlades", "bladeDamage");
		
		taskStatus = TaskStatus.Running;

		SkillExecute();
	}

	public override TaskStatus OnUpdate() {
		return taskStatus;
	}

	public async UniTask SkillExecute() {

		await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_Hold"),
			PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());


		List<UniTask> tasks = new List<UniTask>();
		//start spawning blades
		for (int i = 0; i < bladeCount; i++) {
			UniTask task = SpawnBlade();
			tasks.Add(task);
			await UniTask.WaitForSeconds(bladeInterval, false, PlayerLoopTiming.Update,
				gameObject.GetCancellationTokenOnDestroyOrRecycle());
		}

		await UniTask.WhenAll(tasks);
		taskStatus = TaskStatus.Success;
	}

	public async UniTask SpawnBlade() {
		UnityEngine.Transform playerTr = GetPlayer().transform;
		if(!SpawningUtility.IsSlopeTooSteepAtPoint(playerTr.transform.position, 90f, out Quaternion rotation, out Vector3 groundPos)) {
			GameObject indicator = GameObject.Instantiate(indicatorPrefab, groundPos, rotation);

			await UniTask.WaitForSeconds(bladeWaitTime, false, PlayerLoopTiming.Update,
				gameObject.GetCancellationTokenOnDestroyOrRecycle());
			GameObject.Destroy(indicator);
				
			//spawn blade
			GameObject blade = bladePool.Allocate();
			blade.transform.position = groundPos;
			
			//randomly rotate in y axis
			Vector3 rot = rotation.eulerAngles;
			rot.y = Random.Range(0, 360);



			blade.transform.rotation = Quaternion.Euler(rot);
				
			blade.GetComponent<UpperCutBladeViewController>().Init(enemyEntity.CurrentFaction, (int) bladeDamage,
				gameObject, enemyEntity);

		}
	}

	public override void OnEnd() {
		base.OnEnd();
		StopAllCoroutines();
		rb.isKinematic = false;
	}
}
