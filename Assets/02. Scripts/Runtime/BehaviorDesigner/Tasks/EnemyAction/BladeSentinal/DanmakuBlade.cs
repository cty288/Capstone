using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.Weapons.ViewControllers.Base;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
public class DanmakuBlade : EnemyAction<BladeSentinelEntity>
{
    public SharedGameObject bladePrefab;
    private SafeGameObjectPool pool;
    private float rad;
    private const int bladeAmount = 8;
    private const float maxTime = 15f;
    private float timer = 0;
    private float bladeTravelSpeed;
    private float initRotationTime = 4f;
    private Transform playerTrans;

    private int recycledBulletCount = 0;
    private TaskStatus status;
    
    	
    private Animator animator;
    private NavMeshAgent agent;
    private Rigidbody rb;
    public override void OnAwake()
    {
        base.OnAwake();
        pool = GameObjectPoolManager.Singleton.CreatePool(bladePrefab.Value, 20, 30);
        playerTrans = GetPlayer().transform;
        
        animator = gameObject.GetComponentInChildren<Animator>(true);
        agent = gameObject.GetComponent<NavMeshAgent>();
        rb = gameObject.GetComponent<Rigidbody>();
       
    }

    public override void OnStart()
    {
        base.OnStart();
        agent.enabled = false;
        rb.isKinematic = true;
        animator.CrossFadeInFixedTime("Skill_SingleHand_Start", 0.2f);
        
        status = TaskStatus.Running;
        recycledBulletCount = 0;
        timer = 0;
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
        
        timer += Time.deltaTime;
        if (timer >= maxTime && status == TaskStatus.Running) {
            status = TaskStatus.Success;
        }

        if (status == TaskStatus.Success) {
            anim.CrossFadeInFixedTime("Skill_SingleHand_End", 0.1f);
        }
        
        return status;
    }

    public async UniTask SkillExecute()
    {
        if (bladePrefab == null) {
            Debug.LogError("Knife prefab is not assigned!");
            return;
        }
        
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Skill_SingleHand_Hold"),
            PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycle());
        int attackVersion = Random.Range(0, 2);

        for(int i = 0; i < bladeAmount; i++)
        {
            float angle = i * 360f / 8;
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * rad;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * rad;

            Vector3 spawnPosition = new Vector3(x, 2, z) + this.gameObject.transform.position;
            Quaternion spawnRotation = Quaternion.Euler(0f, angle, 0f);

            GameObject blade = pool.Allocate();
            blade.transform.position = spawnPosition;
            blade.transform.rotation = spawnRotation;
            
            
            blade.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, enemyEntity.GetCustomDataValue<int>("danmaku", "danmakuDamage"), gameObject, gameObject.GetComponent<ICanDealDamage>(), -1f);
            blade.GetComponent<BladeSentinalBladeDanmaku>().SetData(5 , initRotationTime , 30 , 160 , this.gameObject.transform , playerTrans, OnBulletRecycled , attackVersion);

        }
    }

    private void OnBulletRecycled() {
        recycledBulletCount++;
        if (recycledBulletCount == bladeAmount && status == TaskStatus.Running) {
            status = TaskStatus.Success;
        }
    }
    
}
