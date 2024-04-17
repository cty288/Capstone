using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using Runtime.Temporary;
using UnityEngine;

public class BerserkerGenerateEMPField : EnemyAction<BerserkerEntity> {

	private SharedGameObject generatedEMPField;
	private TaskStatus taskStatus; 
	[SerializeField] private GameObject empFieldPrefab;
	public override void OnAwake() {
		base.OnAwake();
		generatedEMPField = (SharedGameObject) GetComponent<BehaviorTree>().GetVariable("GeneratedEMPField");
		
	}

	public override void OnStart() {
		base.OnStart();
		taskStatus = TaskStatus.Running;
		if (generatedEMPField.Value) {
			taskStatus = TaskStatus.Success;
		}
		else {
			SkillExecute();
		}
	}
	public async UniTask SkillExecute() {

		/*await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_SingleHand_Hold"),
			PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());*/

		await UniTask.WaitForSeconds(2f, false, PlayerLoopTiming.Update,
			gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		
		
		float speed = enemyEntity.GetCustomDataValue<float>("emp", "speed");
		float scaleMultiplier = enemyEntity.GetCustomDataValue<float>("emp", "scaleMultiplier");
		
		GameObject player = GetPlayer();

		GameObject empField = GameObject.Instantiate(empFieldPrefab, player.transform.position, Quaternion.identity);
		BerserkerEMPField empFieldComponent = empField.GetComponent<BerserkerEMPField>();
		empFieldComponent.SetFollowTarget(player.transform, speed, scaleMultiplier);
		generatedEMPField.Value = empField;
		
		await UniTask.WaitForSeconds(1f, false, PlayerLoopTiming.Update,
			gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		
		taskStatus = TaskStatus.Success;
	}
	public override TaskStatus OnUpdate() {
		return taskStatus;
	}
}
