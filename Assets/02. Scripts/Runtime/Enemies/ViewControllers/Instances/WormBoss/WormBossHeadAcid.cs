using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning;
using System.Threading.Tasks;
using a;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using FIMSpace.FSpine;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies;
using Runtime.Weapons.ViewControllers.Base;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossHeadAcid : EnemyAction
    {
        public SharedGameObject firePoint;
        
        public SharedGameObject acidBulletPrefab;
        private SafeGameObjectPool pool;
        
        private Transform playerTrans;
        
        public float bulletInterval = 1f;
        public float bulletSpeed = 10f;
        public int bulletCount = 5;

        public override void OnStart()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(acidBulletPrefab.Value, 5, 20);
            playerTrans = GetPlayer().transform;

            StartCoroutine(Shoot());
        }
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Running;
        }
        
        public IEnumerator Shoot()
        {
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < bulletCount; i++)
            {
                SpawnBullet();
                yield return new WaitForSeconds(bulletInterval);
            }
        }
        
        void SpawnBullet() 
        {
            GameObject b = pool.Allocate();
            b.transform.position = firePoint.Value.transform.position;
            b.transform.rotation = firePoint.Value.transform.rotation;
            // b.transform.LookAt(playerTrans);
            
            // b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
            //     enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
            //     gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);

            b.GetComponent<WormBulletToxic>().SetData(bulletSpeed);
        }
        
        public override void OnEnd()
        {
            StopAllCoroutines();
        }
    }
}
