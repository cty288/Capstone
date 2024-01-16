using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using MikroFramework.ResKit;
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
    
    private int bladeCount;
    private List<GameObject> swordList;
    
    
    public SharedGameObject swordPrefab;
    private SafeGameObjectPool bladePool;
    float arcRadius = 1.5f;


    private bool rotating = false;
    public override void OnAwake() {
        base.OnAwake();
		
        animator = gameObject.GetComponentInChildren<Animator>(true);
        agent = gameObject.GetComponent<NavMeshAgent>();
        rb = gameObject.GetComponent<Rigidbody>();
        bladePool = GameObjectPoolManager.Singleton.CreatePool(swordPrefab.Value, 10, 20);
        swordList = new List<GameObject>();
    }

    public override void OnStart() {
        base.OnStart();
        agent.enabled = false;
        rb.isKinematic = true;
        
        shieldDuration = enemyEntity.GetCustomDataValue<float>("shield", "shieldDuration");
        bladeCount = enemyEntity.GetCustomDataValue<int>("shield", "bladeCount");
        taskStatus = TaskStatus.Running;
        SkillExecute();
    }

    public override TaskStatus OnUpdate() {
        Transform playerTr = GetPlayer().transform;
        Vector3 direction = playerTr.position - transform.position;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
		
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, lookRotation, Time.deltaTime * 10f);

        if (rotating)
        {
            pivot.transform.Rotate(new Vector3(0,60,0)*Time.deltaTime);
        }
        else
        {
            pivot.transform.rotation = Quaternion.identity;
        }
        return taskStatus;
    }

    public async UniTask SkillExecute() {

        anim.CrossFadeInFixedTime("Shield_Start", 0.2f);
        
        for(int i = 0; i < bladeCount; i++)
        {
            float angle = i* 360f / bladeCount;
            
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * arcRadius;
            
            float z = Mathf.Cos(Mathf.Deg2Rad * -angle) * arcRadius;

            Vector3 spawnPosition = new Vector3(x, 0, z);
            

            GameObject blade = bladePool.Allocate();
            
            blade.transform.parent = pivot;
            blade.transform.localPosition = spawnPosition;
            blade.transform.forward = transform.forward;
            
            swordList.Add(blade);

        }
        float duration = 1f;
        float timeElapsed = 0;
        Vector3 startPosition = pivot.transform.position;
        Vector3 targetPosition = pivot.transform.position + new Vector3(0, 1, 0);
        while (timeElapsed < duration)
        {
            pivot.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update,
                gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        }
        pivot.transform.position = targetPosition;
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Shield_Loop"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        rotating = true;

        await UniTask.WaitForSeconds(shieldDuration, false, PlayerLoopTiming.Update,
            gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        
        rotating = false;
        
        startPosition = pivot.transform.position;
        targetPosition = pivot.transform.position - new Vector3(0, 1, 0);
        while (timeElapsed < duration)
        {
            pivot.transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update,
                gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        }

        foreach (var blade in swordList)
        {
            blade.GetComponent<DefaultPoolableGameObject>().RecycleToCache();
        }
        swordList.Clear();
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
