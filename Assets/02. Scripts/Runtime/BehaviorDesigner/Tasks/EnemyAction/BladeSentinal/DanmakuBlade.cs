using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Spawning;
using UnityEngine;
using UnityEngine.AI;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;
using MikroFramework;
using MikroFramework.Pool;
public class DanmakuBlade : EnemyAction<BladeSentinelEntity>
{
    public SharedGameObject bladePrefab;
    private SafeGameObjectPool pool;
    private float rad;
    private int bladeAmount;
    private float bladeTravelSpeed;

    public override void OnAwake()
    {
        base.OnAwake();
        pool = GameObjectPoolManager.Singleton.CreatePool(bladePrefab.Value, 20, 30);
       
        
    }

    public override void OnStart()
    {
        base.OnStart();
        SpawnBlades();
    }

    void SpawnBlades()
    {
        if(bladePrefab == null)
        {
            Debug.LogError("Knife prefab is not assigned!");
            return;
        }

        for(int i = 0; i < 8; i++)
        {
            float angle = i * 360f / 8;
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * rad;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * rad;

            Vector3 spawnPosition = new Vector3(x, 8, z) + this.gameObject.transform.position;
            Quaternion spawnRotation = Quaternion.Euler(0f, angle, 0f);

            GameObject blade = pool.Allocate();
            blade.transform.position = spawnPosition;
            blade.transform.rotation = spawnRotation;
            blade.GetComponent<BladeSentinalBladeDanmaku>().SetData(5);

        }
    }



}
