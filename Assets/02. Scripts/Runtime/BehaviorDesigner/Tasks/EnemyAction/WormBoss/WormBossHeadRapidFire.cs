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
    public class WormBossHeadRapidFire : EnemyAction<WormBossEntity>
    {
        private TaskStatus taskStatus;
        public SharedGameObject firePoint;
        
        public SharedGameObject rapidFireBulletPrefab;
        private SafeGameObjectPool pool;

        private GameObject player;
        
        private float bulletSpeed;
        private int bulletCountPerWave;
        private float bulletAccuracy;
        private int bulletDamage;
        private int bulletWaves;
        private float bulletWaveInterval;
        private float bulletRange;

        public override void OnStart()
        {
            base.OnStart(); 
            
            taskStatus = TaskStatus.Running;
            pool = GameObjectPoolManager.Singleton.CreatePool(rapidFireBulletPrefab.Value, 30, 50);
            
            player = GetPlayer();
            
            bulletSpeed = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletSpeed");
            bulletCountPerWave = enemyEntity.GetCustomDataValue<int>("rapidFire", "bulletCountPerWave");
            bulletAccuracy = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletAccuracy");
            bulletDamage = enemyEntity.GetCustomDataValue<int>("rapidFire", "bulletDamage");
            bulletWaves = enemyEntity.GetCustomDataValue<int>("rapidFire", "bulletWaves");
            bulletWaveInterval = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletWaveInterval");
            bulletRange = enemyEntity.GetCustomDataValue<float>("rapidFire", "bulletRange");

            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }
        
        private async UniTask SkillExecute()
        {
            await UniTask.WaitForSeconds(3f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

            for (int i = 0; i < bulletWaves; i++)
            {
                await RapidFire();
            }
            
            // float timer = duration;
            // float lastShootTime = 0f;
            // while (timer > 0)
            // {
            //     timer -= Time.deltaTime;
            //     
            //     if(lastShootTime + bulletWaveInterval < Time.time)
            //     {
            //         lastShootTime = Time.time;
            //         for (int i = 0; i < bulletCountPerWave; i++)
            //         {
            //             SpawnBullet();
            //         }
            //     }
            //     await UniTask.Yield(PlayerLoopTiming.Update,
            //         cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            // }
            // await UniTask.WaitForSeconds(1f,
            //     cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            taskStatus = TaskStatus.Success;
        }
        
        void SpawnBullet()
        {
            GameObject b = pool.Allocate();
            
            // Calculate a random rotation offset within a specified range
            float randomAngle = Random.Range(-10, 10);
            Quaternion randomRotation = Quaternion.Euler(randomAngle, randomAngle, 0);
            Vector3 randomPointAroundPlayer = player.transform.position + Random.insideUnitSphere * 10;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPointAroundPlayer + Random.insideUnitSphere * 10, out hit, 100f, NavMesh.AllAreas))
            {
                randomPointAroundPlayer = hit.position + new Vector3(0,1,0);
            }
            b.transform.position = firePoint.Value.transform.position;
            b.transform.LookAt(randomPointAroundPlayer);
            
            b.GetComponent<IBulletViewController>().Init(enemyEntity.CurrentFaction.Value,
                bulletDamage,
                gameObject, gameObject.GetComponent<ICanDealDamage>() , bulletRange);

            b.GetComponent<WormBossHeadMine>().SetData(bulletSpeed, player, randomPointAroundPlayer);
        }
        
        private async UniTask RapidFire()
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnBullet();
            }
            await UniTask.WaitForSeconds(0,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
        }
    }
}
