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
using Runtime.Enemies;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossShoot : EnemyAction<WormBossEntity>
    {
        public GameObject missilePrefab;
        private bool ended;
        private Transform playerTrans;

        private SafeGameObjectPool pool;
        private int bulletCount;
        private float bulletSpeed;
        private float spawnInterval;

        public List<GameObject> bulletSpawnPos;

        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(missilePrefab, 20, 50);
            playerTrans = GetPlayer().transform;

        }
        public override void OnStart()
        {
            base.OnStart();
            ended = false;
            StartCoroutine(RapidFire());
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            if (ended)
            {
                return TaskStatus.Success;

            }
            else
            {
                return TaskStatus.Running;
            }
            
        }
        IEnumerator RapidFire()
        {
            StartCoroutine(SpawnMissile());
            yield return null;
        }
        IEnumerator SpawnMissile()
        {
            for (int i = 0; i < 7; i++)
            {
                GameObject b = pool.Allocate();
                b.transform.position = bulletSpawnPos[i].transform.position;
                b.transform.rotation = Quaternion.Euler(-90, 0, 0);
                b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value, 5, gameObject, gameObject.GetComponent<ICanDealDamage>(), -1);
                b.GetComponent<WormBossMissile>().Setup(10f, playerTrans , 30, 20);
                yield return new WaitForSeconds(1);

            }
            ended = true;
            yield return null;
        }
        
    }
}

