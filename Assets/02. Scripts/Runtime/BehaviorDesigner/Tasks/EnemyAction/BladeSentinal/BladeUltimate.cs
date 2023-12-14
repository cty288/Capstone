using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Spawning;
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
    private float bladeWaitTime;
    private float bladeDamage;

    private float chargeUpTime;
    
    public SharedGameObject swordPrefab;
    private SafeGameObjectPool bladePool;
    
    [SerializeField] private float jumpHeight;
    public override void OnAwake() {
        base.OnAwake();
        animator = gameObject.GetComponentInChildren<Animator>(true);
        bladePool = GameObjectPoolManager.Singleton.CreatePool(swordPrefab.Value, 10, 20);
		
        animator = gameObject.GetComponentInChildren<Animator>(true);
        agent = gameObject.GetComponent<NavMeshAgent>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public override void OnStart() {
        base.OnStart();
        agent.enabled = false;
        rb.isKinematic = true;
		
        bladeInterval = enemyEntity.GetCustomDataValue<float>("honingBlades", "bladeInterval");
        bladeCount = enemyEntity.GetCustomDataValue<int>("honingBlades", "bladeCount");
        bladeWaitTime = enemyEntity.GetCustomDataValue<float>("honingBlades", "bladeWaitTime");
        bladeDamage = enemyEntity.GetCustomDataValue<int>("honingBlades", "bladeDamage");
		
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

        anim.CrossFadeInFixedTime("Jump_Start", 0.2f);
        await UniTask.WaitForSeconds(chargeUpTime);
        anim.SetTrigger("Jump");
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("Jump"),
        PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());

        //Jump
        float duration = 0.5f;
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
            UniTask.Yield();
        }
        transform.position = targetPosition;


        for(int i = 0; i < bladeCount; i++)
        {
            float angle = i * 360f / 8;
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * rad;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * rad;

            Vector3 spawnPosition = new Vector3(x, 2, z) + this.gameObject.transform.position;
            Quaternion spawnRotation = Quaternion.Euler(0f, angle, 0f);

            GameObject blade = bladePool.Allocate();
            blade.transform.position = spawnPosition;
            blade.transform.rotation = spawnRotation;
            
            
            //blade.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, enemyEntity.GetCustomDataValue<int>("danmaku", "danmakuDamage"), gameObject, gameObject.GetComponent<ICanDealDamage>(), -1f);
            //blade.GetComponent<BladeSentinalBladeDanmaku>().SetData(5 , initRotationTime , 30 , 160 , this.gameObject.transform , playerTrans, OnBulletRecycled);

        }
        
        anim.SetTrigger("SkillEnd");
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());
        taskStatus = TaskStatus.Success;
    }
    

    public override void OnEnd() {
        base.OnEnd();
        StopAllCoroutines();
        rb.isKinematic = false;
    }
}