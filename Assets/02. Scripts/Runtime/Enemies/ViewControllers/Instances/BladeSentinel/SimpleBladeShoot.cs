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


public class SimpleBladeShoot : EnemyAction<BladeSentinelEntity>
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
    private float distanceFromOrigin = 8f;
    private int recycledBulletCount = 0;
    private TaskStatus status;
    private bool ended;
    private float interval = 1f;
    List<GameObject> blades = new List<GameObject>();
    private Animator animator;
    
    public override void OnAwake()
    {
        base.OnAwake();
        pool = GameObjectPoolManager.Singleton.CreatePool(bladePrefab.Value, 20, 30);
        playerTrans = GetPlayer().transform;

        animator = gameObject.GetComponentInChildren<Animator>(true);
        

    }

    public override void OnStart()
    {
        base.OnStart();
        // agent.enabled = false;
        //rb.isKinematic = true;
        animator.CrossFadeInFixedTime("Skill_SingleHand_Start", 0.2f);

        status = TaskStatus.Running;
        recycledBulletCount = 0;
        timer = 0;
        //SkillExecute();
        StartCoroutine(Fire());
    }

    public override TaskStatus OnUpdate()
    {
        if (ended)
        {
            Debug.Log("ended");
            return TaskStatus.Success;

        }
        else
        {

            return TaskStatus.Running;
        }
    }
    IEnumerator Fire()
    {
       
        int[] randomBladeOrder = new int[8];

        // Fill the array with sequential values from 0 to 7
        for (int i = 0; i < 8; i++)
        {
            randomBladeOrder[i] = i;
        }

        // Fisher-Yates shuffle to randomize the order
        for (int i = randomBladeOrder.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = randomBladeOrder[i];
            randomBladeOrder[i] = randomBladeOrder[randomIndex];
            randomBladeOrder[randomIndex] = temp;
        }
        if (bladePrefab == null)
        {
            Debug.LogError("Knife prefab is not assigned!");
            yield return null ;
        }
        for(int i = 0; i < bladeAmount; i++)
        {
            float angle = -90 + i * (180f / (bladeAmount - 1));
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
            Vector3 spawnPosition = transform.position + new Vector3(0, 1.5f, 0);



            GameObject blade = pool.Allocate();
            blade.transform.position = spawnPosition;
            blade.transform.Rotate(0f, 0f, angle);
            blade.transform.position += blade.transform.up * 4;
            blades.Add(blade);
        }
        for (int i = 0; i < bladeAmount; i++)
        {

            SpawnBullet(blades[randomBladeOrder[i]]);
            yield return new WaitForSeconds(interval);
        }
        ended = true;
    }
    void SpawnBullet( GameObject blade )
    {

    

        
            blade.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, enemyEntity.GetCustomDataValue<int>("danmaku", "danmakuDamage"), gameObject, gameObject.GetComponent<ICanDealDamage>(), -1f);
            Vector3 directionToPlayer = (playerTrans.position - transform.position).normalized;
            blade.GetComponent<BladeSentinalSimpleBullet>().SetData(8, directionToPlayer);
            /*
            float angle = i * 360f / 8;
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * rad;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * rad;

            Vector3 spawnPosition = new Vector3(x, 2, z) + this.gameObject.transform.position;
            Quaternion spawnRotation = Quaternion.Euler(0f, angle, 0f);

            GameObject blade = pool.Allocate();
            blade.transform.position = spawnPosition;
            blade.transform.rotation = spawnRotation;


            blade.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, enemyEntity.GetCustomDataValue<int>("danmaku", "danmakuDamage"), gameObject, gameObject.GetComponent<ICanDealDamage>(), -1f);
            blade.GetComponent<BladeSentinalBladeDanmaku>().SetData(5, initRotationTime, 30, 160, this.gameObject.transform, playerTrans, OnBulletRecycled, attackVersion);
            */
        
       
    }

    
    private void OnBulletRecycled()
    {
       
    }

}
