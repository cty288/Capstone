using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Spawning;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.AI;

public class BladeUltimate : EnemyAction<BladeSentinelEntity> {
    private Animator animator;
    private TaskStatus taskStatus;
    private NavMeshAgent agent;
    private Rigidbody rb;
    
    private float rad;

    private float bladeInterval;
    private float bladeCount;
    private int bladeDamage;


    public Transform pivot;
    float arcRadius = 4f;

    private float bladeSpeed;
    private float honingDuration;
    
    
    public SharedGameObject swordPrefab;
    private SafeGameObjectPool bladePool;

    private List<GameObject> swordList;
    private int recycledBulletCount = 0;
    
    
    [SerializeField] private float jumpHeight;
    public override void OnAwake() {
        base.OnAwake();
        animator = gameObject.GetComponentInChildren<Animator>(true);
        bladePool = GameObjectPoolManager.Singleton.CreatePool(swordPrefab.Value, 10, 20);
		
        animator = gameObject.GetComponentInChildren<Animator>(true);
        agent = gameObject.GetComponent<NavMeshAgent>();
        rb = gameObject.GetComponent<Rigidbody>();
        swordList = new List<GameObject>();
    }

    public override void OnStart() {
        base.OnStart();
        agent.enabled = false;
        rb.isKinematic = true;
		
        bladeInterval = enemyEntity.GetCustomDataValue<float>("honingBlades", "bladeInterval");
        bladeCount = enemyEntity.GetCustomDataValue<int>("honingBlades", "bladeCount");
        bladeDamage = enemyEntity.GetCustomDataValue<int>("honingBlades", "bladeDamage");
        
        jumpHeight = enemyEntity.GetCustomDataValue<float>("honingBlades", "jumpHeight");
        honingDuration = enemyEntity.GetCustomDataValue<float>("honingBlades", "honingDuration");
        bladeSpeed  = enemyEntity.GetCustomDataValue<float>("honingBlades", "bladeSpeed");
		
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

        anim.CrossFadeInFixedTime("Ultimate_Jump_Start", 0.2f);
        await UniTask.WaitForSeconds(0.5f);
        anim.SetTrigger("Jump");
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Ultimate_Jump"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        //Jump
        float duration = 1f;
        float timeElapsed = 0;
        Vector3 startPosition = transform.position;
            
        //raycast up 
        RaycastHit hit;
            
        LayerMask mask = LayerMask.GetMask("Ground", "Wall");
        if (Physics.Raycast(transform.position, Vector3.up, out hit, jumpHeight, mask, QueryTriggerInteraction.Ignore)) {
            jumpHeight = hit.point.y - transform.position.y - enemyViewController.SpawnSizeCollider.bounds.size.y;
        }

        Vector3 targetPosition = transform.position + new Vector3(0, jumpHeight, 0);
        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            await UniTask.Yield();
        }
        transform.position = targetPosition;
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Ultimate_Idle"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        anim.CrossFadeInFixedTime("Ultimate_Windup", 0f);

        await UniTask.WaitForSeconds(1f, false, PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        recycledBulletCount = 0;
        for(int i = 0; i < bladeCount; i+=2)
        {
            float angle = (i+1)* 120f / (bladeCount+1);
            
            float x1 = Mathf.Sin(Mathf.Deg2Rad * angle) * arcRadius;
            float y1 = Mathf.Cos(Mathf.Deg2Rad * angle) * arcRadius;

            float x2 = Mathf.Sin(Mathf.Deg2Rad * -angle) * arcRadius;
            float y2 = Mathf.Cos(Mathf.Deg2Rad * -angle) * arcRadius;

            Vector3 spawnPosition1 = new Vector3(x1, y1, 0);
            Vector3 spawnPosition2 = new Vector3(x2, y2, 0);
            

            GameObject blade1 = bladePool.Allocate();
            GameObject blade2 = bladePool.Allocate();

            blade1.transform.parent = pivot;
            blade2.transform.parent = pivot;
            blade1.transform.localPosition = spawnPosition1;
            blade2.transform.localPosition = spawnPosition2;
            blade1.transform.forward = transform.forward;
            blade2.transform.forward = transform.forward;
            
            swordList.Add(blade1);
            swordList.Add(blade2);

            
            blade1.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, bladeDamage, gameObject, gameObject.GetComponent<ICanDealDamage>(), -1f);
            blade2.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, bladeDamage, gameObject, gameObject.GetComponent<ICanDealDamage>(), -1f);
            blade1.GetComponent<BladeSentinalHoningBlade>().SetData(bladeSpeed,honingDuration,GetPlayer().transform,transform,OnBulletRecycled);
            blade2.GetComponent<BladeSentinalHoningBlade>().SetData(bladeSpeed,honingDuration,GetPlayer().transform,transform,OnBulletRecycled);
            
            await UniTask.WaitForSeconds(0.5f, false, PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        }
        anim.CrossFadeInFixedTime("Ultimate_Shoot", 0f);
        for(int i = 0; i < bladeCount; i+=2)
        {

            swordList[i].GetComponent<BladeSentinalHoningBlade>().Activate();
            swordList[i+1].GetComponent<BladeSentinalHoningBlade>().Activate();

            await UniTask.WaitForSeconds(bladeInterval, false, PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

        }

        await UniTask.WaitForSeconds(3f);
        anim.SetTrigger("UltEnd");
        
        duration = 1f;
        timeElapsed = 0;
        startPosition = transform.position;
        
            
         mask = LayerMask.GetMask("Ground", "Wall");
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 100, mask, QueryTriggerInteraction.Ignore)) {
            targetPosition = hit.point;
        }
        
        while (timeElapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            await UniTask.Yield();
        }
        transform.position = targetPosition;
        anim.CrossFadeInFixedTime("Idle", 0f);
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        taskStatus = TaskStatus.Success;
    }
    
    private void OnBulletRecycled() {
        recycledBulletCount++;
    }

    public override void OnEnd() {
        base.OnEnd();
        StopAllCoroutines();
        agent.enabled = true;
    }
}