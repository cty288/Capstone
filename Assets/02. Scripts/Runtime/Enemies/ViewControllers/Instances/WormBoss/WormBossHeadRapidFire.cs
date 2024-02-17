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
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
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
        private TaskStatus taskStatus;
        public SharedGameObject firePoint;
        
        public SharedGameObject rapidFireBulletPrefab;
        private SafeGameObjectPool pool;

        private GameObject player;

        public float bulletInterval = 1f;
        public float bulletSpeed = 10f;
        public int bulletCount = 5;
        private float bulletAccuracy;

        public float duration = 5f;

        public override void OnStart()
        {
            taskStatus = TaskStatus.Running;
            pool = GameObjectPoolManager.Singleton.CreatePool(rapidFireBulletPrefab.Value, 1, 3);
            
            player = GetPlayer();

            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }
        
        private async UniTask SkillExecute()
        {
            await UniTask.WaitForSeconds(2f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

            float timer = duration;
            float lastShootTime = 0f;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                
                if(lastShootTime + bulletInterval < Time.time)
                {
                    lastShootTime = Time.time;
                    for (int i = 0; i < bulletCount; i++)
                    {
                        SpawnBullet();
                    }
                }
                await UniTask.Yield(PlayerLoopTiming.Update,
                    cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            }
            await UniTask.WaitForSeconds(1f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            taskStatus = TaskStatus.Success;
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
        }
    }
}
