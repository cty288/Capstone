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
    public class WormBossHeadRapidFire : EnemyAction
    {
        public SharedGameObject firePoint;
        
        public SharedGameObject rapidFireBulletPrefab;
        private SafeGameObjectPool pool;

        private GameObject player;

        public float bulletInterval = 1f;
        public float bulletSpeed = 10f;
        public int bulletCount = 5;
        private float bulletAccuracy;

        public override void OnStart()
        {
            pool = GameObjectPoolManager.Singleton.CreatePool(rapidFireBulletPrefab.Value, 1, 3);
            
            player = GetPlayer();

            StartCoroutine(Shoot());
        }
        
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Running;
        }
        
        public IEnumerator Shoot()
        {
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                for (int i = 0; i < bulletCount; i++)
                {
                    SpawnBullet();
                }
                yield return new WaitForSeconds(bulletInterval);
            }
        }
        
        void SpawnBullet()
        {
            GameObject b = pool.Allocate();
            
            // Calculate a random rotation offset within a specified range
            float randomAngle = Random.Range(-10, 10);
            Quaternion randomRotation = Quaternion.Euler(randomAngle, randomAngle, 0);
            
            b.transform.position = firePoint.Value.transform.position;
            b.transform.rotation = firePoint.Value.transform.rotation * randomRotation;
            
            // b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
            //     enemyEntity.GetCustomDataValue<int>("attack", "bulletDamage"),
            //     gameObject, gameObject.GetComponent<ICanDealDamage>() , 50f);

            b.GetComponent<WormBullet>().SetData(bulletSpeed, player, bulletAccuracy);
        }
        
        public override void OnEnd()
        {
            StopAllCoroutines();
        }
    }
}
