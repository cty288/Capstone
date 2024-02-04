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
    
    // private float shieldDuration;
    //
    // private int bladeCount;
    private List<GameObject> swordList;
    private List<GameObject> swordSpawnPositions;
    private List<GameObject> sheildList;
    
    
    public SharedGameObject swordPrefab;
    private SafeGameObjectPool bladePool;
    float arcRadius = 1.5f;


    private bool rotating = false;
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
        // pivot.transform.localPosition = Vector3.zero;
        // pivot.transform.rotation = Quaternion.identity;
        // shieldDuration = enemyEntity.GetCustomDataValue<float>("shield", "shieldDuration");
        // bladeCount = enemyEntity.GetCustomDataValue<int>("shield", "bladeCount");
        swordList = enemyEntity.GetSwordList();
        sheildList = enemyEntity.GetShieldList();
        swordSpawnPositions = enemyEntity.GetPositionList();
        
        taskStatus = TaskStatus.Running;
        CreateShield();
    }

    public override TaskStatus OnUpdate() {
        Transform playerTr = GetPlayer().transform;
        Vector3 direction = playerTr.position - transform.position;
        direction.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
		
        gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, lookRotation, Time.deltaTime * 10f);

        if (rotating)
        {
            pivot.transform.Rotate(new Vector3(0,60,0) * Time.deltaTime);
        }
        // else
        // {
        //     pivot.transform.rotation = Quaternion.identity;
        // }
        return taskStatus;
    }

    public async UniTask CreateShield() {

        anim.CrossFadeInFixedTime("Shield_Start", 0.2f);
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Shield_Loop"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        
        rotating = true;
        for(int i = 0; i < swordList.Count; i++)
        {
            GameObject shield;
            if (i > 0) // initialize shields
            {
                shield = sheildList[i - 1];
                shield.SetActive(true);
            }
            
            GameObject blade = swordList[i]; // initialize blades
            blade.SetActive(true);
            blade.transform.parent = swordSpawnPositions[i].transform;
            blade.transform.localPosition = Vector3.zero;
            
            if(i == swordList.Count - 1) // initialize last shield
            {
                shield = sheildList[i];
                shield.SetActive(true);
            }
            
            await UniTask.WaitForSeconds(0.1f);
        }
        rotating = false;
        
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
