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
    public class WormBossHeadLaser : EnemyAction
    {
        public SharedGameObject firePoint;
        
        public SharedGameObject lazerPrefab;
        private SafeGameObjectPool pool;

        private GameObject laserInstance;
        
        public override void OnStart()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(lazerPrefab.Value, 1, 3);

            StartCoroutine(Shoot());
        }
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Running;
        }
        
        public IEnumerator Shoot()
        {
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < 1; i++)
            {
                SpawnLazer();
                yield return null;
            }
        }
        
        void SpawnLazer()
        {
            if (laserInstance != null)
            {
                pool.Recycle(laserInstance);
                laserInstance = null;
            }
            
            laserInstance = pool.Allocate();
            
            // laser.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
            //     enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
            //     gameObject, gameObject.GetComponent<ICanDealDamage>(), 50f);

            Vector3 dir = transform.forward.normalized;
            Quaternion rotation = Quaternion.LookRotation(dir);
            laserInstance.transform.parent = firePoint.Value.transform;
            laserInstance.transform.position = firePoint.Value.transform.position;
            laserInstance.transform.rotation = rotation;
            // laser.GetComponent<WormBulletLazer>().SetData(this.gameObject , dir , player , maxRange , damageInterval);
        }
        
        public override void OnEnd()
        {
            if (laserInstance != null)
            {
                pool.Recycle(laserInstance);
                laserInstance = null;
            }
            
            StopAllCoroutines();
        }
    }
}
