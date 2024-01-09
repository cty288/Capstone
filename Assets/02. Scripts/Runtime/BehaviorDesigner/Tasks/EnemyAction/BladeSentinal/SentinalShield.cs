using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.AI;

public class SentinalShield : EnemyAction<BladeSentinelEntity>
{
    private Animator animator;
    private TaskStatus taskStatus;
    private NavMeshAgent agent;
    private Rigidbody rb;
    

    public Transform pivot;
    
    private float shieldDuration;
    
    
    public SharedGameObject shieldPrefab;
    
    
    
    public override void OnAwake() {
        base.OnAwake();
		
        animator = gameObject.GetComponentInChildren<Animator>(true);
        agent = gameObject.GetComponent<NavMeshAgent>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public override void OnStart() {
        base.OnStart();
        agent.enabled = false;
        rb.isKinematic = true;
        
        shieldDuration = enemyEntity.GetCustomDataValue<float>("honingBlades", "honingDuration");
		
        taskStatus = TaskStatus.Running;
        SkillExecute();
    }

    public override TaskStatus OnUpdate() {
        Transform playerTr = GetPlayer().transform;
        Vector3 direction = playerTr.position - transform.position;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
		
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, lookRotation, Time.deltaTime * 10f);
        return taskStatus;
    }

    public async UniTask SkillExecute() {

        anim.CrossFadeInFixedTime("Shield_Start", 0.2f);
        GameObject shield = GameObject.Instantiate(shieldPrefab.Value,transform);
        shield.transform.localPosition = Vector3.zero+ transform.forward * 1.5f;
        
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Shield_Loop"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        await UniTask.WaitForSeconds(shieldDuration);
        
        anim.CrossFadeInFixedTime("Shield_End", 0f);
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        taskStatus = TaskStatus.Success;
    }
    

    public override void OnEnd() {
        base.OnEnd();
        StopAllCoroutines();
        agent.enabled = true;
    }
}
